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

        private PhaseTypes m_Phase = PhaseTypes.Exited;
        private bool m_IsSuspendedSelf = false;

        public bool IsSuspendedSelf => m_IsSuspendedSelf;
        public bool IsSuspended { get; private set; } = false;


        /// <summary>Called once the state completes its activation sequence. After <see cref="Enter"/>.</summary>
        [System.NonSerialized]
        public BEvent ActivatedEvent;
        /// <summary>Called once the state begins its deactivation sequence. Before <see cref="Exit"/>.</summary>
        [System.NonSerialized]
        public BEvent DeactivatingEvent;

        /// <summary>
        /// The order of execution for states is broken into phases, which are represented by this enum. The phases are:
        /// <br/>Exited (Deactivated, UnEntered) -> 
        /// <br/>Entering (Busy Activating, UnEntered) -> 
        /// <br/>Entered (Activated, Entered) ->
        /// <br/>Exiting (Busy Deactivating, Entered) ->
        /// </summary>
        [System.Flags]
        internal enum PhaseTypes : byte
        {
            /// <summary>Deactivated, UnEntered</summary>
            Exited = 1 << 0,
            /// <summary>Busy Activating, UnEntered</summary>
            Entering = 1 << 1,
            /// <summary>Activated, Entered</summary>
            Entered = 1 << 2,
            /// <summary>Busy Deactivating, Entered</summary>
            Exiting = 1 << 3,

            Suspendable = Entered | Entering,
        }

        public static State GetLowestCommonAnscestor(State a, State b)
        {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b.GetAncestors().Last();
            if (b == null)
                return a.GetAncestors().Last();

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

        /// <returns>An IEnumerable of states to exit if we were transitioning from the "from" state to the "to" state, passing through a lowest common ancestor</returns>
        public static IEnumerable<State> GetExitChain(State from, State to)
        {
            if (from == null)
                yield break;

            State lca = GetLowestCommonAnscestor(from, to);
            foreach (State stateToExit in from.WalkUpTo(lca, inclusive: to == null)) // also exit lca if to is null. We're essentially exiting the entire state machine.
                yield return stateToExit;
        }

        /// <returns>An IEnumerable of states to enter if we were transitioning from the "from" state to the "to" state, passing through a lowest common ancestor</returns>
        public static IEnumerable<State> GetEnterChain(State from, State to)
        {
            if (to == null)
                yield break;

            State lca = GetLowestCommonAnscestor(from, to);
            foreach (State stateToEnter in lca.WalkDownTo(to, inclusive: from == null)) // also enter the lca if from is null. We're essentially entering the state machine for the first time
                yield return stateToEnter;
        }

        public State(StateMachine machine, State parent)
        {
            Machine = machine;
            Parent = parent;
        }

        public override string ToString()
        {
            string phaseIcon = m_Phase switch
            {
                PhaseTypes.Entering => "v",
                PhaseTypes.Exiting => "^",
                _ => ""
            };
            return GetType().Name + (IsSuspended ? "[S]" : "") + (phaseIcon.IsNullOrEmpty() ? "" : $"[{phaseIcon}]");
        }

        public string ToStringWithAncestors()
        {
            IEnumerable<string> ancestorReadouts = GetAncestors().Reverse().Select(s => s.ToString());
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
        
        public void AddActivationActivity(System.Action activationAction) => AddActivity(activationAction, null);
        public void AddDeactivationActivity(System.Action deactivationAction) => AddActivity(null, deactivationAction);
        public void AddActivity(System.Action activationAction, System.Action deactivationAction)
            => AddActivity(new ActionActivity(activationAction, deactivationAction));

        public void AddActivity(IActivity activity)
        {
            if (activity == null)
                return;
            m_Activities.Add(activity);
        }

        /// <summary>Request a transition from this state to the target state</summary>
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
        /// <inheritdoc cref="ActivatingEvent"/>
        protected virtual void OnActivationBegun() { }
        /// <inheritdoc cref="ActivatedEvent"/>
        protected virtual void OnActivationComplete() { }
        protected virtual void OnSuspend() { }
        protected virtual void OnResume() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate(float deltaTime) { }
        /// <inheritdoc cref="DeactivatingEvent"/>
        protected virtual void OnDeactivationBegun() { }
        /// <inheritdoc cref="DeactivatedEvent"/>
        protected virtual void OnDeactivationComplete() { }
        protected virtual void OnExit() { }

        internal void Enter()
        {
            d.AssertFormat(m_Phase == PhaseTypes.Exited, "Trying to enter state '{0}' that's not in the Exited phase! Fix me!", GetType().Name);

            m_Phase = PhaseTypes.Entering;

            //d.LogFormat("Entering State {0}", ToString());

            if (Parent != null)
            {
                d.AssertFormat(Parent.ActiveChild == null, "Trying to enter state '{0}' but its parent '{1}' already has an active child '{2}'! Fix me!", GetType().Name, Parent.GetType().Name, Parent.ActiveChild?.GetType().Name);
                Parent.ActiveChild = this;
            }

            OnEnter();

            UpdateSuspensionStatus(); // update suspension in case our parent is suspended and we are not, ensures this happens before any updates
        }

        internal void CompleteActivation()
        {
            OnActivationComplete();
            ActivatedEvent.Invoke();

            m_Phase = PhaseTypes.Entered;
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
            bool isSuspended = m_IsSuspendedSelf || ((m_Phase & PhaseTypes.Suspendable) != 0 && (Parent?.IsSuspended ?? false));
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
            m_Phase = PhaseTypes.Exiting;

            DeactivatingEvent.Invoke();
            OnDeactivationBegun();
        }

        internal void Exit()
        {
            d.AssertFormat(m_Phase == PhaseTypes.Exiting, "Trying to exit state '{0}' that's not in the Exiting (Deactivating) phase! Fix me!", GetType().Name);
            d.AssertFormat(ActiveChild == null, "Trying to exit state '{0}' that has an active child '{1}'! Fix me!", GetType().Name, ActiveChild?.GetType().Name);

            //d.LogFormat("Exiting State {0}", ToString());

            if (Parent != null && Parent.ActiveChild == this)
                Parent.ActiveChild = null;

            Resume(); // resume ourselves before exiting to ensure we run our exit logic even if we were suspended before
                      // suspension can be considered a kind of sub-state "lite", so we need to "exit" that sub-state before we can exit the state itself
            OnExit();

            m_Phase = PhaseTypes.Exited;
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
