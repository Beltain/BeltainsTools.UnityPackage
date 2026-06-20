using UnityEngine;
using UnityEngine.Events;

namespace BeltainsTools.EventHandling
{
    [AddComponentMenu(EventChannelBase.k_Path_CreateAssetMenu + "EventChannelListener", order: -1)]
    public class EventChannelListener : MonoBehaviour, EventChannel.IListener
    {
        [SerializeField]
        protected EventChannel m_Channel;
        [SerializeField]
        protected UnityEvent m_Response;

        [System.NonSerialized]
        public BEvent ResponseEvent;

        protected virtual void OnEventRaised() { }

        protected virtual void OnEnable()
        {
            m_Channel?.RegisterListener(this);
        }

        protected virtual void OnDisable()
        {
            m_Channel?.UnregisterListener(this);
        }

        void EventChannel.IListener.OnEventRaised()
        {
            ResponseEvent.Invoke();
            m_Response?.Invoke();
            OnEventRaised();
        }
    }

    public abstract class EventChannelListener<T> : MonoBehaviour, EventChannel<T>.IListener
    {
        [SerializeField]
        protected EventChannel<T> m_Channel;
        [SerializeField]
        protected UnityEvent<T> m_Response;

        [System.NonSerialized]
        public BEvent<T> ResponseEvent;

        protected virtual void OnEventRaised(T value) { }

        protected virtual void OnEnable()
        {
            m_Channel?.RegisterListener(this);
        }

        protected virtual void OnDisable()
        {
            m_Channel?.UnregisterListener(this);
        }

        void EventChannel<T>.IListener.OnEventRaised(T value)
        {
            ResponseEvent.Invoke(value);
            m_Response?.Invoke(value);
            OnEventRaised(value);
        }
    }
}
