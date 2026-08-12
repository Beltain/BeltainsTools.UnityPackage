using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.DataStructures
{
    public class Observable<T>
    {
        private T m_Value;

        /// <summary>Event that is invoked when the value changes.<br/>The first parameter is the <b>old value</b>, and the second parameter is the <b>new value</b>.</summary>
        public event System.Action<T, T> ValueChangedEvent;

        public T Value
        {
            get => m_Value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(m_Value, value))
                    return;

                T oldValue = m_Value;
                m_Value = value;
                ValueChangedEvent?.Invoke(oldValue, m_Value);
            }
        }

        public Observable(T initialValue = default)
        {
            m_Value = initialValue;
        }

        public static implicit operator T(Observable<T> observable) => observable.Value;
        public static implicit operator Observable<T>(T value) => new Observable<T>(value);

        public override string ToString() => m_Value?.ToString() ?? "null";


        public void SubscribeAndInheritCurrentValue(System.Action<T, T> listenerCallback)
        {
            listenerCallback.Invoke(m_Value, m_Value);
            Subscribe(listenerCallback);
        }

        public void Subscribe(System.Action<T, T> listenerCallback)
        {
            ValueChangedEvent += listenerCallback;
        }

        public void Unsubscribe(System.Action<T, T> listenerCallback)
        {
            ValueChangedEvent -= listenerCallback;
        }
    }
}
