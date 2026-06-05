using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    /// <summary>Represents a collection of asynchronous steps to execute and handles the status of their execution updating</summary>
    public interface ISequence
    {
        void Start();
        /// <returns>True if the execution is complete</returns>
        bool Tick();
    }

    public delegate Task PhaseStep(CancellationToken ct);

    public class ParallelSequence : ISequence
    {
        private readonly List<PhaseStep> m_Steps;
        private readonly CancellationToken m_CancellationToken;

        private Task[] m_Tasks;

        public ParallelSequence(List<PhaseStep> steps, CancellationToken ct)
        {
            m_Steps = steps;
            m_CancellationToken = ct;
            m_Tasks = new Task[m_Steps != null ? m_Steps.Count : 0];
        }

        public void Start()
        {
            if (m_Steps == null || m_Steps.Count == 0)
                return;

            for (int i = 0; i < m_Steps.Count; i++)
                m_Tasks[i] = m_Steps[i].Invoke(m_CancellationToken);
        }

        public bool Tick()
        {
            bool allComplete = true;
            for (int i = 0; allComplete && i < m_Tasks.Length; i++)
            {
                if (!m_Tasks[i].IsCompleted)
                    allComplete = false;
            }
            return allComplete;
        }
    }

    public class SequentialSequence : ISequence 
    {
        private readonly List<PhaseStep> m_Steps;
        private readonly CancellationToken m_CancellationToken;

        private int m_StepIndex = -1;
        private Task m_Current;

        private bool IsDone => m_StepIndex >= m_Steps.Count;

        public SequentialSequence(List<PhaseStep> steps, CancellationToken ct)
        {
            m_Steps = steps;
            m_CancellationToken = ct;
        }

        public void Start()
        {
            Next();
        }

        public bool Tick()
        {
            if (IsDone)
                return true;

            if (m_Current == null || m_Current.IsCompleted)
                Next();

            return IsDone;
        }

        public void Next()
        {
            m_StepIndex++;
            if (!IsDone)
                m_Current = m_Steps[m_StepIndex].Invoke(m_CancellationToken);
        }
    }
}
