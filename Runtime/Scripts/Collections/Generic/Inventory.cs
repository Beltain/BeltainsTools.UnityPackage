using BeltainsTools.EventHandling;
using BeltainsTools.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Collections.Generic
{
    [System.Serializable]
    public class Inventory<TItemType> : IEnumerable<Inventory<TItemType>.ItemStack>, BeltainsTools.Serialization.IDataSaver<Inventory<TItemType>.SaveData> where TItemType : System.Enum
    {
        [SerializeField]
        private List<ItemStack> m_ItemStacks = new List<ItemStack>();
        /// <summary>This is for tracking what items we've had in the inventory at any point in time, even if we don't have them currently</summary>
        
        private HashSet<TItemType> m_ItemHistory = new HashSet<TItemType>();

        public IReadOnlyCollection<ItemStack> ItemStacks => m_ItemStacks;
        public IReadOnlyCollection<TItemType> ItemHistory => m_ItemHistory;

        public bool IsEmpty => m_ItemStacks.Count == 0;

        [System.NonSerialized]
        public BEvent<Inventory<TItemType>> StacksChangedEvent;
        [System.NonSerialized]
        public BEvent<Inventory<TItemType>> ItemHistoryChangedEvent;


        [System.Serializable]
        public struct ItemStack
        {
            public TItemType Identifier;
            public int Count;

            public ItemStack(TItemType itemType) : this(itemType, 1) { }
            public ItemStack(TItemType itemType, int count)
            {
                Identifier = itemType;
                Count = count;
            }
        }

        public class SaveData : BeltainsTools.Serialization.SaveData
        {
            public List<ItemStack> m_ItemStacks = new List<ItemStack>();
            public List<TItemType> m_ItemHistory = new List<TItemType>();
        }


        public Inventory() { }
        public Inventory(Inventory<TItemType> other) : this()
        {
            CopyFrom(other);
        }



        public IEnumerator<ItemStack> GetEnumerator()
            => ((IEnumerable<ItemStack>)m_ItemStacks).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => ((System.Collections.IEnumerable)m_ItemStacks).GetEnumerator();


        public void CopyFrom(Inventory<TItemType> otherInventory)
        {
            RemoveAllItems(andItemHistory: true);
            foreach (TItemType item in otherInventory.m_ItemHistory)
                AddToItemHistory(item);
            AddItems(otherInventory);
        }


        public bool HasEverHadItem(TItemType itemID)
            => m_ItemHistory.Contains(itemID);
        public bool HasItem(TItemType itemID)
            => HasItem(itemID, out int _);
        public bool HasItem(TItemType itemID, out int numAvailable)
        {
            for (int i = 0; i < m_ItemStacks.Count; i++)
            {
                if (m_ItemStacks[i].Identifier.Equals(itemID))
                {
                    numAvailable = m_ItemStacks[i].Count;
                    d.AssertFormat(numAvailable > 0,
                        "We are meant to strip empty item stacks from item inventories yet we have somehow missed one ({0})! FIX ME!!!", itemID);
                    return true;
                }
            }

            numAvailable = 0;
            return false;
        }


        /// <returns>If we were able to remove any</returns>
        /// <param name="numRemoved">The number of items we were able to remove (will be 0 if we couldn't remove any)</param>
        public bool TryRemoveItems(TItemType itemID, int numToRemove, out int numRemoved)
        {
            numRemoved = 0;
            if (numToRemove == 0)
                return false;

            if (!HasItem(itemID, out int numAvailable))
                return false;

            numRemoved = Mathf.Min(numToRemove, numAvailable);
            RemoveItems(itemID, numRemoved);
            return true;
        }

        public void RemoveItems(TItemType itemID, int numToRemove)
        {
            if (numToRemove == 0)
                return;

            for (int i = 0; i < m_ItemStacks.Count; i++)
            {
                if (m_ItemStacks[i].Identifier.Equals(itemID))
                {
                    ItemStack stack = m_ItemStacks[i];
                    stack.Count = Mathf.Max(0, stack.Count - numToRemove);
                    m_ItemStacks[i] = stack;

                    if (m_ItemStacks[i].Count == 0)
                        m_ItemStacks.RemoveAt(i); // strip empty stacks to keep things clean

                    StacksChangedEvent.Invoke(this);
                    return;
                }
            }
        }

        public void AddItems(Inventory<TItemType> otherInventory)
        {
            foreach (ItemStack stack in otherInventory.m_ItemStacks)
                AddItems(stack);
        }

        public void AddItems(ItemStack itemStack) => AddItems(itemStack.Identifier, itemStack.Count);
        public void AddItems(TItemType itemID, int numToGive)
        {
            if (numToGive <= 0)
                return;

            for (int i = 0; i < m_ItemStacks.Count; i++)
            {
                if (m_ItemStacks[i].Identifier.Equals(itemID))
                {
                    ItemStack stack = m_ItemStacks[i];
                    stack.Count += numToGive;
                    m_ItemStacks[i] = stack;

                    StacksChangedEvent.Invoke(this);
                    return;
                }
            }

            m_ItemStacks.Add(new ItemStack(itemID, numToGive));
            AddToItemHistory(itemID);

            StacksChangedEvent.Invoke(this);
        }

        public void InitialiseHistory()
        {
            for (int i = 0; i < m_ItemStacks.Count; i++)
                m_ItemHistory.Add(m_ItemStacks[i].Identifier);
            ItemHistoryChangedEvent.Invoke(this);
        }

        public void AddToItemHistory(TItemType itemID)
        {
            m_ItemHistory.Add(itemID);
            ItemHistoryChangedEvent.Invoke(this);
        }

        private void ClearItemHistory()
        {
            m_ItemHistory.Clear();
            ItemHistoryChangedEvent.Invoke(this);
        }

        private void RemoveAllItems(bool andItemHistory = false)
        {
            m_ItemStacks.Clear();
            StacksChangedEvent.Invoke(this);
            if (andItemHistory)
                ClearItemHistory();
        }

        public bool Serialize(out SaveData data)
        {
            data = new SaveData() { };
            foreach (ItemStack itemStack in m_ItemStacks)
                data.m_ItemStacks.Add(itemStack);
            foreach (TItemType itemID in m_ItemHistory)
                data.m_ItemHistory.Add(itemID);
            return true;
        }

        public bool Deserialize(in SaveData data)
        {
            if (data.m_ItemStacks == null)
                return false; // how did this even happen?

            RemoveAllItems(andItemHistory: true);
            foreach (TItemType itemID in data.m_ItemHistory)
                AddToItemHistory(itemID);
            foreach (ItemStack itemStack in data.m_ItemStacks)
                AddItems(itemStack);
            return true;
        }
    }
}
