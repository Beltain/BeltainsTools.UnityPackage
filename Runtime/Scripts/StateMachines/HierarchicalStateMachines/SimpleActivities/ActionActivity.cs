using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public class ActionActivity : Activity
    {
        private readonly System.Action m_EnterActionToExecute;
        private readonly System.Action m_ExitActionToExecute;

        public ActionActivity(System.Action enterActionToExecute, System.Action exitActionToExecute)
        {
            m_EnterActionToExecute = enterActionToExecute;
            m_ExitActionToExecute = exitActionToExecute;
        }

        protected override Task OnActivateAsync(CancellationToken ct)
        {
            m_EnterActionToExecute?.Invoke();
            return Task.CompletedTask;
        }

        protected override Task OnDeactivateAsync(CancellationToken ct)
        {
            m_ExitActionToExecute?.Invoke();
            return Task.CompletedTask;
        }
    }
}
