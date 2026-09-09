using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.EventHandling
{
    public class DelegateIterator<T> where T : System.Delegate
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
            Debug.Assert(m_Thread == System.Threading.Thread.CurrentThread, "Event thread mismatch");
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

                Debug.Assert(m_Thread == System.Threading.Thread.CurrentThread, "Event thread mismatch in Add");
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
                Debug.Assert(m_Thread == System.Threading.Thread.CurrentThread, "Event thread mismatch in Remove");
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

