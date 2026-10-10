// Sourced mostly from: https://www.youtube.com/watch?v=c-XoTg6Fba4
// (or https://github.com/adammyhre/Unity-Hierarchical-StateMachine/tree/master)
// THANK YOU GIT-AMEND YOU LEGEND

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public class StateMachine : System.IDisposable
    {
        public readonly State RootState;
        public readonly TransitionSequencer Sequencer;

        private bool m_Started = false;
        private bool m_Stopping = false; // clean stop in progress, keep ticking the sequencer until the exit completes

        public bool IsRunning => m_Started;

        public static StateMachine BuildFrom(State rootState, TransitionSequencer.SequencingModes sequencingMode = TransitionSequencer.SequencingModes.Sequential)
        {
            StateMachineBuilder builder = new StateMachineBuilder(rootState);
            return builder.Build(sequencingMode);
        }

        public StateMachine(State rootState, TransitionSequencer.SequencingModes sequencingMode = TransitionSequencer.SequencingModes.Sequential)
        {
            d.Assert(rootState != null, "State machine root state cannot be null!");
            RootState = rootState;
            Sequencer = new TransitionSequencer(this, sequencingMode);
        }

        // NOTE: no finalizer, state exit callbacks touch Unity objects and must never run on the finalizer thread
        public void Dispose()
        {
            StopImmediately();
        }

        public override string ToString()
        {
            if (RootState == null)
                return "Invalid State Machine";
            if (!m_Started && !m_Stopping)
                return $"Unstarted State Machine w/ Root '{RootState}'";
            string activeTree = RootState.ToStringActiveTree(includeActivationStatus: true);
            return $"{activeTree}\n{Sequencer.ToString()}";
        }

        public void Start()
        {
            EnsureStarted();
        }

        private void EnsureStarted()
        {
            if (m_Started)
                return;
            if (m_Stopping)
                StopImmediately(); // restarting mid clean-stop, finish the stop instantly so we enter from a clean tree
            m_Started = true;
            Sequencer.RequestTransition(null, RootState); // from null to root state's lowest initial substate is a full entry of the state machine
        }

        /// <summary>Exits the state machine entirely, and instantly. Skipping activities and deactivation calls. Any running or queued transitions are cancelled.</summary>
        public void StopImmediately()
        {
            if (!m_Started && !m_Stopping)
                return;
            m_Started = false;
            m_Stopping = false;
            Sequencer.CancelAll();
        }

        /// <summary>Exit the state machine entirely, with an optional callback for a clean exit (inclusive of all exit activities).</summary>
        /// <param name="exitTransitionCompleteCallback">The callback for when a full exit of the state machine has been completed, including all exit activities.</param>
        public void Stop(System.Action<State, State, bool> exitTransitionCompleteCallback = null)
        {
            if (!m_Started)
                return;
            m_Started = false;
            m_Stopping = true;
            Sequencer.RequestTransition(null, null, (from, to, success) =>
            {
                m_Stopping = false;
                exitTransitionCompleteCallback?.Invoke(from, to, success);
            }); // to null is a full exit of the state machine, queued behind anything already requested
        }

        public void Update() => Update(Time.deltaTime);
        public void Update(float deltaTime)
        {
            if (!m_Started && !m_Stopping)
                return;
            if (m_Stopping)
            {
                Sequencer.Update(0f); // only progress the exit, don't run state logic
                return;
            }
            Sequencer.Update(deltaTime);
        }

        public void FixedUpdate()
        {
            if (!m_Started || Sequencer.IsTransitioning)
                return;
            Sequencer.Lock();
            try { FixedUpdate_Internal(); }
            finally { Sequencer.Unlock(); }
        }

        public void LateUpdate() => LateUpdate(Time.deltaTime);
        public void LateUpdate(float deltaTime)
        {
            if (!m_Started || Sequencer.IsTransitioning)
                return;
            Sequencer.Lock();
            try { LateUpdate_Internal(deltaTime); }
            finally { Sequencer.Unlock(); }
        }

        internal void Update_Internal(float deltaTime)
        {
            if (m_Stopping)
                return;
            RootState.Update(deltaTime);
        }
        internal void FixedUpdate_Internal() => RootState.FixedUpdate();
        internal void LateUpdate_Internal(float deltaTime) => RootState.LateUpdate(deltaTime);


        /// <summary>Instantly changes state, skipping activities. Overlay-aware: everything active below the divergence point (including overlays) is exited child first.</summary>
        /// <remarks><paramref name="to"/> null is a full exit of the state machine</remarks>
        public void ChangeState(State from, State to)
        {
            Sequencer.ApplyImmediate(from, to);
        }
    }
}
