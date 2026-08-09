using UnityEngine;

namespace BeltainsTools
{
    /// <summary>A base class for creating <see cref="Singleton{T}"/> components.</summary>
    public abstract class Singleton : MonoBehaviour { }

    /// <summary>A <see cref="Singleton"/> component that ensures only one instance of the component exists in the scene at any given time.</summary>
    /// <remarks>Does not persist between scene loads. Use <see cref="PersistentSingleton{T}"/> for that behavior.</remarks>
    [DisallowMultipleComponent]
    public abstract class Singleton<T> : Singleton where T : Singleton<T>
    {
        public static T Instance;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                d.LogError($"Multiple instances of singleton {typeof(T).Name} detected. Destroying duplicate.");
                Destroy(this.gameObject);
                return;
            }

            Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    /// <summary>
    /// A <see cref="Singleton{T}"/> that persists between scene loads. 
    /// Use this for managers or services that should exist throughout the lifetime of the application, or control their own lifetime outside of scene management.
    /// </summary>
    public abstract class PersistentSingleton<T> : Singleton<T> where T : PersistentSingleton<T>
    {
        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(this.gameObject);
        }
    }
}
