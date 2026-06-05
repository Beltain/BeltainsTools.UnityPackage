using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    /// <summary>Handles asynchronous behaviour to execute in parallel or sequence when exiting or entering states of a HSM.</summary>
    /// <remarks>This ensures that the states themselves are decoupled from any async executions, and exist as more concrete things that just define the current behaviour</remarks>
    public interface IActivity
    {
        public enum StatusTypes
        {
            Inactive,
            Activating,
            Active,
            Deactivating
        }

        StatusTypes Status { get; }
        Task ActivateAsync(CancellationToken ct);
        Task DeactivateAsync(CancellationToken ct);
    }

    public abstract class Activity : IActivity
    {
        public IActivity.StatusTypes Status { get; protected set; } = IActivity.StatusTypes.Inactive;

        protected virtual async Task OnActivateAsync(CancellationToken ct) => await Task.CompletedTask;
        async Task IActivity.ActivateAsync(CancellationToken ct)
        {
            if (Status != IActivity.StatusTypes.Inactive)
                return;

            Status = IActivity.StatusTypes.Activating;
            await OnActivateAsync(ct);
            Status = IActivity.StatusTypes.Active;
        }

        protected virtual async Task OnDeactivateAsync(CancellationToken ct) => await Task.CompletedTask;
        async Task IActivity.DeactivateAsync(CancellationToken ct)
        {
            if (Status != IActivity.StatusTypes.Active)
                return;

            Status = IActivity.StatusTypes.Deactivating;
            await OnDeactivateAsync(ct);
            Status = IActivity.StatusTypes.Inactive;
        }
    }
}
