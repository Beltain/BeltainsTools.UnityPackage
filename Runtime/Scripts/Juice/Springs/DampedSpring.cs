using UnityEngine;
using BeltainsTools.Maths;
using System.Collections.Generic;

namespace BeltainsTools.Juice
{
    public static class DampedSpring
    {
        [System.Serializable]
        public abstract class Base
        {
            [SerializeField]
            protected float m_Frequency;
            [SerializeField, Range(0f, 1f)]
            protected float m_Damping01;

            private float m_Epsilon;

            public Base(float frequency, float damping01, float epsilon = 0.0001f)
            {
                m_Frequency = frequency;
                m_Damping01 = damping01;
                m_Epsilon = epsilon;
            }

            /// <summary>Update by standard <see cref="Time.deltaTime"/> step</summary>
            public void Update() => Update(Time.deltaTime);
            /// <summary>Update by standard <see cref="Time.fixedDeltaTime"/> step</summary>
            public void FixedUpdate() => Update(Time.fixedDeltaTime);
            public void Update(float deltaTime)
            {
                if (m_Epsilon > 0f && GetIsResting(m_Epsilon))
                    return;
                OnUpdate(deltaTime);
            }

            /// <summary>Whether or not to update the Damped Spring based on the provided epsilon</summary>
            protected abstract bool GetIsResting(float epsilon);
            protected abstract void OnUpdate(float deltaTime);
        }

        [System.Serializable]
        public abstract class Base<T> : Base where T : struct, System.IEquatable<T>
        {
            private T m_TargetValue;

            private T m_CurrentValue;
            private T m_CurrentVelocity;

            public delegate void ValueChangedDelegate(T newValue);
            [System.NonSerialized]
            public HashSet<ValueChangedDelegate> m_ValueChangedListeners = new HashSet<ValueChangedDelegate>();

            public T Current => m_CurrentValue;
            public T Target => m_TargetValue;

            [System.Obsolete("Please use Current instead!")]
            public T Value => m_CurrentValue;



            public static implicit operator T(Base<T> dampedSpring)
            {
                return dampedSpring.m_CurrentValue;
            }

            public Base(float frequency, float damping01, float epsilon = 0.0001f)
                : base(frequency, damping01, epsilon) { }
            public Base(float frequency, float damping01, T targetValue, T startValue, float epsilon = 0.0001f)
                : base (frequency, damping01, epsilon)
            {
                Set(targetValue, startValue);
            }

            /// <inheritdoc cref="Init(T, T, ValueChangedDelegate)"/>
            public void Init(T value, ValueChangedDelegate onValueChanged = null) => Init(value, value, onValueChanged);
            /// <summary>
            /// Initialise this <see cref="DampedSpring"/> with starting values and an optional callback.
            /// <para>Please ensure <see cref="DeInit"/> is called before trying to initialise again.</para>
            /// </summary>
            public void Init(T targetValue, T currentValue, ValueChangedDelegate onValueChanged = null)
            {
                if (onValueChanged != null)
                    AddListener(onValueChanged);
                Set(targetValue, currentValue);
            }

            public void DeInit()
            {
                m_ValueChangedListeners.Clear();
            }

            public void AddListener(ValueChangedDelegate callback)
            {
                m_ValueChangedListeners.Add(callback);
            }

            public void RemoveListener(ValueChangedDelegate callback)
            {
                m_ValueChangedListeners.Remove(callback);
            }

            /// <inheritdoc cref="Set(T, T)"/>
            public void Set(T value) => Set(value, value);
            /// <summary>Set the <see cref="Current"/> and <see cref="Target"/> values instantly, forcing a 0s update</summary>
            public void Set(T target, T current)
            {
                SetTarget_internal(target);
                SetCurrent_internal(current, forceSet: true);
                Update(0);
            }

            public void SetCurrent(T currentValue) => SetCurrent_internal(currentValue, forceSet: false);
            private void SetCurrent_internal(T currentValue, bool forceSet)
            {
                if (!forceSet && m_CurrentValue.Equals(currentValue))
                    return;

                m_CurrentValue = currentValue;

                foreach (ValueChangedDelegate listener in m_ValueChangedListeners)
                    listener.Invoke(m_CurrentValue);
            }

            public void SetTarget(T targetRestValue) => SetTarget_internal(targetRestValue);
            private void SetTarget_internal(T targetRestValue)
            {
                m_TargetValue = targetRestValue;
            }


            protected abstract void OnUpdate(float deltaTime, ref T currentValue, ref T currentVelocity, T targetValue);
            protected override void OnUpdate(float deltaTime)
            {
                T current = m_CurrentValue;
                OnUpdate(deltaTime, ref current, ref m_CurrentVelocity, m_TargetValue);
                SetCurrent_internal(current, forceSet: false); // funnelling the updated current value through the setter to ensure any effects are applied
            }
        }



