using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using System.Reflection;
using System;

namespace BeltainsTools.Pooling
{
    public class Pooler
    {
        public static Pooler Inst = null;

        /// <summary>Existing pools for prefab <see cref="GameObject"/> instance IDs</summary>
        Dictionary<int, ComponentPool> ExistingPoolsLookup { get; set; }
        /// <summary>Iteratable list containing same data as <see cref="ExistingPoolsLookup"/></summary>
        List<ComponentPool> ExistingPools { get; set; }
        /// <summary>The transform which will hold all inactive pooled objects</summary>
        public Transform RecyclingBin { get; private set; }
        public RectTransform RecyclingBin_UI { get; private set; }
        /// <summary>The transform which will hold all pre-pooled parent objects</summary>
        public Transform TemplateBin { get; private set; }
        public RectTransform TemplateBin_UI { get; private set; }

        public Pooler(Transform parent)
        {
            d.Assert(Inst == null, "Attempting to create a second pooler while pooler is a singleton!");

            InitBins(parent);

            Inst = this;
            ExistingPoolsLookup = new Dictionary<int, ComponentPool>();
            ExistingPools = new List<ComponentPool>();
        }

        public T GetPrePooledPrefab<T>(T prefab) where T : Component
        {
            return (T)GetOrCreatePoolFromPrefab(prefab).m_PrePooledPrefab;
        }

        void InitBins(Transform binParent)
        {
            RecyclingBin = new GameObject("_Recycling").transform;
            TemplateBin = new GameObject("_Templates").transform;
            RecyclingBin.parent = binParent;
            TemplateBin.parent = binParent;

            Canvas uiBinCanvas = new GameObject("_UIBins", typeof(Canvas)).GetComponent<Canvas>();
            uiBinCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiBinCanvas.transform.SetParent(binParent);
            RecyclingBin_UI = new GameObject("_Recycling_UI", typeof(RectTransform)).transform as RectTransform;
            TemplateBin_UI = new GameObject("_Templates_UI", typeof(RectTransform)).transform as RectTransform;
            RecyclingBin_UI.SetParent(uiBinCanvas.transform as RectTransform);
            RecyclingBin_UI.anchorMin = Vector2.zero;
            RecyclingBin_UI.anchorMax = Vector2.one;
            RecyclingBin_UI.anchoredPosition = Vector2.zero;
            RecyclingBin_UI.sizeDelta = Vector2.zero;
            TemplateBin_UI.SetParent(uiBinCanvas.transform as RectTransform);
            TemplateBin_UI.anchorMin = Vector2.zero;
            TemplateBin_UI.anchorMax = Vector2.one;
            TemplateBin_UI.anchoredPosition = Vector2.zero;
            TemplateBin_UI.sizeDelta = Vector2.zero;
        }

        ComponentPool GetOrCreatePoolFromPrefab<T>(T prefab) where T : Component
        {
            int prefabInstanceID = prefab.gameObject.GetInstanceID();
            if (Inst.ExistingPoolsLookup.ContainsKey(prefabInstanceID))
                return Inst.ExistingPoolsLookup[prefabInstanceID];

            bool uiPool = ComponentPool.GetComponentUsesUIPool(prefab);

            Inst.ExistingPoolsLookup[prefabInstanceID] = new ComponentPool(
                prefab, 
                uiPool ? Inst.TemplateBin_UI : Inst.TemplateBin,
                uiPool ? Inst.RecyclingBin_UI : Inst.RecyclingBin
                );
            ExistingPools.Add(Inst.ExistingPoolsLookup[prefabInstanceID]);

            return Inst.ExistingPoolsLookup[prefabInstanceID];
        }

        ComponentPool GetPoolForSpawnedObject<T>(T spawnedObject) where T : Component
        {
            int spawnedObjectID = spawnedObject.gameObject.GetInstanceID();
            for (int i = 0; i < ExistingPools.Count; i++)
            {
                if (ExistingPools[i].IsPoolForObject(spawnedObjectID))
                    return ExistingPools[i];
            }
            return null;
        }

        public static T Spawn<T>(T prefab) where T : Component => Spawn(prefab, prefab.transform.position, prefab.transform.rotation);
        public static T Spawn<T>(T prefab, Transform parent) where T : Component => Spawn(prefab, parent, false);
        public static T Spawn<T>(T prefab, Transform parent, bool instantiateInWorldSpace) where T : Component
        {
            if (parent == null)
                return Spawn(prefab);
            return Spawn(prefab, prefab.transform.position, prefab.transform.rotation, parent, instantiateInWorldSpace);
        }
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component => Spawn(prefab, position, rotation, null);
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent, bool instantiateInWorldSpace = false) where T : Component
        {
            ComponentPool poolForObject = Inst.GetOrCreatePoolFromPrefab(prefab);
            return poolForObject.SpawnPooledObject(position, rotation, parent, instantiateInWorldSpace).GetComponent<T>();
        }

