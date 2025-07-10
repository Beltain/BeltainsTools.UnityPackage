using UnityEngine;

namespace BeltainsTools.Tweens
{
    /// <summary>A very primitive and unmanaged Tween.</summary>
    public class TimeLerper
    {
        protected float m_StartValue;
        protected float m_EndValue;
        protected float m_CurrentValue;

        protected float m_DurationStart;
        protected float m_DurationRemaining;

        protected AnimationCurve m_LerpCurve;


        public bool HasEnded { get; private set; }
        public float CurrentValue => m_CurrentValue;


        public enum CurveTypes
        {
            Linear = 0,
            EaseInOut = 1,
            EaseIn = 2,
            EaseOut = 3,
        }

        public TimeLerper(float startValue, float endValue, float duration, CurveTypes curveType = CurveTypes.Linear)
        {
            Debug.Assert(duration != 0f, "Attempted to create a time lerper with a 0 duration! This is not allowed!");

            m_StartValue = startValue;
            m_CurrentValue = startValue;
            m_EndValue = endValue;
            m_DurationStart = duration;
            m_DurationRemaining = m_DurationStart;

            switch (curveType)
            {
                case CurveTypes.Linear:
                    m_LerpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                    break;
                case CurveTypes.EaseInOut:
                    m_LerpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                    break;
                case CurveTypes.EaseIn:
                    m_LerpCurve = AnimationCurveExtensions.EaseIn(0f, 0f, 1f, 1f);
                    break;
                case CurveTypes.EaseOut:
                    m_LerpCurve = AnimationCurveExtensions.EaseOut(0f, 0f, 1f, 1f);
                    break;
            }
        }

        public static TimeLerper Create(float startValue, float endValue, float duration, CurveTypes curveType = CurveTypes.Linear)
            => new TimeLerper(startValue, endValue, duration, curveType);

        public static TimeLerper Create_SpeedWise(float startValue, float endValue, float speed, CurveTypes curveType = CurveTypes.Linear)
            => new TimeLerper(startValue, endValue, Mathf.Abs((endValue - startValue) / speed), curveType);



        /// <summary>Update the time lerper's progress (based on delta time) and return whether it's complete or not</summary>
        public bool Update()
        {
            if (!HasEnded)
            {
                m_DurationRemaining = Mathf.Max(0f, m_DurationRemaining - Time.deltaTime);
                float progress = 1f - (m_DurationRemaining / m_DurationStart);
                progress = m_LerpCurve.Evaluate(progress); //use the value returned from the progress through the duration and slap it through a curve to retrun a smoothed (or otherwise altered) value.

                m_CurrentValue = Mathf.Lerp(m_StartValue, m_EndValue, progress);

                HasEnded = m_CurrentValue == m_EndValue;
            }

            return HasEnded;
        }
    }
}
