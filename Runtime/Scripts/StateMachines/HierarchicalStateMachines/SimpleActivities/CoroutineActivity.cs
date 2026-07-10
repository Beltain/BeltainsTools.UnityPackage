using BeltainsTools.Coroutines;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public class CoroutineActivity : Activity
    {
        private MonoBehaviour m_Owner;
        private System.Func<IEnumerator> m_ActivateCoroutineFactory;
        private System.Func<IEnumerator> m_DeactivateCoroutineFactory;

        public CoroutineActivity(System.Func<IEnumerator> activateCoroutine, System.Func<IEnumerator> deactivateCoroutine, MonoBehaviour owner)
        {
            m_Owner = owner;
            m_ActivateCoroutineFactory = activateCoroutine;
            m_DeactivateCoroutineFactory = deactivateCoroutine;
        }

        protected override async Task OnActivateAsync(CancellationToken ct)
        {
            if (m_ActivateCoroutineFactory == null)
                return;
            await ToTask(m_ActivateCoroutineFactory(), ct);
        }

        protected override async Task OnDeactivateAsync(CancellationToken ct)
        {
            if (m_DeactivateCoroutineFactory == null)
                return;
            await ToTask(m_DeactivateCoroutineFactory(), ct);
        }

        private Task ToTask(IEnumerator coroutine, CancellationToken ct = default)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (m_Owner == null)
            {
                RunHandle handle = CoroutineRunner.Run(coroutine, (success) => {
                    tcs.TrySetResult(success);
                });

                ct.Register(() =>
                {
                    handle.Stop(false);
                    tcs.TrySetCanceled();
                });

                return tcs.Task;
            }
            else
            {
                ct.Register(() =>
                {
                    m_Owner.StopCoroutine(coroutine);
                    tcs.TrySetCanceled();
                });

                m_Owner.StartCoroutine(RunCoroutineWithCallback(coroutine, () => tcs.TrySetResult(true)));
            }

            return tcs.Task;
        }

        private static IEnumerator RunCoroutineWithCallback(IEnumerator coroutine, Action onComplete)
        {
            yield return coroutine;
            onComplete?.Invoke();
        }
    }
}
