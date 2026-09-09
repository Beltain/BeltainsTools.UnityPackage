namespace BeltainsTools.EventHandling
{
    public interface IBEvent
    {
        bool HasSubscribers { get; }
        int SubscriberCount { get; }

        void Clear();
        void EnsureNoSubscribers();
        void PrintSubscribers(string context);
    }

    /// <summary>
    /// Simple event that can be subscribed to/unsubscribed from by delegates
    /// <para>Essentially a c# event with a few more controls/features for debugging and management</para>
    /// </summary>
    public struct BEvent : IBEvent
    {
        DelegateIterator<System.Action> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action _delegate) => BEventHelper.Subscribe(ref m_DelegateList, _delegate);
        public void Unsubscribe(System.Action _delegate) => BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        public void Clear() => m_DelegateList = null;
        public void Invoke() => BEventHelper.Invoke(m_DelegateList, static d => d.Invoke());
        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }

    /// <inheritdoc cref="BEvent"/>
    public struct BEvent<A> : IBEvent
    {
        DelegateIterator<System.Action<A>> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A> _delegate) => BEventHelper.Subscribe(ref m_DelegateList, _delegate);
        public void Unsubscribe(System.Action<A> _delegate) => BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        public void Clear() => m_DelegateList = null;
        public void Invoke(A arg1) => BEventHelper.Invoke(m_DelegateList, d => d.Invoke(arg1));
        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }

    /// <inheritdoc cref="BEvent"/>
    public struct BEvent<A, B> : IBEvent
    {
        DelegateIterator<System.Action<A, B>> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B> _delegate) => BEventHelper.Subscribe(ref m_DelegateList, _delegate);
        public void Unsubscribe(System.Action<A, B> _delegate) => BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        public void Clear() => m_DelegateList = null;
        public void Invoke(A arg1, B arg2) => BEventHelper.Invoke(m_DelegateList, d => d.Invoke(arg1, arg2));
        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }

    /// <inheritdoc cref="BEvent"/>
    public struct BEvent<A, B, C> : IBEvent
    {
        DelegateIterator<System.Action<A, B, C>> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B, C> _delegate) => BEventHelper.Subscribe(ref m_DelegateList, _delegate);
        public void Unsubscribe(System.Action<A, B, C> _delegate) => BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        public void Clear() => m_DelegateList = null;
        public void Invoke(A arg1, B arg2, C arg3) => BEventHelper.Invoke(m_DelegateList, d => d.Invoke(arg1, arg2, arg3));
        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }

    /// <inheritdoc cref="BEvent"/>
    public struct BEvent<A, B, C, D> : IBEvent
    {
        DelegateIterator<System.Action<A, B, C, D>> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B, C, D> _delegate) => BEventHelper.Subscribe(ref m_DelegateList, _delegate);
        public void Unsubscribe(System.Action<A, B, C, D> _delegate) => BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        public void Clear() => m_DelegateList = null;
        public void Invoke(A arg1, B arg2, C arg3, D arg4) => BEventHelper.Invoke(m_DelegateList, d => d.Invoke(arg1, arg2, arg3, arg4));
        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }
}

