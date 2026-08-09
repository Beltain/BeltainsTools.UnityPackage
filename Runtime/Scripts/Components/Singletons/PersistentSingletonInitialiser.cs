using BeltainsTools.EventHandling;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools
{
    /// <summary>
    /// Use for ordered initialisation of <see cref="PersistentSingleton{T}"/>s
    /// </summary>
    public class PersistentSingletonInitialiser : MonoBehaviour
    {
        public static InitialisationStages s_InitStage = InitialisationStages.Uninitialised;

        [SerializeField]
        Singleton[] m_SingletonPrefabs = new Singleton[0];
        [SerializeField]
        bool m_InitialiseOnAwake = true;

        [System.NonSerialized]
        /// <summary>Triggered when all singletons have been instantiated and awoken</summary>
        public BEvent AllInitialisedEvent;
        [System.NonSerialized]
        /// <summary>Triggered when all singletons have been instantiated, awoken and started (if <see cref="m_RequireAllStarted"/> is set)</summary>
        public BEvent AllExtensivelyInitialisedEvent;


        public enum InitialisationStages
        {
            Uninitialised,
            Initialising,
            /// <summary>Initted up to and including awake methods</summary>
            Initialised,
            /// <summary>Initted up to and including start methods</summary>
            InitialisedExtensively 
        }


        public void Initialise(System.Action<bool> onCompleteCallback = null)
        {
            if (s_InitStage != InitialisationStages.Uninitialised)
            {
                CompleteInitialise(successfully: false, onCompleteCallback);
                return;
            }
            s_InitStage = InitialisationStages.Initialising;

            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitialiseCo(onCompleteCallback));
        }

        private IEnumerator InitialiseCo(System.Action<bool> onCompleteCallback = null)
        {
            Transform singletonParent = new GameObject("Singletons").transform;
            DontDestroyOnLoad(singletonParent.gameObject);

            HashSet<Singleton> inittedSingletons = new HashSet<Singleton>();
            foreach (Singleton singleton in m_SingletonPrefabs)
                inittedSingletons.Add(Instantiate(singleton, singletonParent));

            s_InitStage = InitialisationStages.Initialised;
            AllInitialisedEvent.Invoke();

            yield return new WaitForEndOfFrame(); //ensures all awake and start methods have a chane to run

            s_InitStage = InitialisationStages.InitialisedExtensively;
            AllExtensivelyInitialisedEvent.Invoke();

            CompleteInitialise(successfully: true, onCompleteCallback);
        }


        private void CompleteInitialise(bool successfully, System.Action<bool> onCompleteCallback)
        {
            Destroy(gameObject);
            onCompleteCallback?.Invoke(successfully);
        }


        private void Awake()
        {
            if (m_InitialiseOnAwake)
                Initialise();
        }
    }
}
