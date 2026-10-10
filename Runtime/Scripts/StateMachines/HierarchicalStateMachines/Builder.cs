// Sourced mostly from: https://www.youtube.com/watch?v=c-XoTg6Fba4
// THANK YOU GIT-AMEND YOU LEGEND

using System.Collections.Generic;
using System.Reflection;

namespace BeltainsTools.StateMachines.HSM
{
    /// <remarks>
    /// Honestly hate this class, need to figure a way of doing this without reflection 
    /// so that we make less assumptions about the structure of the state machine 
    /// and can have more flexible state definitions. 
    /// </remarks>
    public class StateMachineBuilder
    {
        readonly State m_Root;

        public StateMachineBuilder(State root)
        {
            this.m_Root = root;
        }

        public StateMachine Build(TransitionSequencer.SequencingModes sequencingMode = TransitionSequencer.SequencingModes.Sequential)
        {
            StateMachine machine = new StateMachine(m_Root, sequencingMode);
            Wire(m_Root, machine, new HashSet<State>());
            return machine;
        }

        void Wire(State state, StateMachine machine, HashSet<State> visited)
        {
            if (state == null) 
                return;
            if (!visited.Add(state)) 
                return; // State is already wired

            // set state machine refs in state
            FieldInfo machineField = typeof(State).GetField(nameof(State.Machine), BindingFlags.Instance | BindingFlags.Public);
            if (machineField != null) 
                machineField.SetValue(state, machine);

            HashSet<State> seenChildren = new HashSet<State>();

            // find all state fields and wire them up, skipping parent back references
            foreach (FieldInfo field in GetAllInstanceFields(state.GetType()))
            {
                if (!typeof(State).IsAssignableFrom(field.FieldType)) 
                    continue; // Only consider fields that are State
                if (field.DeclaringType == typeof(State)) 
                    continue; // Skip State's own fields (Parent back-edge, ActiveChild)

                OverlayStateAttribute overlayAttribute = field.GetCustomAttribute<OverlayStateAttribute>(true);
                if (field.IsDefined(typeof(ExcludeFromChildrenAttribute), true))
                {
                    if (overlayAttribute != null)
                        d.LogWarning($"Field '{field.DeclaringType.Name}.{field.Name}' is marked both [ExcludeFromChildren] and [OverlayState], it will be excluded. Fix me!");
                    continue; // Explicitly excluded from the hierarchy
                }

                var child = (State)field.GetValue(state);
                if (child == null) 
                    continue;
                if (!ReferenceEquals(child.Parent, state))
                {
                    if (overlayAttribute != null)
                        d.LogWarning($"Overlay field '{field.DeclaringType.Name}.{field.Name}' holds '{child}' whose parent isn't '{state}'. Overlays must be direct children of their host, ignoring. Fix me!");
                    continue; // Ensure it's actually our direct child
                }

                OverlayModes? mode = overlayAttribute?.Mode;
                if (!seenChildren.Add(child))
                {
                    if (child.m_OverlayMode != mode)
                        d.LogWarning($"State '{child}' is referenced by multiple fields on '{state}' with conflicting [OverlayState] declarations, keeping the first ({(child.m_OverlayMode.HasValue ? child.m_OverlayMode.ToString() : "regular child")}). Fix me!");
                    continue;
                }
                child.m_OverlayMode = mode;

                Wire(child, machine, visited); // Recurse into the child
            }
        }

        /// <summary>All instance fields declared on the type and its base types, including private ones (which FlattenHierarchy doesn't return for base types)</summary>
        static IEnumerable<FieldInfo> GetAllInstanceFields(System.Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (System.Type current = type; current != null && current != typeof(object); current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags))
                    yield return field;
        }
    }
}
