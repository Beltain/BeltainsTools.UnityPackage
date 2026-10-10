using System;

namespace BeltainsTools.StateMachines.HSM
{
    /// <summary>
    /// When applied to a <see cref="State"/> field within a state, the <see cref="StateMachineBuilder"/>
    /// will not wire that field's state (or its subtree) as a child when building the HSM.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ExcludeFromChildrenAttribute : Attribute { }
}