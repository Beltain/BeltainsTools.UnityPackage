using UnityEngine;

namespace BeltainsTools.EventHandling
{
    /// <summary>Shared, type-agnostic helpers for the BEvent family to avoid duplicated code.</summary>
    internal static class BEventHelper
    {
        public static void Subscribe<TDelegate>(ref DelegateIterator<TDelegate> list, TDelegate _delegate) where TDelegate : System.Delegate
        {
            if (list == null)
                list = new DelegateIterator<TDelegate>();

            Debug.Assert(!list.Contains(_delegate), "Event subscriber is DUPLICATED!");

            list.Add(_delegate);
        }

        public static void Unsubscribe<TDelegate>(ref DelegateIterator<TDelegate> list, TDelegate _delegate) where TDelegate : System.Delegate
        {
            if (list == null)
                return;

            list.Remove(_delegate);

            if (list.Count == 0)
                list = null;
        }

        /// <summary>Iterates the list, invoking each delegate via <paramref name="invokeAction"/>, and safely stops iterating.</summary>
        public static void Invoke<TDelegate>(DelegateIterator<TDelegate> list, System.Action<TDelegate> invokeAction) where TDelegate : System.Delegate
        {
            if (list == null)
                return;

            try
            {
                foreach (var _delegate in list)
                    invokeAction(_delegate);
            }
            finally
            {
                // can be null if event is unsubscribed during sending event!
                list?.StopIterating();
            }
        }

        public static void PrintSubscribers<TDelegate>(DelegateIterator<TDelegate> list, string contextMsg) where TDelegate : System.Delegate
        {
            int count = list != null ? list.Count : 0;
            string msg = contextMsg + " Current subscribers (" + count + "):";
            if (list != null)
            {
                foreach (var _delegate in list)
                {
                    msg += "\n\t[ object '" + _delegate.Target + "' method '" + _delegate.Method + "' ],";
                }
            }
        }

        public static void EnsureNoSubscribers<TDelegate>(ref DelegateIterator<TDelegate> list) where TDelegate : System.Delegate
        {
#if UNITY_EDITOR
            if (list != null)
            {
                Debug.Log("List of subscribers:");

                foreach (var _delegate in list)
                    Debug.LogFormat("\tobject \"{0}\" method \"{1}\"", _delegate.Target, _delegate.Method);

                Debug.Assert(false, "Event has subscribers during call to EnsureNoSubscribers");
            }
#endif // UNITY_EDITOR

            list = null;
        }
    }
}

