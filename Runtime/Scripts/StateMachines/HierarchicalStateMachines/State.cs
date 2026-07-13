// Sourced mostly from: https://www.youtube.com/watch?v=c-XoTg6Fba4
// (or https://github.com/adammyhre/Unity-Hierarchical-StateMachine/tree/master)
// THANK YOU GIT-AMEND YOU LEGEND

using BeltainsTools.EventHandling;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public abstract class State
    {
        public static HashSet<State> s_AncestorCache = new HashSet<State>();

        public readonly StateMachine Machine;
        public readonly State Parent;

        private readonly List<IActivity> m_Activities = new List<IActivity>();
        
        public State ActiveChild;

        private bool m_IsSuspendedSelf = false;

        public bool IsSuspendedSelf => m_IsSuspendedSelf;
        public bool IsSuspended { get; private set; } = false;


        /// <summary>Called once the state completes its activation sequence. After <see cref="Enter"/>.</summary>
        [System.NonSerialized]
        public BEvent ActivatedEvent;
        /// <summary>Called once the state begins its deactivation sequence. Before <see cref="Exit"/>.</summary>
        [System.NonSerialized]
        public BEvent DeactivatingEvent;


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
            IEnumerable<State> ancestors = GetAncestors();
            IEnumerable<string> ancestorReadouts = ancestors.Reverse().Select(s => s.GetType().Name + (s.IsSuspended ? "[S]" : ""));
            return string.Join(" > ", ancestorReadouts);
        }

        /// <returns>All activities that need to be activated</returns>
        public IEnumerable<PhaseStep> GetActivationActivities()
        {
            foreach (IActivity activity in m_Activities)
                if (activity.Status == IActivity.StatusTypes.Inactive)
                    yield return activity.ActivateAsync;
        }

        /// <returns>All activities that need to be deactivated</returns>
        public IEnumerable<PhaseStep> GetDeactivationActivities()
        {
            foreach (IActivity activity in m_Activities)
                if (activity.Status == IActivity.StatusTypes.Active)
                    yield return activity.DeactivateAsync;
        }

        public void AddActivationActivity(System.Func<IEnumerator> coroutineFactory, MonoBehaviour owner = null) => AddActivity(coroutineFactory, null, owner);
        public void AddDeactivationActivity(System.Func<IEnumerator> coroutineFactory, MonoBehaviour owner = null) => AddActivity(null, coroutineFactory, owner);
        public void AddActivity(System.Func<IEnumerator> activationCoroutineFactory, System.Func<IEnumerator> deactivationCoroutineFactory, MonoBehaviour owner = null)
            => AddActivity(new CoroutineActivity(activationCoroutineFactory, deactivationCoroutineFactory, owner));

        public void AddActivity(IActivity activity)
        {
            if (activity == null)
                return;
            m_Activities.Add(activity);
        }

        protected void TransitionTo(State targetState)
        {
            d.Assert(targetState != null, "Cannot transition to a null state!");
            Machine.Sequencer.RequestTransition(this, targetState);
        }

        /// <returns>The initial sub-state of this state, or null if we're a leaf state</returns>
        protected virtual State GetInitialSubState() => null;
        /// <returns>Which state to transition to this frame, or null if we shouldn't</returns>
        protected virtual State GetTransition() => null;

        protected virtual void OnEnter() { }
        /// <inheritdoc cref="ActivatedEvent"/>
        protected virtual void OnActivationComplete() { }
        protected virtual void OnSuspend() { }
        protected virtual void OnResume() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate(float deltaTime) { }
        /// <inheritdoc cref="DeactivatingEvent"/>
        protected virtual void OnDeactivationBegun() { }
        protected virtual void OnExit() { }

        internal void Enter() // parent enters before child (parents enter, then suspend if necessary, then children enter, then active children suspend if necessary, all the way down the hierarchy)
        {
            if (Parent != null)
                Parent.ActiveChild = this;
            OnEnter();

            UpdateSuspensionStatus(); // update suspension in case our parent is suspended and we are not, ensures this happens before any updates
                                      // do this parents first

            GetInitialSubState()?.Enter(); // Should anyways be covered by the state machine's transition sequencer,
                                           // but just incase we are running raw state machine updates without the sequencer,
                                           // we will manually enter the initial sub-state here
        }

        internal void CompleteActivation()
        {
            OnActivationComplete();
            ActivatedEvent.Invoke();
        }

        protected void Suspend() => SetSuspendedSelf(true);
        protected void Resume() => SetSuspendedSelf(false);
        protected void SetSuspendedSelf(bool suspended)
        {
            m_IsSuspendedSelf = suspended;
            UpdateSuspensionStatus();
        }

        internal void UpdateSuspensionStatus()
        {
            bool isSuspended = m_IsSuspendedSelf || (Parent?.IsSuspended ?? false);
            if (isSuspended != IsSuspended)
            {
                IsSuspended = isSuspended;

                ActiveChild?.UpdateSuspensionStatus();

                if (IsSuspended)
                    OnSuspend();
                else
                    OnResume();
            }
        }

        internal void Update(float deltaTime)
        {
            if (IsSuspended)
                return;

            State transitionState = GetTransition();
            if (transitionState != null)
            {
                TransitionTo(transitionState);
                return;
            }

            ActiveChild?.Update(deltaTime); // child first
            OnUpdate(deltaTime); // then us
        }

        internal void FixedUpdate()
        {
            if (IsSuspended)
                return;

            ActiveChild?.FixedUpdate();
            OnFixedUpdate();
        }

        internal void LateUpdate(float deltaTime)
        {
            if (IsSuspended)
                return;

            ActiveChild?.LateUpdate(deltaTime);
            OnLateUpdate(deltaTime);
        }

        internal void BeginDeactivation()
        {
            DeactivatingEvent.Invoke();
            OnDeactivationBegun();
        }

        internal void Exit() // depth first exit (leaf resumes, exits, then parents resume and exit all the way up the chain)
        {
            // resume just before OnExit to ensure we Exit as the absolute last step of the deactivation sequence,
            // beven if we were suspended before, this cleans us up right before exiting
            // v This is a little trick to ensure our children can unsuspend,
            // v if we left ourselves suspended before exiting, they would be locked in suspension
            bool wasSuspended = IsSuspended;
            IsSuspended = false; // temporarily unsuspend ourselves so that our children can unsuspend themselves and run their exit logic, even if we were suspended before
            //

            ActiveChild?.Exit();
            ActiveChild = null;

            IsSuspended = wasSuspended;
            Resume(); // now that we've reached the leaf, we can fire our actual unsuspension logic.
                      // Doing it this way ensures all the parents are unsuspended and don't block child unsuspension
                      // while children also fire their unsuspension logic depth-first, inline with our exit logic ordering

            OnExit();
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

        public State GetLowestInitialSubState()
        {
            State current = this;
            while (true)
            {
                State initialSubState = current.GetInitialSubState();
                if (initialSubState == null)
                    return current;
                current = initialSubState;
            }
        }

        /// <inheritdoc cref="GetAncestor(System.Type)"/>
        public T GetAncestor<T>() where T : State => (T)GetAncestor(typeof(T));
        /// <returns>The first <see cref="Parent"/> that matches the provided <see cref="State"/> type</returns>
        public State GetAncestor(System.Type type)
        {
            d.Assert(type.IsSubclassOf(typeof(State)), "Ancestor State Type must be a subclass of State when trying to get state ancestor!");
            foreach (State parent in GetAncestors())
            {
                if (type.IsAssignableFrom(parent.GetType()))
                    return parent;
            }
            return null;
        }

        /// <summary>Walks up the state chain and returns all ancestors of this state, starting with this state and ending with the root</summary>
        public IEnumerable<State> GetAncestors()
        {
            for (State current = this; current != null; current = current.Parent)
                yield return current;
        }

        /// <returns>Returns the deepest currently active descendant state relative to this state</returns>
        public State GetLeaf()
        {
            State current = this;
            while (current.ActiveChild != null)
                current = current.ActiveChild;
            return current;
        }
    }
}
