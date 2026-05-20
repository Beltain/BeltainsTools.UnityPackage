namespace BeltainsTools.FSM
{
    public class FuncPredicate : IPredicate
    {
        private readonly System.Func<bool> m_Predicate;

        public static implicit operator FuncPredicate(System.Func<bool> predicate)
        {
            return new FuncPredicate(predicate);
        }

        public FuncPredicate(System.Func<bool> predicate)
        {
            m_Predicate = predicate;
        }

        public bool Evaluate() => m_Predicate.Invoke();
    }
}
