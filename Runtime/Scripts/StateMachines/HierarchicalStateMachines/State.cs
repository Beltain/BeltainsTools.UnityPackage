// Sourced mostly from: https://www.youtube.com/watch?v=c-XoTg6Fba4
// (or https://github.com/adammyhre/Unity-Hierarchical-StateMachine/tree/master)
// THANK YOU GIT-AMEND YOU LEGEND

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public abstract class State
    {
        public static HashSet<State> s_AncestorCache = new HashSet<State>();

        public readonly StateMachine Machine;
        public readonly State Parent;
        public State ActiveChild;

        private readonly List<IActivity> m_Activities = new List<IActivity>();

        public IReadOnlyList<IActivity> Activities => m_Activities;


        public static State GetLowestCommonAnscestor(State a, State b)
        {
            foreach (State aAncestor in a.GetAncestors())
                s_AncestorCache.Add(aAncestor);

            foreach (State bAncestor in b.GetAncestors())
            {
                if (s_AncestorCache.Contains(bAncestor))
                {
                    s_AncestorCache.Clear();
                    return bAncestor;
                }
            }

            s_AncestorCache.Clear();
            d.LogWarning($"States {a} and {b} are not in the same state machine, so they have no common ancestor but are being called as if they do! Address me!");
            return null;
        }

        public State(StateMachine machine, State parent)
        {
            Machine = machine;
            Parent = parent;
        }

        public override string ToString()
        {
            return string.Join(" > ", GetAncestors().Reverse().Select(s => s.GetType().Name));
        }

        public void AddActivity(IActivity activity)
        {
            if (activity == null)
                return;
            m_Activities.Add(activity);
        }

        /// <returns>The initial sub-state of this state, or null if we're a leaf state</returns>
        protected virtual State GetInitialSubState() => null;
        /// <returns>Which state to transition to this frame, or null if we shouldn't</returns>
        protected virtual State GetTransition() => null;

        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate(float deltaTime) { }

        internal void Enter() // parent enters before child
        {
            if (Parent != null)
                Parent.ActiveChild = this;
            OnEnter();
            State initialSubState = GetInitialSubState();
            initialSubState?.Enter();
        }

        internal void Exit() // depth first exit
        {
            ActiveChild?.Exit();
            ActiveChild = null;
            OnExit();
        }

        internal void Update(float deltaTime)
        {
            State transitionState = GetTransition();
            if (transitionState != null)
            {
                Machine.Sequencer.RequestTransition(this, transitionState);
                return;
            }

            ActiveChild?.Update(deltaTime); // child first
            OnUpdate(deltaTime); // then us
        }

        internal void FixedUpdate()
        {
            ActiveChild?.FixedUpdate();
            OnFixedUpdate();
        }

        internal void LateUpdate(float deltaTime)
        {
            ActiveChild?.LateUpdate(deltaTime);
            OnLateUpdate(deltaTime);
        }

        /// <returns>Returns the deepest currently active descendant state relative to this state</returns>
        public State GetLeaf()
        {
            State current = this;
            while (current.ActiveChild != null) 
                current = current.ActiveChild;
            return current;
        }

        /// <inheritdoc cref="GetLowestCommonAnscestor(State, State)"/>
        public State GetLowestCommonAncestor(State other)
        {
            return GetLowestCommonAnscestor(this, other);
        }

        /// <summary>Assumes an ancestor is passed, walks up the chain of states and returns them until it reaches the ancestor</summary>
        /// <remarks>If the passed state is not an ancestor, walks the entire tree until the root</remarks>
        /// <param name="inclusive">Whether to include the ancestor in the returned enumerable</param>
        public IEnumerable<State> WalkUpTo(State ancestor, bool inclusive)
        {
            State current = this;
            while (current != null)
            {
                if (current != ancestor || inclusive)
                    yield return current;
                if (current == ancestor)
                    yield break;
                current = current.Parent;
            }
        }

        /// <summary>Assumes a descendant is passed, walks down the chain of states and returns them until it reaches the descendant</summary>
        /// <remarks>If the passed state is not a descendant, walks the entire tree until a leaf</remarks>
        /// <param name="inclusive">Whether to include this (starting) state in the returned enumerable</param>
        public IEnumerable<State> WalkDownTo(State descendant, bool inclusive)
        {
            Stack<State> stack = new Stack<State>();
            foreach (State state in descendant.WalkUpTo(this, inclusive))
                stack.Push(state);
            while (stack.Count > 0)
                yield return stack.Pop();
        }

        /// <summary>Walks up the state chain and returns all ancestors of this state, starting with this state and ending with the root</summary>
        public IEnumerable<State> GetAncestors()
        {
            for (State current = this; current != null; current = current.Parent)
                yield return current;
        }

        /// <summary>Walks down the state chain and returns all descendants of this state, starting with this state and ending with the deepest active descendant</summary>
        public IEnumerable<State> GetDescendants()
        {
            for (State current = this; current != null; current = current.ActiveChild)
                yield return current;
        }
    }
}