        public static void Recycle<T>(T spawnedObject) where T : Component
        {
            ComponentPool poolForObject = Inst.GetPoolForSpawnedObject(spawnedObject);
            d.Assert(poolForObject != null, "Tried to recycle a non-pooled object! No bueno my guy!");
            poolForObject.RecyclePooledObject(spawnedObject);
        }

        public static void RecycleAllActiveObjects()
        {
            for (int i = 0; i < Inst.ExistingPools.Count; i++)
            {
                Inst.ExistingPools[i].RecycleAllPooledObjects();
            }
        }
    }

    internal class PooledTypeMessager
    {
        internal enum MessageTypes
        {
            PrePool,
            OnPool,
            OnDepool,
            OnSpawn,
            OnRecycle
        }

        static Dictionary<int, PooledTypeMessager> s_TypeHashMessagerLookup = null;
        Dictionary<MessageTypes, Action<Component>[]> m_MessageDelegateLookup;
        static string[] s_MessageTypesAsText = null;
        static HashSet<Component> s_Unmessageables = new HashSet<Component>();

        public PooledTypeMessager(Type type)
        {
            if (s_MessageTypesAsText == null)
            {
                Array messageTypes = Enum.GetNames(typeof(MessageTypes));
                s_MessageTypesAsText = new string[messageTypes.Length];
                for (int i = 0; i < s_MessageTypesAsText.Length; i++)
                    s_MessageTypesAsText[i] = messageTypes.GetValue(i).ToString();
            }

            m_MessageDelegateLookup = new Dictionary<MessageTypes, Action<Component>[]>();
            for (int i = 0; i < s_MessageTypesAsText.Length; i++)
                m_MessageDelegateLookup.Add((MessageTypes)i, GetDelegatesFor(type, s_MessageTypesAsText[i]));
        }

        internal static void BlockMessagesTo<T>(T parentComponent) where T : Component => SetMessagesBlockedTo(parentComponent, true);
        internal static void UnblockMessagesTo<T>(T parentComponent) where T : Component => SetMessagesBlockedTo(parentComponent, false);
        static void SetMessagesBlockedTo<T>(T parentComponent, bool blocked) where T : Component
        {
            foreach (Component component in parentComponent.GetComponentsInChildren<Component>(true))
            {
                if (blocked)
                    s_Unmessageables.Add(component);
                else
                    s_Unmessageables.Remove(component);
            }
        }

        /// <summary>Sends messages for each component in this gameObject (including components in it's children)</summary>
        internal static void SendMessage(MessageTypes messageType, GameObject gameObject)
        {
            foreach(Component component in gameObject.GetComponentsInChildren<Component>(true))
                SendMessage(messageType, component);
        }

        /// <summary>Sends a message of the specified type to the provided component and all its children</summary>
        internal static void SendMessage(MessageTypes messageType, Component component)
        {
            if (s_Unmessageables.Contains(component))
                return;

            if (s_TypeHashMessagerLookup == null)
                s_TypeHashMessagerLookup = new Dictionary<int, PooledTypeMessager>();

            int componentTypeHash = component.GetType().GetHashCode();
            if (!s_TypeHashMessagerLookup.TryGetValue(componentTypeHash, out PooledTypeMessager messager))
            {
                messager = new PooledTypeMessager(component.GetType());
                s_TypeHashMessagerLookup.Add(componentTypeHash, messager);
            }

            messager.SendMessageFromMessager(messageType, component);
        }

        void SendMessageFromMessager(MessageTypes messageType, Component component)
        {
            for (int i = 0; i < m_MessageDelegateLookup[messageType].Length; i++)
            {
                m_MessageDelegateLookup[messageType][i].Invoke(component);
            }
        }


        static MethodInfo _s_genericCreateDelegateHelper;
        static List<Action<Component>> _s_TempDelegatesList;
        static List<MethodInfo> _s_TempDelegateMethodInfoList;
        static Action<Component>[] GetDelegatesFor(Type type, string methodName)
        {
            if (_s_TempDelegatesList == null)
            {
                _s_TempDelegatesList = new List<Action<Component>>();
                _s_TempDelegateMethodInfoList = new List<MethodInfo>();
            }
            _s_TempDelegatesList.Clear();
            _s_TempDelegateMethodInfoList.Clear();

            Type typeTemp = type;
            do
            {
                MethodInfo methodInfo = typeTemp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (methodInfo != null && methodInfo.ReturnType == typeof(void) && methodInfo.GetParameters().Length == 0)
                    _s_TempDelegateMethodInfoList.Add(methodInfo);
                typeTemp = typeTemp.BaseType;
            }
            while (typeTemp != null && typeTemp != typeof(Component));

            int methodListSize = _s_TempDelegateMethodInfoList.Count;
            for (int i = 0; i < methodListSize; i++)
            {
                MethodInfo methInfo = _s_TempDelegateMethodInfoList[methodListSize - 1 - i];

                if (_s_genericCreateDelegateHelper == null)
                    _s_genericCreateDelegateHelper = typeof(PooledTypeMessager).GetMethod("CreateDelegateHelper", BindingFlags.Static | BindingFlags.NonPublic);

                MethodInfo constructedHelper = _s_genericCreateDelegateHelper.MakeGenericMethod(methInfo.DeclaringType);
                _s_TempDelegatesList.Add((Action<Component>)constructedHelper.Invoke(null, new object[] { methInfo }));
            }
            return _s_TempDelegatesList.ToArray();
        }

        /// <summary>Method for adapting each component to work with the component delegate, as it would expect the method in that child class and not just want to execute on the base 'component' class</summary>
        static Action<Component> CreateDelegateHelper<T>(MethodInfo method) where T : class
        {
            var action = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), method);
            return (Component component) => action(component as T);
        }
    }


    public static class Extensions
    {
        public static T Spawn<T>(this T prefab) where T : Component
            => Pooler.Spawn(prefab);
        public static T Spawn<T>(this T prefab, Transform parent) where T : Component
            => Pooler.Spawn(prefab, parent);
        public static T Spawn<T>(this T prefab, Transform parent, bool instantiateInWorldSpace) where T : Component
            => Pooler.Spawn(prefab, parent, instantiateInWorldSpace);
        public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
            => Pooler.Spawn(prefab, position, rotation);
        public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
            => Pooler.Spawn(prefab, position, rotation, parent);

        public static void Recycle<T>(this T pooledObject) where T : Component
            => Pooler.Recycle(pooledObject);
    }




    class ComponentPool : Pool<Component>
    {
        public ComponentPool(Component prefab, Transform templateBin, Transform recyclingBin, int startingPoolSize = 1) : base(prefab, templateBin, recyclingBin, startingPoolSize) { }
    }

    class Pool<T> where T : Component
   {
        public T m_Prefab;
        public T m_PrePooledPrefab;
        public HashSet<T> m_PooledObjects;
        public HashSet<int> m_PooledObjectGameObjectIDs;
        public Stack<T> m_AvailableObjects;

        int m_ActivePoolablesCount = 0;
        bool m_IsUIPool;

        Transform m_TemplateBin;
        Transform m_RecyclingBin;

        public Pool(T prefab, Transform templateBin, Transform recyclingBin, int startingPoolSize = 1)
        {
            m_Prefab = prefab;

            m_TemplateBin = templateBin;
            m_RecyclingBin = recyclingBin;

            m_IsUIPool = GetComponentUsesUIPool(prefab);
            m_PrePooledPrefab = GameObject.Instantiate(m_Prefab, m_TemplateBin);
            m_PrePooledPrefab.name = $"{m_Prefab.name}_Template";
            PooledTypeMessager.SendMessage(PooledTypeMessager.MessageTypes.PrePool, m_PrePooledPrefab.gameObject);
            PooledTypeMessager.BlockMessagesTo(m_PrePooledPrefab); // don't send any more messages as this is the template and shouldn't be receiving pool/spawn/etc requests
            m_PrePooledPrefab.gameObject.SetActive(false);

            m_PooledObjects = new HashSet<T>();
            m_PooledObjectGameObjectIDs = new HashSet<int>();
            m_AvailableObjects = new Stack<T>();
            IncreasePool(startingPoolSize);
        }

        public static bool GetComponentUsesUIPool(T component)
            => component.transform is RectTransform;

        /// <summary>
        /// Get whether or not the referenced GameObject instance belongs to this pool
        /// </summary>
        /// <param name="instanceID">The GameObject instance ID of the tested object</param>
        public bool IsPoolForObject(int instanceID)
        {
            return m_PooledObjectGameObjectIDs.Contains(instanceID);
        }

        /// <summary>Get a pooled object, and increase the pool size if none are available</summary>
        public T GetRecycledOrNewPooledObject()
        {
            if (m_AvailableObjects.Count == 0)
                IncrementPool();

            return m_AvailableObjects.Pop();
        }

        /// <summary>Set the given pooled object active, set it's transform details and send relavent messages</summary>
        public T SpawnPooledObject(Vector3 position, Quaternion rotation, Transform parent, bool instantiateInWorldSpace = false)
        {
            T pooledObject = GetRecycledOrNewPooledObject();

            Transform pooledTrans = pooledObject.transform;

            //the original object's position and rotation are used for the cloned object's local position and rotation,
            //or its world position and rotation if the instantiateInWorldSpace parameter is true.
            //If the position and rotation are specified, they are used as the object's position and rotation in world space.
            //https://docs.unity3d.com/ScriptReference/Object.Instantiate.html

            if (m_IsUIPool)
            {
                RectTransform pooledRectTrans = pooledTrans as RectTransform;
                pooledRectTrans.SetRectTransformParent(parent as RectTransform);
                if (instantiateInWorldSpace)
                    pooledRectTrans.SetPositionAndRotation(position, rotation);
                else
                {
                    pooledRectTrans.anchoredPosition = position;
                    pooledRectTrans.localRotation = rotation;
                }
            }
            else
            {
                pooledTrans.SetParent(parent);
                if (instantiateInWorldSpace)
                    pooledTrans.SetPositionAndRotation(position, rotation);
                else
                    pooledTrans.SetLocalPositionAndRotation(position, rotation);
            }

            pooledObject.gameObject.name = m_Prefab.name;
            pooledObject.gameObject.SetActive(true);
            PooledTypeMessager.SendMessage(PooledTypeMessager.MessageTypes.OnSpawn, pooledObject.gameObject);
            m_ActivePoolablesCount++;

            return pooledObject;
        }

        public void RecycleAllPooledObjects()
        {
            foreach (T pooledObject in m_PooledObjects)
                RecyclePooledObject(pooledObject);
        }

        /// <summary>Set the given pooled object inactive, set it's transform details and send relavent messages</summary>
        public void RecyclePooledObject(T pooledObject)
        {
            if (!pooledObject.gameObject.activeSelf)
                return;

            if(m_IsUIPool)
                (pooledObject.transform as RectTransform).SetRectTransformParent(m_RecyclingBin as RectTransform);
            else
                pooledObject.transform.SetParent(m_RecyclingBin);

            m_ActivePoolablesCount--;
            PooledTypeMessager.SendMessage(PooledTypeMessager.MessageTypes.OnRecycle, pooledObject.gameObject);

            pooledObject.gameObject.SetActive(false);
            m_AvailableObjects.Push(pooledObject);
        }


        /// <summary>Instantiate more objects in this pool in an inactive state</summary>
        public void IncreasePool(int increaseSize)
        {
            for (int i = 0; i < increaseSize; i++)
                IncrementPool();
        }

        /// <summary>Instantiate a new object in the pool in an inactive state</summary>
        public void IncrementPool()
        {
            T newlyPooledObject;
            if(m_PrePooledPrefab.transform is RectTransform)
                newlyPooledObject = GameObject.Instantiate(m_PrePooledPrefab, m_RecyclingBin);
            else
                newlyPooledObject = GameObject.Instantiate(m_PrePooledPrefab, m_RecyclingBin);
            newlyPooledObject.gameObject.SetActive(false);
            m_PooledObjects.Add(newlyPooledObject);
            m_PooledObjectGameObjectIDs.Add(newlyPooledObject.gameObject.GetInstanceID());
            m_AvailableObjects.Push(newlyPooledObject);
            PooledTypeMessager.SendMessage(PooledTypeMessager.MessageTypes.OnPool, newlyPooledObject.gameObject);
        }

        /// <summary>Remove all incactive pooled objects from this pool</summary>
        /// <param name="trim01">The percentage of 'fat'(inactive poolables) that should be removed from the pool</param>
        public void CullPool(float trim01 = 1f)
        {
            T[] inactivePoolables = m_PooledObjects.Where(r => !r.gameObject.activeSelf).ToArray();
            for (int i = 0; i < inactivePoolables.Length * trim01; i++)
                DecrementPool(inactivePoolables[i]);
        }

        /// <summary>Destroy a pooled object and remove it from this pool</summary>
        public void DecrementPool(T pooledObject)
        {
            d.Assert(!pooledObject.gameObject.activeSelf, "Tried to depool an active poolable! No beuno my dude!");
            if (m_PooledObjects.Remove(pooledObject))
            {
                m_PooledObjectGameObjectIDs.Remove(pooledObject.gameObject.GetInstanceID());
                PooledTypeMessager.SendMessage(PooledTypeMessager.MessageTypes.OnDepool, pooledObject.gameObject);
                GameObject.Destroy(pooledObject);
            }
        }
    }
}
