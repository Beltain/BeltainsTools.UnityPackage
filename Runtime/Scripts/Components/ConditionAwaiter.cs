using System.Collections;
using UnityEngine;

namespace BeltainsTools
{
    public abstract class ConditionAwaiter : MonoBehaviour
    {
        [SerializeField, Tooltip("How long we should wait to be ready before giving up. If negative, we wait indefinitely")]
        private float m_AwaitTimeout = -1f;

        private bool m_Readied = false;

        protected abstract void OnReady();
        protected abstract bool GetIsReady();

        private void TryReady()
        {
            if (!m_Readied && GetIsReady())
            {
                m_Readied = true;
                OnReady();
            }
        }

        private void Awake()
        {
            TryReady();
        }

        void Start()
        {
            TryReady();

            if (!m_Readied)
                Coroutines.ConditionDelayedAction.Execute(TryReady, () => GetIsReady(), this, m_AwaitTimeout);
        }
    }
}

