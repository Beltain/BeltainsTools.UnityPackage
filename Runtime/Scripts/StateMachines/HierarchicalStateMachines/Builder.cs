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

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            
            // set state machine refs in state
            FieldInfo machineField = typeof(State).GetField("Machine", flags);
            if (machineField != null) 
                machineField.SetValue(state, machine);

            // find all state fields and wire them up, skipping parent back references
            foreach (FieldInfo field in state.GetType().GetFields(flags))
            {
                if (!typeof(State).IsAssignableFrom(field.FieldType)) 
                    continue; // Only consider fields that are State
                if (field.Name == "Parent") 
                    continue; // Skip back-edge to parent 

                var child = (State)field.GetValue(state);
                if (child == null) 
                    continue;
                if (!ReferenceEquals(child.Parent, state)) 
                    continue; // Ensure it's actually our direct child

                Wire(child, machine, visited); // Recurse into the child
            }
        }
    }
}
