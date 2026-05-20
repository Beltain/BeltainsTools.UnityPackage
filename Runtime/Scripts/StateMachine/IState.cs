namespace BeltainsTools.FSM
{
    public interface IState
    {
        public void OnEnter() { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public void OnLateUpdate() { }
        public void OnExit() { }
    }
}
