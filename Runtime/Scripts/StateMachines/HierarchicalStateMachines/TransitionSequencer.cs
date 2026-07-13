using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public class TransitionSequencer
    {
        public readonly StateMachine Machine;
        private readonly SequencingModes m_SequencingMode;

        private PhaseExecutor m_PhaseExecutor = new PhaseExecutor(); // handles the sub-stages of a transition, the "phases"
        private TransitionData m_ActiveTransitionData; // transition currently running
        private TransitionData m_PendingTransitionData; // transition stored when attempting to trigger one while another is running

        public bool IsTransitioning => m_PhaseExecutor.IsRunning;

        public enum SequencingModes
        {
            Parallel,
            Sequential,
        }

        private struct TransitionData 
        {
            public State From;
            public State To;
            public State LCA;

            public IEnumerable<State> ExitChain;
            public IEnumerable<State> EnterChain;

            public bool IsValid => To != null;

            public TransitionData(State from, State to) 
            { 
                From = from; 
                To = to; 
                LCA = From == null ? To.Machine.RootState : From.GetLowestCommonAncestor(To); 

                ExitChain = From?.WalkUpTo(LCA, inclusive: false) ?? null;
                EnterChain = LCA?.WalkDownTo(To, inclusive: false) ?? null;
            }

            public void Clear() { this.From = null; this.To = null; this.LCA = null; this.ExitChain = null; this.EnterChain = null; }
        }

        /// <summary>Executor for the sub-stages of transitions, their "phases"</summary>
        private class PhaseExecutor 
        {
            List<PhaseStep> m_Steps = new List<PhaseStep>();
            ISequence m_Sequence;
            CancellationTokenSource m_SequencerCancellationTokenSrc;
            System.Action m_Callback;

            public bool IsRunning => m_Sequence != null;

            public void Start(IEnumerable<PhaseStep> steps, SequencingModes sequencingMode, System.Action callback)
            {
                d.Assert(!IsRunning, "Trying to start a sequenced execution of phase steps for a state transition while one is already running! Fix me!");

                m_Steps.AddRange(steps);
                m_Callback = callback;
                m_SequencerCancellationTokenSrc = new CancellationTokenSource();
                m_Sequence = BuildSequencer(sequencingMode, m_Steps, m_SequencerCancellationTokenSrc);
                m_Sequence.Start();
            }

            public void Tick()
            {
                if (!IsRunning)
                    return;
                if (!m_Sequence.Tick())
                    return; // just a normal update
                End(); // we done
            }

            private void End()
            {
                System.Action callback = m_Callback;

                m_Steps.Clear();
                m_Sequence = null;
                m_SequencerCancellationTokenSrc = null;
                m_Callback = null;

                callback?.Invoke();
            }

            private static ISequence BuildSequencer(SequencingModes mode, List<PhaseStep> phaseSteps, CancellationTokenSource ct)
            {
                return mode switch
                {
                    SequencingModes.Parallel => new ParallelSequence(phaseSteps, ct.Token),
                    SequencingModes.Sequential => new SequentialSequence(phaseSteps, ct.Token),
                    _ => throw new System.NotImplementedException()
                };
            }
        }

        private static IEnumerable<PhaseStep> GatherPhaseSteps(IEnumerable<State> stateChain, bool deactivate)
        {
            foreach (State state in stateChain)
            {
                IEnumerable<PhaseStep> activities = deactivate ? 
                    state.GetDeactivationActivities() : 
                    state.GetActivationActivities();
                foreach (PhaseStep step in activities)
                    yield return step;
            }
        }

        public TransitionSequencer(StateMachine machine, SequencingModes sequencingMode)
        {
            Machine = machine;
            m_SequencingMode = sequencingMode;
        }

        public void RequestTransition(State from, State to)
        {
            d.AssertFormat(to != null, "Trying to request a transition to a null state! from: {0}. This is not possible! Please fix me!", from);
            to = to.GetLowestInitialSubState();
            TransitionData transition = new TransitionData(from, to);
            if (!transition.IsValid)
                return;

            if (IsTransitioning)
            {
                m_PendingTransitionData = transition;
                return;
            }

            BeginTransition(transition);
        }

        private void BeginTransition(TransitionData transition)
        {
            m_ActiveTransitionData = transition;
            if (transition.From != null)
                TransitionStartExitPhase();
            else
                TransitionEndExitPhase(); // skip exit if we don't have a "from"
        }

        private void TransitionStartExitPhase()
        {
            // mark deactivation started for the old state chain as started, from bottom to top
            foreach (State state in m_ActiveTransitionData.ExitChain)
                state.BeginDeactivation();

            // get and deactivate the old state chain's phase steps
            m_PhaseExecutor.Start(GatherPhaseSteps(m_ActiveTransitionData.ExitChain, deactivate: true), m_SequencingMode, TransitionEndExitPhase);
        }

        private void TransitionEndExitPhase()
        {
            Machine.ChangeState(m_ActiveTransitionData.From, m_ActiveTransitionData.To);
            TransitionStartEnterPhase();
        }

        private void TransitionStartEnterPhase()
        {
            // get and activate the new state chain's phase steps
            m_PhaseExecutor.Start(GatherPhaseSteps(m_ActiveTransitionData.EnterChain, deactivate: false), m_SequencingMode, TransitionEndEnterPhase);
        }

        private void TransitionEndEnterPhase()
        {
            // mark activation for the new state chain as complete, from top to bottom
            foreach (State state in m_ActiveTransitionData.EnterChain)
                state.CompleteActivation();

            // finalise
            EndTransition();
        }

        private void EndTransition()
        {
            m_ActiveTransitionData.Clear();

            if (m_PendingTransitionData.IsValid)
            {
                BeginTransition(m_PendingTransitionData);
                m_PendingTransitionData.Clear();
            }
        }

        public void Update(float deltaTime)
        {
            if (IsTransitioning)
            {
                m_PhaseExecutor.Tick();
                return;
            }

            Machine.Update_Internal(deltaTime);
        }
    }
}
