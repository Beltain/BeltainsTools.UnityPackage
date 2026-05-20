using UnityEngine;
using UnityEngine.InputSystem;

namespace BeltainsTools.FSM
{
    internal class FSMTestComponent : MonoBehaviour
    {
        StateMachine m_StateMachine;

        private abstract class StateBase : IState
        {
            protected FSMTestComponent m_FSMTest;

            protected StateBase(FSMTestComponent fsmTest)
            {
                m_FSMTest = fsmTest;
            }

            public virtual void OnEnter() { }
            public virtual void OnExit() { }
        }

        private class StateA : StateBase
        {
            public StateA(FSMTestComponent fsmTest) : base(fsmTest) { }

            public override void OnEnter()
            {
                Debug.Log($"State A: OnEnter on {m_FSMTest}");
            }

            public override void OnExit()
            {
                Debug.Log($"State A: OnExit on {m_FSMTest}");
            }
        }

        private class StateB : StateBase
        {
            public StateB(FSMTestComponent fsmTest) : base(fsmTest) { }

            public override void OnEnter()
            {
                Debug.Log($"State B: OnEnter on {m_FSMTest}");
            }
            public override void OnExit()
            {
                Debug.Log($"State B: OnExit on {m_FSMTest}");
            }
        }

        private class StateC : StateBase
        {
            public StateC(FSMTestComponent fsmTest) : base(fsmTest) { }

            public override void OnEnter()
            {
                Debug.Log($"State C: OnEnter on {m_FSMTest}");
            }
            public override void OnExit()
            {
                Debug.Log($"State C: OnExit on {m_FSMTest}");
            }
        }




        private void Awake()
        {
            m_StateMachine = new StateMachine();

            StateA stateA = new StateA(this);
            StateB stateB = new StateB(this);
            StateC stateC = new StateC(this);

            m_StateMachine.AddTransition(stateA, stateB, () => Keyboard.current.bKey.wasPressedThisFrame);
            m_StateMachine.AddTransition(stateA, () => Keyboard.current.aKey.wasPressedThisFrame);
            m_StateMachine.AddTransition(stateC, () => Keyboard.current.cKey.wasPressedThisFrame);

            m_StateMachine.SetState(stateA);
        }

        private void Update()
        {
            m_StateMachine.Update();
        }
    }
}
