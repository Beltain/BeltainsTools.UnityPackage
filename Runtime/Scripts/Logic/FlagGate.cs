using BeltainsTools.EventHandling;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Logic
{
    /// <summary>
    /// Boolean value that can be enabled/disabled by setting/removing enable/disable tokens.<br/>
    /// Default value is <i>enabled</i>,<br/>
    /// <b>Disable</b> tokens change the value to <i>disabled</i>,<br/>
    /// <b>Enable</b> tokens force the value back to <i>enabled</i>.
    /// </summary>
    public class FlagGate : System.IDisposable
    {
        public bool IsEnabled { get; private set; } = false;

        private HashSet<Token> m_EnableTokens = new HashSet<Token>();
        private HashSet<Token> m_DisableTokens = new HashSet<Token>();

        private bool m_SuppressEvents = false;

        public BEvent<bool> EnabledStatusChangedEvent;

        public class Token : System.IDisposable
        {
            private readonly System.Action<Token> OnRemovedCallback;

            public Token(System.Action<Token> onRemovedCallback)
            {
                OnRemovedCallback = onRemovedCallback;
            }

            public void Remove() => Dispose();
            public void Dispose()
            {
                OnRemovedCallback.Invoke(this);
            }
        }

        public static implicit operator bool(FlagGate gate)
        {
            return gate.IsEnabled;
        }

        public FlagGate()
        {
            IsEnabled = GetEnabled();
        }

        public void Dispose()
        {
            m_SuppressEvents = true;
            for (int i = m_EnableTokens.Count - 1; i >= 0; i--)
                m_EnableTokens.ElementAt(i).Dispose();
            for (int i = m_DisableTokens.Count - 1; i >= 0; i--)
                m_DisableTokens.ElementAt(i).Dispose();
        }

        #region Useful Observer Methods    Useful Observer Methods    Useful Observer Methods    Useful Observer Methods    Useful Observer Methods    Useful Observer Methods
        public void SubscribeAndInherit(System.Action<bool> onEnabledStatusChangedSubscriber)
        {
            Subscribe(onEnabledStatusChangedSubscriber);
            if (!m_SuppressEvents)
                onEnabledStatusChangedSubscriber?.Invoke(IsEnabled);
        }

        public void Subscribe(System.Action<bool> onEnabledStatusChangedSubscriber)
        {
            EnabledStatusChangedEvent.Subscribe(onEnabledStatusChangedSubscriber);
        }

        public void Unsubscribe(System.Action<bool> onEnabledStatusChangedSubscriber)
        {
            EnabledStatusChangedEvent.Unsubscribe(onEnabledStatusChangedSubscriber);
        }
        #endregion

        public void SetEnable(bool set, ref Token token, System.Action<Token> onRemovedCallback)
        {
            if (set && token == null)
                token = Enable(onRemovedCallback);
            else if (!set && token != null)
                token.Dispose();
        }

        /// <summary>Sets an enable flag</summary>
        /// <returns>A token for removing the flag</returns>
        public Token Enable(System.Action<Token> onRemovedCallback)
        {
            Token token = new Token((removedToken) =>
            {
                if (m_EnableTokens.Remove(removedToken))
                    RefreshEnabled();
                onRemovedCallback?.Invoke(removedToken);
            });
            if (m_EnableTokens.Add(token))
                RefreshEnabled();
            return token;
        }

        public void SetDisable(bool set, ref Token token, System.Action<Token> onRemovedCallback)
        {
            if (set && token == null)
                token = Disable(onRemovedCallback);
            else if (!set && token != null)
                token.Dispose();
        }

        /// <summary>Sets a disable flag</summary>
        /// <returns>A token for removing the flag</returns>
        public Token Disable(System.Action<Token> onRemovedCallback)
        {
            Token token = new Token((removedToken) =>
            {
                if (m_DisableTokens.Remove(removedToken))
                    RefreshEnabled();
                onRemovedCallback?.Invoke(removedToken);
            });
            if (m_DisableTokens.Add(token))
                RefreshEnabled();
            return token;
        }

        private bool GetEnabled()
        {
            return m_EnableTokens.Count > 0 || m_DisableTokens.Count == 0;
        }

        private void RefreshEnabled()
        {
            bool newEnabled = GetEnabled();
            if (newEnabled == IsEnabled)
                return;
            IsEnabled = newEnabled;

            if (!m_SuppressEvents)
                EnabledStatusChangedEvent.Invoke(IsEnabled);
        }
    }
}
