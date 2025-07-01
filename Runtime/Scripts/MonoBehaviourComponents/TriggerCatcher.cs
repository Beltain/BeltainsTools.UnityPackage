using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools
{
    public class TriggerCatcher : MonoBehaviour
    {
        event System.Action<EventTypes, Collision> m_CollisionEvent;
        event System.Action<EventTypes, Collider> m_TriggerEvent;

        public bool HasSubscribers => m_CollisionEvent != null || m_TriggerEvent != null;

        public enum EventTypes
        {
            Enter,
            Stay,
            Exit
        }

        public static void SubscribeToCollisionEvent(Component component, System.Action<EventTypes, Collision> eventDelegate)
        {
            if (!(component.GetComponent<TriggerCatcher>() is TriggerCatcher triggerCatcher))
                triggerCatcher = component.gameObject.AddComponent<TriggerCatcher>();
            triggerCatcher.SubscribeToCollisionEvent(eventDelegate);
        }

        public static void SubscribeToTriggerEvent(Component component, System.Action<EventTypes, Collider> eventDelegate)
        {
            if (!(component.GetComponent<TriggerCatcher>() is TriggerCatcher triggerCatcher))
                triggerCatcher = component.gameObject.AddComponent<TriggerCatcher>();
            triggerCatcher.SubscribeToTriggerEvent(eventDelegate);
        }


        public static void Unsubscribe(Component component, System.Action<EventTypes, Collision> eventDelegate)
        {
            if (!(component.GetComponent<TriggerCatcher>() is TriggerCatcher triggerCatcher))
                return;
            triggerCatcher.Unsubscribe(eventDelegate);

            if (!triggerCatcher.HasSubscribers)
                Destroy(triggerCatcher);
        }

        public static void Unsubscribe(Component component, System.Action<EventTypes, Collider> eventDelegate)
        {
            if (!(component.GetComponent<TriggerCatcher>() is TriggerCatcher triggerCatcher))
                return;
            triggerCatcher.Unsubscribe(eventDelegate);

            if (!triggerCatcher.HasSubscribers)
                Destroy(triggerCatcher);
        }

        void SubscribeToCollisionEvent(System.Action<EventTypes, Collision> eventDelegate)
        {
            m_CollisionEvent += eventDelegate;
        }

        void SubscribeToTriggerEvent(System.Action<EventTypes, Collider> eventDelegate)
        {
            m_TriggerEvent += eventDelegate;
        }

        void Unsubscribe(System.Action<EventTypes, Collision> eventDelegate)
        {
            m_CollisionEvent -= eventDelegate;
        }

        void Unsubscribe(System.Action<EventTypes, Collider> eventDelegate)
        {
            m_TriggerEvent -= eventDelegate;
        }


        private void OnCollisionEnter(Collision collision)
            => m_CollisionEvent?.Invoke(EventTypes.Enter, collision);

        private void OnCollisionStay(Collision collision)
            => m_CollisionEvent?.Invoke(EventTypes.Stay, collision);


        private void OnCollisionExit(Collision collision)
            => m_CollisionEvent?.Invoke(EventTypes.Exit, collision);


        private void OnTriggerEnter(Collider other)
            => m_TriggerEvent?.Invoke(EventTypes.Enter, other);

        private void OnTriggerStay(Collider other)
            => m_TriggerEvent?.Invoke(EventTypes.Stay, other);

        private void OnTriggerExit(Collider other)
            => m_TriggerEvent?.Invoke(EventTypes.Exit, other);

    }

}