using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public class DelayActivity : Activity
    {
        private readonly float m_ActivationDelay;
        private readonly float m_DeactivationDelay;

        public DelayActivity(float activationDelay, float deactivationDelay)
        {
            m_ActivationDelay = activationDelay;
            m_DeactivationDelay = deactivationDelay;
        }

        protected override async Task OnActivateAsync(CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(m_ActivationDelay));
        }

        protected override async Task OnDeactivateAsync(CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(m_DeactivationDelay));
        }
    }
}
