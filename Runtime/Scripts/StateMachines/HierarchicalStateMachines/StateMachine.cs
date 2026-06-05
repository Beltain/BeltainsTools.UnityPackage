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

        public StateMachine(State rootState, TransitionSequencer.SequencingModes sequencingMode = TransitionSequencer.SequencingModes.Sequential)
        {
            d.Assert(rootState != null, "State machine root state cannot be null!");
            RootState = rootState;
            Sequencer = new TransitionSequencer(this, sequencingMode);
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
            RootState.Enter();
        }

        public void Update() => Update(Time.deltaTime);
        public void Update(float deltaTime)
        {
            EnsureStarted();
            Sequencer.Update(deltaTime);
        }

        public void FixedUpdate()
        {
            EnsureStarted();
            FixedUpdate_Internal();
        }

        public void LateUpdate() => LateUpdate(Time.deltaTime);
        public void LateUpdate(float deltaTime)
        {
            EnsureStarted();
            LateUpdate_Internal(deltaTime);
        }

        internal void Update_Internal(float deltaTime) => RootState.Update(deltaTime);
        internal void FixedUpdate_Internal() => RootState.FixedUpdate();
        internal void LateUpdate_Internal(float deltaTime) => RootState.LateUpdate(deltaTime);



        public void ChangeState(State from, State to)
        {
            if (from == to || from == null || to == null)
                return;

            State commonAncestor = from.GetLowestCommonAncestor(to);
            foreach (State ancestorState in from.WalkUpTo(commonAncestor, inclusive: false))
                ancestorState.Exit();
            foreach (State descendantState in commonAncestor.WalkDownTo(to, inclusive: false))
                descendantState.Enter();
        }
    }
}
