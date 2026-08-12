using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.DataStructures
{
    public class ObservableCollection<T> : IEnumerable<T>
    {
        readonly ICollection<T> m_Collection;

        public int Count => m_Collection.Count;
        public IReadOnlyCollection<T> Collection => (IReadOnlyCollection<T>)m_Collection;

        public event Action<ObservableCollection<T>> ChangedEvent;
        public event Action<T> ItemAddedEvent;
        public event Action<T> ItemRemovedEvent;


        public ObservableCollection()
        {
            m_Collection = new List<T>();
        }

        public ObservableCollection(ICollection<T> backingCollection)
        {
            m_Collection = backingCollection ?? throw new ArgumentNullException(nameof(backingCollection));
        }

        public static implicit operator ObservableCollection<T>(List<T> list) => new ObservableCollection<T>(list);
        public static implicit operator ObservableCollection<T>(HashSet<T> hashset) => new ObservableCollection<T>(hashset);
        public static implicit operator ObservableCollection<T>(T[] array) => new ObservableCollection<T>(array);


        public void SubscribeAndInheritCurrentValue(Action<ObservableCollection<T>> onChangedCallback)
        {
            onChangedCallback.Invoke(this);
            Subscribe(onChangedCallback);
        }

        public void Subscribe(Action<ObservableCollection<T>> onChangedCallback)
        {
            ChangedEvent += onChangedCallback;
        }

        public void Unsubscribe(Action<ObservableCollection<T>> onChangedCallback)
        {
            ChangedEvent -= onChangedCallback;
        }


        public bool Contains(T item)
            => m_Collection.Contains(item);

        public bool Add(T item)
        {
            int oldCount = m_Collection.Count;
            m_Collection.Add(item);
            if (oldCount == m_Collection.Count)
                return false;

            ItemAddedEvent?.Invoke(item);
            ChangedEvent?.Invoke(this);
            return true;
        }

        public bool Remove(T item)
        {
            if (!m_Collection.Remove(item))
                return false;

            ItemRemovedEvent?.Invoke(item);
            ChangedEvent?.Invoke(this);
            return true;
        }

        public void Clear()
        {
            if (m_Collection.Count == 0)
                return;

            if (ItemRemovedEvent != null)
            {
                List<T> removed = new List<T>(m_Collection);
                m_Collection.Clear();
                foreach (T item in removed)
                    ItemRemovedEvent.Invoke(item);
            }
            else
            {
                m_Collection.Clear();
            }

            ChangedEvent?.Invoke(this);
        }

        public IEnumerator<T> GetEnumerator() => m_Collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
