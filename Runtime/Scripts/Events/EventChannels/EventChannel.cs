using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.EventHandling
{
    /// <summary>
    /// Simple event channel using scriptable objects to allow for decoupled event broadcasting.
    /// </summary>
    public abstract class EventChannelBase : ScriptableObject
    {
        public const string k_Path_CreateAssetMenu = BTInternal.PackageData.Paths.CreateAssetMenu.k_Events + "EventChannels/";

        protected readonly HashSet<object> m_Observers = new HashSet<object>();

        protected void RegisterListener(object observer)
        {
            d.Assert(observer != null, "Observer cannot be null");
            m_Observers.Add(observer);
        }

        protected void UnregisterListener(object observer)
        {
            d.Assert(observer != null, "Observer cannot be null");
            m_Observers.Remove(observer);
        }
    }

    /// <inheritdoc cref="EventChannelBase"/>
    public abstract class EventChannel<T> : EventChannelBase
    {
        public T LastValue { get; private set; } = default;

        public void Invoke(T value)
        {
            LastValue = value;
            foreach (IListener observer in m_Observers)
                observer.OnEventRaised(value);
        }

        public void RegisterListener(IListener observer) => base.RegisterListener(observer);
        public void UnregisterListener(IListener observer) => base.UnregisterListener(observer);

        public interface IListener
        {
            void OnEventRaised(T value);
        }
    }

    /// <inheritdoc cref="EventChannelBase"/>
    [CreateAssetMenu(fileName = "new EventChannel", menuName = k_Path_CreateAssetMenu + "Channel/Empty")]
    public class EventChannel : EventChannelBase
    {
        public void Invoke()
        {
            foreach (IListener observer in m_Observers)
                observer.OnEventRaised();
        }

        public void RegisterListener(IListener observer) => base.RegisterListener(observer);
        public void UnregisterListener(IListener observer) => base.UnregisterListener(observer);

        public interface IListener
        {
            void OnEventRaised();
        }
    }
}
