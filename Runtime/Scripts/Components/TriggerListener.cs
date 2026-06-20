using BeltainsTools.EventHandling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools
{
    /// <summary>
    /// A higher level version of <seealso cref="TriggerCatcher"/>, newer than <seealso cref="TriggerCatcher"/> and using BEvents.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerListener : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        protected Collider m_Collider = null;

        [SerializeField] protected UnityEngine.Events.UnityEvent TriggerEnterActions;
        [SerializeField] protected UnityEngine.Events.UnityEvent TriggerExitActions;
        [SerializeField] protected UnityEngine.Events.UnityEvent TriggerStayActions;

        [System.NonSerialized]
        public BEvent<Collider> TriggerEnterEvent;
        [System.NonSerialized]
        public BEvent<Collider> TriggerExitEvent;
        [System.NonSerialized]
        public BEvent<Collider> TriggerStayEvent;
        [System.NonSerialized]
        public BEvent<Collider, EventTypes> TriggeredEvent;

        public enum EventTypes
        {
            Stay,
            Enter,
            Exit
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnterEvent.Invoke(other);
            TriggerEnterActions?.Invoke();
            TriggeredEvent.Invoke(other, EventTypes.Enter);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExitEvent.Invoke(other);
            TriggerExitActions?.Invoke();
            TriggeredEvent.Invoke(other, EventTypes.Exit);
        }

        private void OnTriggerStay(Collider other)
        {
            TriggerStayEvent.Invoke(other);
            TriggerStayActions?.Invoke();
            TriggeredEvent.Invoke(other, EventTypes.Stay);
        }


        void PrePool()
        {
            m_Collider = GetComponent<Collider>();
            if (!m_Collider.isTrigger)
            {
                d.LogWarning($"Collider on physics sensor [{gameObject.name}] not set to trigger. Setting now... Please update [{gameObject.name}]");
                m_Collider.isTrigger = true;
            }
        }
    }
}
