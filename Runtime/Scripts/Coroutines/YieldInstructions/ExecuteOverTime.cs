using UnityEngine;

namespace BeltainsTools.Coroutines
{
    public class ExecuteOverTime : CustomYieldInstruction
    {
        float m_TDuration;
        float m_TRemaining;
        System.Action<float> m_UpdateAction;
        TimeStepType m_TimeStepType;

        public enum TimeStepType
        {
            deltaTime,
            fixedDeltaTime,
            unscaledDeltaTime,
            fixedUnscaledDeltaTime,
        }

        /// <summary>
        /// Execute an action every specified timestep, until the duration has lapsed.
        /// <para>t is passed to the executing action, where t goes from 0-1 over the full duration</para>
        /// </summary>
        public ExecuteOverTime(float time, System.Action<float> methodToExecuteOverTime, TimeStepType timeStepType = TimeStepType.deltaTime)
        {
            m_TDuration = time == 0f ? 1f : time;
            m_TRemaining = time == 0f ? 0f : time;
            m_UpdateAction = methodToExecuteOverTime;
            if (m_UpdateAction == null)
                throw new System.Exception("Expected a method to execute over time but got null in ExecuteOverTime");
            m_TimeStepType = timeStepType;
        }

        public override bool keepWaiting
        {
            get
            {
                m_TRemaining = Mathf.Max(0f, m_TRemaining - GetTimeDelta());
                float t = 1f - (m_TRemaining / m_TDuration);
                m_UpdateAction.Invoke(t);
                return t != 1f;
            }
        }

        float GetTimeDelta()
        {
            switch (m_TimeStepType)
            {
                case TimeStepType.fixedDeltaTime:
                    return Time.fixedDeltaTime;
                case TimeStepType.unscaledDeltaTime:
                    return Time.unscaledDeltaTime;
                case TimeStepType.fixedUnscaledDeltaTime:
                    return Time.fixedUnscaledDeltaTime;
                case TimeStepType.deltaTime:
                default:
                    return Time.deltaTime;
            }
        }
    }
}
