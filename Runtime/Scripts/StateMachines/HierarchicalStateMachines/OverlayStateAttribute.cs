namespace BeltainsTools.StateMachines.HSM
{
    public enum OverlayModes
    {
        /// <summary>While active, suspends the host's regular active child and any overlays beneath it. The host itself keeps running.</summary>
        SuspendUnderlying,
        /// <summary>While active, runs alongside the host's regular active child and any overlays beneath it.</summary>
        RunAlongside,
    }

    /// <summary>
    /// Marks a child state field as an overlay of the declaring (host) state. 
    /// Overlays are never entered through normal transitions or initial sub-states, they are pushed/popped on their host 
    /// and live alongside the host's regular <see cref="State.ActiveChild"/>.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class OverlayStateAttribute : System.Attribute
    {
        public readonly OverlayModes Mode;

        public OverlayStateAttribute(OverlayModes mode = OverlayModes.SuspendUnderlying)
        {
            Mode = mode;
        }
    }
}