        [System.Serializable]
        public class Float : Base<float>
        {
            public Float(float frequency, float damping01, float epsilon = 0.0001f)
                : base(frequency, damping01, epsilon) { }
            public Float(float frequency, float damping01, float restValue, float startValue, float epsilon = 0.0001f)
                : base(frequency, damping01, restValue, startValue, epsilon) { }


            protected override bool GetIsResting(float epsilon)
            {
                return Current.Approximately(Target, epsilon);
            }

            protected override void OnUpdate(
                float deltaTime, 
                ref float currentValue, 
                ref float currentVelocity, 
                float targetValue
                )
            {
                DampedSpringMotion.CalcDampedSimpleHarmonicMotion(ref currentValue, ref currentVelocity, targetValue, deltaTime, m_Frequency, m_Damping01);
            }
        }

        [System.Serializable]
        public class Vector2 : Base<UnityEngine.Vector2>
        {
            public Vector2(float frequency, float damping01, float epsilon = 0.0001f)
                : base(frequency, damping01, epsilon) { }
            public Vector2(float frequency, float damping01, UnityEngine.Vector2 restValue, UnityEngine.Vector2 startValue, float epsilon = 0.0001f)
                : base(frequency, damping01, restValue, startValue, epsilon) { }


            protected override bool GetIsResting(float epsilon)
            {
                return UnityEngine.Vector2.SqrMagnitude(Current - Target) < epsilon * epsilon;
            }

            protected override void OnUpdate(
                float deltaTime, 
                ref UnityEngine.Vector2 currentValue, 
                ref UnityEngine.Vector2 currentVelocity, 
                UnityEngine.Vector2 targetValue
                )
            {
                DampedSpringMotion.CalcDampedSimpleHarmonicMotion(ref currentValue, ref currentVelocity, targetValue, deltaTime, m_Frequency, m_Damping01);
            }
        }

        [System.Serializable]
        public class Vector3 : Base<UnityEngine.Vector3>
        {
            public Vector3(float frequency, float damping01, float epsilon = 0.0001f)
                : base(frequency, damping01, epsilon) { }
            public Vector3(float frequency, float damping01, UnityEngine.Vector3 restValue, UnityEngine.Vector3 startValue, float epsilon = 0.0001f)
                : base(frequency, damping01, restValue, startValue, epsilon) { }


            protected override bool GetIsResting(float epsilon)
            {
                return UnityEngine.Vector3.SqrMagnitude(Current - Target) < epsilon * epsilon;
            }

            protected override void OnUpdate(
                float deltaTime, 
                ref UnityEngine.Vector3 currentValue, 
                ref UnityEngine.Vector3 currentVelocity, 
                UnityEngine.Vector3 targetValue
                )
            {
                DampedSpringMotion.CalcDampedSimpleHarmonicMotion(ref currentValue, ref currentVelocity, targetValue, deltaTime, m_Frequency, m_Damping01);
            }
        }

        [System.Serializable]
        public class Vector4 : Base<UnityEngine.Vector4>
        {
            public Vector4(float frequency, float damping01, float epsilon = 0.0001f)
                : base(frequency, damping01, epsilon) { }
            public Vector4(float frequency, float damping01, UnityEngine.Vector4 restValue, UnityEngine.Vector4 startValue, float epsilon = 0.0001f)
                : base(frequency, damping01, restValue, startValue, epsilon) { }


            protected override bool GetIsResting(float epsilon)
            {
                return UnityEngine.Vector4.SqrMagnitude(Current - Target) < epsilon * epsilon;
            }

            protected override void OnUpdate(
                float deltaTime, 
                ref UnityEngine.Vector4 currentValue, 
                ref UnityEngine.Vector4 currentVelocity, 
                UnityEngine.Vector4 targetValue
                )
            {
                DampedSpringMotion.CalcDampedSimpleHarmonicMotion(ref currentValue, ref currentVelocity, targetValue, deltaTime, m_Frequency, m_Damping01);
            }
        }

        [System.Serializable]
        public class Quaternion : Base<UnityEngine.Quaternion>
        {
            public Quaternion(float frequency, float damping01, float epsilon = 0.0001f)
                : base(frequency, damping01, epsilon) { }
            public Quaternion(float frequency, float damping01, UnityEngine.Quaternion restValue, UnityEngine.Quaternion startValue, float epsilon = 0.0001f)
                : base(frequency, damping01, restValue, startValue, epsilon) { }

            protected override bool GetIsResting(float epsilon)
            {
                return UnityEngine.Vector3.Angle(Current * UnityEngine.Vector3.forward, Target * UnityEngine.Vector3.forward) < epsilon;
            }

            protected override void OnUpdate(
                float deltaTime, 
                ref UnityEngine.Quaternion currentValue, 
                ref UnityEngine.Quaternion currentVelocity, 
                UnityEngine.Quaternion targetValue
                )
            {
                DampedSpringMotion.CalcDampedSimpleHarmonicMotion(ref currentValue, ref currentVelocity, targetValue, deltaTime, m_Frequency, m_Damping01);
            }
        }
    }
}
