using System.Collections.Generic;
using UnityEngine;

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

    #region Event Types

    public struct BEvent : IBEvent
    {
        DelegateIterator<System.Action> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action _delegate)
        {
            if (m_DelegateList == null)
                m_DelegateList = new DelegateIterator<System.Action>();

            Debug.Assert(!m_DelegateList.Contains(_delegate), "Event subscriber is DUPLICATED!");

            m_DelegateList.Add(_delegate);
        }

        public void Unsubscribe(System.Action _delegate)
        {
            if (m_DelegateList == null)
                return;

            m_DelegateList.Remove(_delegate);

            if (m_DelegateList.Count == 0)
                m_DelegateList = null;
        }

        public void Clear()
        {
            m_DelegateList = null;
        }

        public void Invoke()
        {
            if (m_DelegateList != null)
            {
                try
                {
                    foreach (var _delegate in m_DelegateList)
                        _delegate.Invoke();
                }
                finally
                {
                    // can be null if event is unsubscribed during sending event!
                    if (m_DelegateList != null)
                    {
                        m_DelegateList.StopIterating();
                    }
                }
            }
        }

        public void EnsureNoSubscribers()
        {
#if UNITY_EDITOR
            if (m_DelegateList != null)
            {
                Debug.Log("List of subscribers:");

                foreach (var _delegate in m_DelegateList)
                    Debug.LogFormat("\tobject \"{0}\" method \"{1}\"", _delegate.Target, _delegate.Method);

                Debug.Assert(false, "Event has subscribers during call to EnsureNoSubscribers");
            }
#endif // UNITY_EDITOR

            Clear();
        }

        public void PrintSubscribers(string contextMsg)
        {
            string msg = contextMsg + " Current subscribers (" + SubscriberCount + "):";
            if (m_DelegateList != null)
            {
                foreach (var _delegate in m_DelegateList)
                {
                    msg += "\n\t[ object '" + _delegate.Target + "' method '" + _delegate.Method + "' ],";
                }
            }
        }
    }

    public struct BEvent<A> : IBEvent
    {
        DelegateIterator<System.Action<A>> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A> _delegate)
        {
            if (m_DelegateList == null)
                m_DelegateList = new DelegateIterator<System.Action<A>>();

            Debug.Assert(!m_DelegateList.Contains(_delegate), "Event subscriber is DUPLICATED!");

            m_DelegateList.Add(_delegate);
        }

        public void Unsubscribe(System.Action<A> _delegate)
        {
            if (m_DelegateList == null)
                return;

            m_DelegateList.Remove(_delegate);

            if (m_DelegateList.Count == 0)
                m_DelegateList = null;
        }

        public void Clear()
        {
            m_DelegateList = null;
        }

        public void Invoke(A arg1)
        {
            if (m_DelegateList != null)
            {
                try
                {
                    foreach (var _delegate in m_DelegateList)
                        _delegate.Invoke(arg1);
                }
                finally
                {
                    // can be null if event is unsubscribed during sending event!
                    if (m_DelegateList != null)
                    {
                        m_DelegateList.StopIterating();
                    }
                }
            }
        }

        public void PrintSubscribers(string contextMsg)
        {
            string msg = contextMsg + " Current subscribers (" + SubscriberCount + "):";
            if (m_DelegateList != null)
            {
                foreach (var _delegate in m_DelegateList)
                {
                    msg += "\n\t[ object '" + _delegate.Target + "' method '" + _delegate.Method + "' ],";
                }
            }
        }

        public void EnsureNoSubscribers()
        {
#if UNITY_EDITOR
            if (m_DelegateList != null)
            {
                Debug.Log("List of subscribers:");

                foreach (var _delegate in m_DelegateList)
                    Debug.LogFormat("\tobject \"{0}\" method \"{1}\"", _delegate.Target, _delegate.Method);

                Debug.Assert(false, "Event has subscribers during call to EnsureNoSubscribers");
            }
#endif // UNITY_EDITOR

            Clear();
        }
    }

    public struct BEvent<A, B> : IBEvent
    {
        DelegateIterator<System.Action<A, B>> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B> _delegate)
        {
            if (m_DelegateList == null)
                m_DelegateList = new DelegateIterator<System.Action<A, B>>();

            Debug.Assert(!m_DelegateList.Contains(_delegate), "Event subscriber is DUPLICATED!");

            m_DelegateList.Add(_delegate);
        }

        public void Unsubscribe(System.Action<A, B> _delegate)
        {
            if (m_DelegateList == null)
                return;

            m_DelegateList.Remove(_delegate);

            if (m_DelegateList.Count == 0)
                m_DelegateList = null;
        }

        public void Clear()
        {
            m_DelegateList = null;
        }

        public void Invoke(A arg1, B arg2)
        {
            if (m_DelegateList != null)
            {
                try
                {
                    foreach (var _delegate in m_DelegateList)
                        _delegate.Invoke(arg1, arg2);
                }
                finally
                {
                    // can be null if event is unsubscribed during sending event!
                    if (m_DelegateList != null)
                    {
                        m_DelegateList.StopIterating();
                    }
                }
            }
        }

        public void PrintSubscribers(string contextMsg)
        {
            string msg = contextMsg + " Current subscribers (" + SubscriberCount + "):";
            if (m_DelegateList != null)
            {
                foreach (var _delegate in m_DelegateList)
                {
                    msg += "\n\t[ object '" + _delegate.Target + "' method '" + _delegate.Method + "' ],";
                }
            }
        }

        public void EnsureNoSubscribers()
        {
#if UNITY_EDITOR
            if (m_DelegateList != null)
            {
                Debug.Log("List of subscribers:");

                foreach (var _delegate in m_DelegateList)
                    Debug.LogFormat("\tobject \"{0}\" method \"{1}\"", _delegate.Target, _delegate.Method);

                Debug.Assert(false, "Event has subscribers during call to EnsureNoSubscribers");
            }
#endif // UNITY_EDITOR

            Clear();
        }
    }

    public struct BEvent<A, B, C> : IBEvent
    {
        DelegateIterator<System.Action<A, B, C>> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B, C> _delegate)
        {
            if (m_DelegateList == null)
                m_DelegateList = new DelegateIterator<System.Action<A, B, C>>();

            Debug.Assert(!m_DelegateList.Contains(_delegate), "Event subscriber is DUPLICATED!");

            m_DelegateList.Add(_delegate);
        }

        public void Unsubscribe(System.Action<A, B, C> _delegate)
        {
            if (m_DelegateList == null)
                return;

            m_DelegateList.Remove(_delegate);

            if (m_DelegateList.Count == 0)
                m_DelegateList = null;
        }

        public void Clear()
        {
            m_DelegateList = null;
        }

        public void Invoke(A arg1, B arg2, C arg3)
        {
            if (m_DelegateList != null)
            {
                try
                {
                    foreach (var _delegate in m_DelegateList)
                        _delegate.Invoke(arg1, arg2, arg3);
                }
                finally
                {
                    // can be null if event is unsubscribed during sending event!
                    if (m_DelegateList != null)
                    {
                        m_DelegateList.StopIterating();
                    }
                }
            }
        }

        public void PrintSubscribers(string contextMsg)
        {
            string msg = contextMsg + " Current subscribers (" + SubscriberCount + "):";
            if (m_DelegateList != null)
            {
                foreach (var _delegate in m_DelegateList)
                {
                    msg += "\n\t[ object '" + _delegate.Target + "' method '" + _delegate.Method + "' ],";
                }
            }
        }

        public void EnsureNoSubscribers()
        {
#if UNITY_EDITOR
            if (m_DelegateList != null)
            {
                Debug.Log("List of subscribers:");

                foreach (var _delegate in m_DelegateList)
                    Debug.LogFormat("\tobject \"{0}\" method \"{1}\"", _delegate.Target, _delegate.Method);

                Debug.Assert(false, "Event has subscribers during call to EnsureNoSubscribers");
            }
#endif // UNITY_EDITOR

            Clear();
        }
    }

    public struct BEvent<A, B, C, D> : IBEvent
    {
        DelegateIterator<System.Action<A, B, C, D>> m_DelegateList;

        public bool HasSubscribers => m_DelegateList != null;
        public int SubscriberCount => m_DelegateList != null ? m_DelegateList.Count : 0;

        public void Subscribe(System.Action<A, B, C, D> _delegate)
        {
            if (m_DelegateList == null)
                m_DelegateList = new DelegateIterator<System.Action<A, B, C, D>>();

            Debug.Assert(!m_DelegateList.Contains(_delegate), "Event subscriber is DUPLICATED!");

            m_DelegateList.Add(_delegate);
        }

        public void Unsubscribe(System.Action<A, B, C, D> _delegate)
        {
            if (m_DelegateList == null)
                return;

            m_DelegateList.Remove(_delegate);

            if (m_DelegateList.Count == 0)
                m_DelegateList = null;
        }

        public void Clear()
        {
            m_DelegateList = null;
        }

        public void Invoke(A arg1, B arg2, C arg3, D arg4)
        {
            if (m_DelegateList != null)
            {
                try
                {
                    foreach (var _delegate in m_DelegateList)
                        _delegate.Invoke(arg1, arg2, arg3, arg4);
                }
                finally
                {
                    // can be null if event is unsubscribed during sending event!
                    if (m_DelegateList != null)
                    {
                        m_DelegateList.StopIterating();
                    }
                }
            }
        }

        public void PrintSubscribers(string contextMsg)
        {
            string msg = contextMsg + " Current subscribers (" + SubscriberCount + "):";
            if (m_DelegateList != null)
            {
                foreach (var _delegate in m_DelegateList)
                {
                    msg += "\n\t[ object '" + _delegate.Target + "' method '" + _delegate.Method + "' ],";
                }
            }
        }

        public void EnsureNoSubscribers()
        {
#if UNITY_EDITOR
            if (m_DelegateList != null)
            {
                Debug.Log("List of subscribers:");

                foreach (var _delegate in m_DelegateList)
                    Debug.LogFormat("\tobject \"{0}\" method \"{1}\"", _delegate.Target, _delegate.Method);

                Debug.Assert(false, "Event has subscribers during call to EnsureNoSubscribers");
            }
#endif // UNITY_EDITOR

            Clear();
        }
    }

    #endregion


    public class DelegateIterator<T>
    {
        List<T> m_Delegates = new List<T>();

        const ushort k_IterationInvalid = 0xffff - 1;
        const ushort k_IterationStart = 0xffff;
        ushort m_IterationIndex = k_IterationInvalid;
        ushort m_IterationEnd;

#if UNITY_EDITOR
        System.Threading.Thread m_Thread;
#endif

        static readonly DelegateIterator<T> s_NullIterator = new DelegateIterator<T>();

        public int Count { get { return m_Delegates.Count; } }

        public T Current { get { Debug.Assert(m_IterationIndex != k_IterationInvalid); return m_Delegates[m_IterationIndex]; } }

        public bool Contains(T dd) { return (m_Delegates.IndexOf(dd) >= 0); }

        bool IsIterating { get { return m_IterationIndex != k_IterationInvalid; } }

        public DelegateIterator()
        {
#if UNITY_EDITOR
            m_Thread = System.Threading.Thread.CurrentThread;
#endif
        }

        public DelegateIterator<T> GetEnumerator()
        {
            if (IsIterating)
            {
                Debug.Assert(false, "Illegal attempt to start iterating a delegate list, while an iteration is already in progress");

                // this dummy delegate list will always be empty, therefore the iteration attempt will do nothing
                return s_NullIterator;
            }

#if UNITY_EDITOR
            Debug.Assert(m_Thread == System.Threading.Thread.CurrentThread, "Event thread mismatch (CALL A CODER)");
#endif

            m_IterationIndex = k_IterationStart; // index is unsigned so 0xffff will roll over to 0 when incremented
            m_IterationEnd = (ushort)m_Delegates.Count;
            return this;
        }

        public bool MoveNext()
        {
            m_IterationIndex++;

            if (m_IterationIndex == m_IterationEnd)
            {
                m_IterationIndex = k_IterationInvalid;
                return false;
            }

#if UNITY_EDITOR
            if (m_IterationIndex > m_IterationEnd) // shouldn't be possible
            {
                Debug.Assert(false, "FIXME: delegate iteration overrun (ignored in editor)");
                m_IterationIndex = k_IterationInvalid;
                return false;
            }
#endif

            return true;
        }

        public void StopIterating()
        {
            m_IterationIndex = k_IterationInvalid;
        }

        public void Add(T _delegate)
        {
            Debug.Assert(!IsIterating, "Subscribing to Event handler while it is being iterated. This has the potential to go wrong and the functionality is being deprecated!");
            if (IsIterating)
            {

            }

            Debug.Assert(_delegate != null, "Event Subscribe called with null delegate");
            if (_delegate != null)
            {
#if UNITY_EDITOR
                if (m_Delegates.Count >= k_IterationInvalid || m_Delegates.Count >= k_IterationStart)
                {
                    Debug.Assert(false, "DelegateIterator<T> only supports 65533 delegates! Convert m_IterationIndex from byte to short to support more");
                }

                Debug.Assert(m_Thread == System.Threading.Thread.CurrentThread, "Event thread mismatch in Add (CALL A CODER)");
#endif // UNITY_EDITOR				

                m_Delegates.Add(_delegate);
                m_IterationEnd++;
            }
        }

        public void Remove(T _delegate)
        {
            Debug.Assert(_delegate != null, "Event Unsubscribe called with null delegate");
            if (_delegate != null)
            {
#if UNITY_EDITOR
                Debug.Assert(m_Thread == System.Threading.Thread.CurrentThread, "Event thread mismatch in Remove (CALL A CODER)");
#endif // UNITY_EDITOR

                // find it
                int index = m_Delegates.LastIndexOf(_delegate); // use last index, so we search most recently added first!
                if (index >= 0)
                {
                    // remove it
                    m_Delegates.RemoveAt(index);

                    // fixup iteration so that newly removed or added elements don't get called
                    if (IsIterating)
                    {
                        // Only move index back if the removed event is before our current index/end
                        // Also don't allow the splines to go further than 1 below zero (k_IterationStart)
                        if (index <= m_IterationIndex && m_IterationIndex != k_IterationStart)
                            m_IterationIndex--;

                        if (index <= m_IterationEnd)
                            m_IterationEnd--;
                    }
                }
            }
        }
    }
}

