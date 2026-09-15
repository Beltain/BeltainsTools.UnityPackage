// Sourced mostly from: https://www.youtube.com/watch?v=c-XoTg6Fba4
// (or https://github.com/adammyhre/Unity-Hierarchical-StateMachine/tree/master)
// THANK YOU GIT-AMEND YOU LEGEND

using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public class StateMachine
    {
        public readonly State RootState;
        public readonly TransitionSequencer Sequencer;

        private bool m_Started = false;

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

        public override string ToString()
        {
            return RootState != null ?
                RootState.GetLeaf().ToString() :
                "Invalid State Machine";
        }

        public void Start()
        {
            EnsureStarted();
        }

        private void EnsureStarted()
        {
            if (m_Started)
                return;
            m_Started = true;
            Sequencer.RequestTransition(null, RootState); // from null to root state's lowest initial substate is a full entry of the state machine
        }

        /// <summary>Exit the state machine entirely, with an optional callback for a clean exit.</summary>
        /// <param name="exitTransitionCompleteCallback">The callback for when a full exit of the state machine has been completed, including all exit activities.</param>
        private void Stop(System.Action<State, State, bool> exitTransitionCompleteCallback = null)
        {
            if (!m_Started)
                return;
            m_Started = false;
            Sequencer.RequestTransition(RootState, null, exitTransitionCompleteCallback); // from root state's current leaf to null is a full exit of the state machine
        }

        public void Update() => Update(Time.deltaTime);
        public void Update(float deltaTime)
        {
            if (!m_Started)
                return;
            Sequencer.Update(deltaTime);
        }

        public void FixedUpdate()
        {
            if (!m_Started)
                return;
            FixedUpdate_Internal();
        }

        public void LateUpdate() => LateUpdate(Time.deltaTime);
        public void LateUpdate(float deltaTime)
        {
            if (!m_Started)
                return;
            LateUpdate_Internal(deltaTime);
        }

        internal void Update_Internal(float deltaTime) => RootState.Update(deltaTime);
        internal void FixedUpdate_Internal() => RootState.FixedUpdate();
        internal void LateUpdate_Internal(float deltaTime) => RootState.LateUpdate(deltaTime);



        public void ChangeState(State from, State to)
        {
            if (from == to)
                return;

            State lca = State.GetLowestCommonAnscestor(from, to);
            if (from != null)
            {
                foreach (State ancestorState in from.WalkUpTo(lca, inclusive: to == null)) // also exit lca if to is null. We're essentially exiting the entire state machine.
                    ancestorState.Exit();
            }
            if (to != null)
            {
                foreach (State descendantState in lca.WalkDownTo(to, inclusive: from == null)) // also enter the lca if from is null. We're essentially entering the state machine for the first time
                    descendantState.Enter();
            }
        }
    }
}
