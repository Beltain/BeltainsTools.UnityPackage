using System.Collections.Generic;
using UnityEngine;


namespace BeltainsTools.FSM
{
    public class StateMachine
    {
        private StateHandler m_Current;
        private Dictionary<System.Type, StateHandler> m_States = new Dictionary<System.Type, StateHandler>();
        private HashSet<ITransition> m_FromAnyTransitions = new HashSet<ITransition>();


        private class StateHandler
        {
            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }

            public StateHandler(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public void AddTransition(IState to, IPredicate predicate)
            {
                Transitions.Add(new Transition(to, predicate));
            }
        }



        public void SetState(IState state)
        {
            if (state != null)
            {
                m_Current = GetOrCreateStateHandler(state);
                m_Current.State.OnEnter();
            }
            else
            {
                m_Current = null;
            }
        }

        private void SwitchState(IState state)
        {
            if (m_Current != null && state == m_Current.State)
                return;

            IState prev = m_Current != null ? m_Current.State : null;
            IState next = GetOrCreateStateHandler(state).State; // get our managed ref

            prev?.OnExit();
            next.OnEnter();

            m_Current = GetOrCreateStateHandler(state);
        }


        public void AddTransition(IState from, IState to, System.Func<bool> predicate) => AddTransition(from, to, (FuncPredicate)predicate);
        public void AddTransition(IState from, IState to, IPredicate predicate)
        {
            GetOrCreateStateHandler(from).AddTransition(GetOrCreateStateHandler(to).State, predicate);
        }

        public void AddTransition(IState to, System.Func<bool> predicate) => AddTransition(to, (FuncPredicate)predicate);
        public void AddTransition(IState to, IPredicate predicate)
        {
            m_FromAnyTransitions.Add(new Transition(GetOrCreateStateHandler(to).State, predicate));
        }


        private bool TryGetAnyTransition(StateHandler stateHandler, out ITransition resultTransition)
        {
            if (TryGetTransitionFrom(m_FromAnyTransitions, out resultTransition))
                return true;

            if (stateHandler != null && TryGetTransitionFrom(stateHandler.Transitions, out resultTransition))
                return true;

            resultTransition = null;
            return false;
        }

        private bool TryGetTransitionFrom(IEnumerable<ITransition> transitions, out ITransition resultTransition)
        {
            foreach (ITransition transition in transitions)
            {
                if (transition.Condition.Evaluate())
                {
                    resultTransition = transition;
                    return true;
                }
            }

            resultTransition = null;
            return false;
        }


        private StateHandler GetOrCreateStateHandler(IState state)
        {
            if (!m_States.TryGetValue(state.GetType(), out StateHandler stateHandler))
            {
                stateHandler = new StateHandler(state);
                m_States.Add(state.GetType(), stateHandler);
            }

            return stateHandler;
        }




        public void Update()
        {
            if (m_Current == null)
                return;

            if (TryGetAnyTransition(m_Current, out ITransition transition))
                SwitchState(transition.To);

            m_Current.State.OnUpdate();
        }

        public void FixedUpdate()
        {
            if (m_Current == null)
                return;

            m_Current.State.OnFixedUpdate();
        }

        public void LateUpdate()
        {
            if (m_Current == null)
                return;

            m_Current.State.OnLateUpdate();
        }
    }
}
