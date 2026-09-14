namespace BeltainsTools.EventHandling
{
    /// <summary>
    /// A sticky <see cref="IBEvent"/> that can be subscribed to/unsubscribed from by delegates<br/>
    /// If the event has already been invoked, new subscribers will be immediately invoked upon subscription.
    /// </summary>
    public interface IStickyBEvent : IBEvent
    {
        bool HasBeenInvoked { get; }

        void ClearSticky();
    }

    /// <inheritdoc cref="IStickyBEvent"/>
    public struct StickyBEvent : IStickyBEvent
    {
        DelegateIterator<System.Action> m_DelegateList;
        bool m_HasBeenInvoked;

        public bool HasBeenInvoked => m_HasBeenInvoked;
        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action _delegate)
        {
            BEventHelper.Subscribe(ref m_DelegateList, _delegate);
            if (m_HasBeenInvoked)
                _delegate?.Invoke();
        }

        public void Unsubscribe(System.Action _delegate)
        {
            BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        }

        public void Clear()
        {
            m_DelegateList = null;
            ClearSticky();
        }

        public void ClearSticky()
        {
            m_HasBeenInvoked = false;
        }

        public void Invoke()
        {
            m_HasBeenInvoked = true;
            BEventHelper.Invoke(m_DelegateList, static d => d.Invoke());
        }

        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }

    /// <inheritdoc cref="IStickyBEvent"/>
    /// <remarks>Doesn't allow reference types as generic parameters to prevent potential memory leaks and unintended behavior.</remarks>
    public struct StickyBEvent<A> : IStickyBEvent 
        where A : struct
    {
        DelegateIterator<System.Action<A>> m_DelegateList;
        bool m_HasBeenInvoked;
        A m_LastArg;

        public bool HasBeenInvoked => m_HasBeenInvoked;
        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A> _delegate)
        {
            BEventHelper.Subscribe(ref m_DelegateList, _delegate);
            if (m_HasBeenInvoked)
                _delegate?.Invoke(m_LastArg);
        }

        public void Unsubscribe(System.Action<A> _delegate)
        {
            BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        }

        public void Clear()
        {
            m_DelegateList = null;
            ClearSticky();
        }

        public void ClearSticky()
        {
            m_HasBeenInvoked = false;
            m_LastArg = default;
        }

        public void Invoke(A arg1)
        {
            m_HasBeenInvoked = true;
            m_LastArg = arg1;
            BEventHelper.Invoke(m_DelegateList, d => d.Invoke(arg1));
        }

        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }

    /// <inheritdoc cref="StickyBEvent{A}"/>
    public struct StickyBEvent<A, B> : IStickyBEvent
        where A : struct 
        where B : struct
    {
        DelegateIterator<System.Action<A, B>> m_DelegateList;
        bool m_HasBeenInvoked;
        A m_LastArg1;
        B m_LastArg2;

        public bool HasBeenInvoked => m_HasBeenInvoked;
        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B> _delegate)
        {
            BEventHelper.Subscribe(ref m_DelegateList, _delegate);
            if (m_HasBeenInvoked)
                _delegate?.Invoke(m_LastArg1, m_LastArg2);
        }

        public void Unsubscribe(System.Action<A, B> _delegate)
        {
            BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        }

        public void Clear()
        {
            m_DelegateList = null;
            ClearSticky();
        }

        public void ClearSticky()
        {
            m_HasBeenInvoked = false;
            m_LastArg1 = default;
            m_LastArg2 = default;
        }

        public void Invoke(A arg1, B arg2)
        {
            m_HasBeenInvoked = true;
            m_LastArg1 = arg1;
            m_LastArg2 = arg2;
            BEventHelper.Invoke(m_DelegateList, d => d.Invoke(arg1, arg2));
        }

        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }

    /// <inheritdoc cref="StickyBEvent{A}"/>
    public struct StickyBEvent<A, B, C> : IStickyBEvent
        where A : struct
        where B : struct
        where C : struct
    {
        DelegateIterator<System.Action<A, B, C>> m_DelegateList;
        bool m_HasBeenInvoked;
        A m_LastArg1;
        B m_LastArg2;
        C m_LastArg3;

        public bool HasBeenInvoked => m_HasBeenInvoked;
        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B, C> _delegate)
        {
            BEventHelper.Subscribe(ref m_DelegateList, _delegate);
            if (m_HasBeenInvoked)
                _delegate?.Invoke(m_LastArg1, m_LastArg2, m_LastArg3);
        }

        public void Unsubscribe(System.Action<A, B, C> _delegate)
        {
            BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        }

        public void Clear()
        {
            m_DelegateList = null;
            ClearSticky();
        }

        public void ClearSticky()
        {
            m_HasBeenInvoked = false;
            m_LastArg1 = default;
            m_LastArg2 = default;
            m_LastArg3 = default;
        }

        public void Invoke(A arg1, B arg2, C arg3)
        {
            m_HasBeenInvoked = true;
            m_LastArg1 = arg1;
            m_LastArg2 = arg2;
            m_LastArg3 = arg3;
            BEventHelper.Invoke(m_DelegateList, d => d.Invoke(arg1, arg2, arg3));
        }

        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }

    /// <inheritdoc cref="StickyBEvent{A}"/>
    public struct StickyBEvent<A, B, C, D> : IStickyBEvent
        where A : struct
        where B : struct
        where C : struct
        where D : struct
    {
        DelegateIterator<System.Action<A, B, C, D>> m_DelegateList;
        bool m_HasBeenInvoked;
        A m_LastArg1;
        B m_LastArg2;
        C m_LastArg3;
        D m_LastArg4;

        public bool HasBeenInvoked => m_HasBeenInvoked;
        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B, C, D> _delegate)
        {
            BEventHelper.Subscribe(ref m_DelegateList, _delegate);
            if (m_HasBeenInvoked)
                _delegate?.Invoke(m_LastArg1, m_LastArg2, m_LastArg3, m_LastArg4);
        }

        public void Unsubscribe(System.Action<A, B, C, D> _delegate)
        {
            BEventHelper.Unsubscribe(ref m_DelegateList, _delegate);
        }

        public void Clear()
        {
            m_DelegateList = null;
            ClearSticky();
        }

        public void ClearSticky()
        {
            m_HasBeenInvoked = false;
            m_LastArg1 = default;
            m_LastArg2 = default;
            m_LastArg3 = default;
            m_LastArg4 = default;
        }

        public void Invoke(A arg1, B arg2, C arg3, D arg4)
        {
            m_HasBeenInvoked = true;
            m_LastArg1 = arg1;
            m_LastArg2 = arg2;
            m_LastArg3 = arg3;
            m_LastArg4 = arg4;
            BEventHelper.Invoke(m_DelegateList, d => d.Invoke(arg1, arg2, arg3, arg4));
        }

        public void PrintSubscribers(string contextMsg) => BEventHelper.PrintSubscribers(m_DelegateList, contextMsg);
        public void EnsureNoSubscribers() => BEventHelper.EnsureNoSubscribers(ref m_DelegateList);
    }
}

