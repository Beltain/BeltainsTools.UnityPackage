// Sourced mostly from: https://www.youtube.com/watch?v=c-XoTg6Fba4
// (or https://github.com/adammyhre/Unity-Hierarchical-StateMachine/tree/master)
// THANK YOU GIT-AMEND YOU LEGEND

using BeltainsTools.EventHandling;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public abstract class State
    {
        public static HashSet<State> s_AncestorCache = new HashSet<State>();

        public readonly StateMachine Machine;
        public readonly State Parent;

        private readonly List<IActivity> m_Activities = new List<IActivity>();
        
        /// <summary>The active regular (non-overlay) child of this state. Overlays never occupy this slot, see <see cref="ActiveOverlays"/>.</summary>
        public State ActiveChild;

        /// <summary>Overlays currently active on this (host) state, ordered bottom to top (push order).</summary>
        public IReadOnlyList<State> ActiveOverlays => m_ActiveOverlays;
        private readonly List<State> m_ActiveOverlays = new List<State>();

        /// <summary>Set by the builder when this state is declared through an <see cref="OverlayStateAttribute"/> field on its parent.</summary>
        internal OverlayModes? m_OverlayMode = null;
        public bool IsOverlay => m_OverlayMode.HasValue;
        public OverlayModes? OverlayMode => m_OverlayMode;

        private PhaseTypes m_Phase = PhaseTypes.Exited;
        private bool m_IsSuspendedSelf = false;

        // activity tracking for the current phase, used for debug readouts
        private int m_PendingActivities = 0; // activities of the current phase that haven't finished yet (incl. running)
        private int m_RunningActivities = 0; // activities of the current phase that are currently executing
        private int m_ActivityEpoch = 0; // invalidates tracking of activities from cancelled phases

        public bool IsSuspendedSelf => m_IsSuspendedSelf;
        public bool IsSuspended { get; private set; } = false;
        /// <summary>Whether this state is currently part of the active tree (entering, entered or exiting)</summary>
        public bool IsActive => m_Phase != PhaseTypes.Exited;
        /// <summary>Whether this state is entering or entered (i.e. active and not on its way out)</summary>
        internal bool IsLive => (m_Phase & PhaseTypes.Suspendable) != 0;


        /// <summary>Called once the state begins its activation sequence. After <see cref="Enter"/>.</summary>
        [System.NonSerialized]
        public BEvent ActivatingEvent;
        /// <summary>Called once the state completes its activation sequence. After <see cref="Enter"/>.</summary>
        [System.NonSerialized]
        public BEvent ActivatedEvent;
        /// <summary>Called once the state begins its deactivation sequence. Before <see cref="Exit"/>.</summary>
        [System.NonSerialized]
        public BEvent DeactivatingEvent;
        /// <summary>Called once the state completes its deactivation sequence. Before <see cref="Exit"/>.</summary>
        [System.NonSerialized]
        public BEvent DeactivatedEvent;

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

            Enterable = Exited | Entering,
            Exitable = Entering | Entered | Exiting,
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

        public override string ToString() => ToString(includeActivationStatus: false);
        public string ToString(bool includeActivationStatus)
        {
            string details = null;
            if (includeActivationStatus)
            {
                string phaseIcon = "";
                if (m_RunningActivities > 0)
                    phaseIcon = m_Phase == PhaseTypes.Exiting ? "^" : "v"; // running exit/entry activities
                else if ((m_Phase & (PhaseTypes.Entering | PhaseTypes.Exiting)) != 0 && m_PendingActivities > 0)
                    phaseIcon = "..."; // waiting for its turn to run entry/exit activities
                details = (IsSuspended ? "[S]" : "") + (phaseIcon.IsNullOrEmpty() ? "" : $"[{phaseIcon}]");
            }
            return GetType().Name + details;
        }

        public string ToStringWithAncestors(bool includeActivationStatus)
        {
            IEnumerable<string> ancestorReadouts = GetAncestors().Reverse().Select(s => s.ToString(includeActivationStatus));
            return string.Join(" / ", ancestorReadouts);
        }

        /// <summary>
        /// Readout of this state and all of its active descendants, including overlays.
        /// <br/>Overlays are shown in braces after their host, bottom to top: '+' for <see cref="OverlayModes.SuspendUnderlying"/>, '~' for <see cref="OverlayModes.RunAlongside"/>.
        /// <br/>e.g. <c>Root/Gameplay{+Pause/PauseMain}/Running[S]</c>
        /// </summary>
        public string ToStringActiveTree(bool includeActivationStatus)
        {
            StringBuilder sb = new StringBuilder();
            AppendActiveTree(sb, includeActivationStatus);
            return sb.ToString();
        }

        private void AppendActiveTree(StringBuilder sb, bool includeActivationStatus)
        {
            sb.Append(ToString(includeActivationStatus));
            foreach (State overlay in m_ActiveOverlays)
            {
                sb.Append(overlay.m_OverlayMode == OverlayModes.RunAlongside ? " {~" : " {+");
                overlay.AppendActiveTree(sb, includeActivationStatus);
                sb.Append('}');
            }
            if (ActiveChild != null)
            {
                sb.Append(" / ");
                ActiveChild.AppendActiveTree(sb, includeActivationStatus);
            }
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

        /// <returns>The activities to run for this phase, wrapped so this state tracks when they're waiting/running for debug readouts</returns>
        internal List<PhaseStep> PrepareActivitySteps(bool deactivate)
        {
            List<PhaseStep> steps = (deactivate ? GetDeactivationActivities() : GetActivationActivities()).ToList();

            m_ActivityEpoch++;
            m_PendingActivities = steps.Count;
            m_RunningActivities = 0;

            int epoch = m_ActivityEpoch;
            for (int i = 0; i < steps.Count; i++)
                steps[i] = TrackActivity(steps[i], epoch);
            return steps;
        }

        private PhaseStep TrackActivity(PhaseStep step, int epoch)
        {
            return async ct =>
            {
                if (epoch == m_ActivityEpoch)
                    m_RunningActivities++;
                try
                {
                    await (step(ct) ?? Task.CompletedTask);
                }
                finally
                {
                    if (epoch == m_ActivityEpoch) // ignore stragglers from cancelled phases
                    {
                        m_RunningActivities--;
                        m_PendingActivities--;
                    }
                }
            };
        }

        private void ClearActivityTracking()
        {
            m_ActivityEpoch++;
            m_PendingActivities = 0;
            m_RunningActivities = 0;
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

        /// <summary>Request that the given overlay is pushed onto its host (its <see cref="Parent"/>)</summary>
        protected void PushOverlay(State overlay) => Machine.Sequencer.RequestPushOverlay(overlay);
        /// <summary>Request that the given active overlay (and all of its active descendants) is popped off its host</summary>
        protected void PopOverlay(State overlay) => Machine.Sequencer.RequestPopOverlay(overlay);

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
            d.AssertFormat((m_Phase & PhaseTypes.Enterable) != 0, "Trying to enter state '{0}' that's not in the Enterable phase! Fix me!", GetType().Name);

            m_Phase = PhaseTypes.Entering;

            //d.LogFormat("Entering State {0}", ToString());

            if (Parent != null)
            {
                if (IsOverlay)
                {
                    d.AssertFormat(!Parent.m_ActiveOverlays.Contains(this), "Trying to enter overlay '{0}' but it's already active on its host '{1}'! Fix me!", GetType().Name, Parent.GetType().Name);
                    Parent.m_ActiveOverlays.Add(this);
                }
                else
                {
                    d.AssertFormat(Parent.ActiveChild == null, "Trying to enter state '{0}' but its parent '{1}' already has an active child '{2}'! Fix me!", GetType().Name, Parent.GetType().Name, Parent.ActiveChild?.GetType().Name);
                    Parent.ActiveChild = this;
                }
            }

            OnEnter();

            UpdateSuspensionStatus(); // update suspension in case our parent is suspended and we are not, ensures this happens before any updates
            if (IsOverlay)
                Parent.RefreshChildSuspension(); // we may now be suspending our host's underlying branches
        }

        internal void BeginActivation()
        {
            ActivatingEvent.Invoke();
            OnActivationBegun();
        }

        internal void CompleteActivation()
        {
            OnActivationComplete();
            ActivatedEvent.Invoke();

            ClearActivityTracking();
            m_Phase = PhaseTypes.Entered;
        }

        protected void Suspend() => SetSuspendedSelf(true);
        protected void Resume() => SetSuspendedSelf(false);
        protected void SetSuspendedSelf(bool suspended)
        {
            m_IsSuspendedSelf = suspended;

            // suspension callbacks may request transitions, defer them until the suspension pass is complete so the tree isn't mutated mid-walk
            TransitionSequencer sequencer = Machine?.Sequencer;
            sequencer?.Lock();
            try { UpdateSuspensionStatus(); }
            finally { sequencer?.Unlock(); }
        }

        /// <returns>Whether the given active child or overlay of this state is suspended by a <see cref="OverlayModes.SuspendUnderlying"/> overlay above it</returns>
        internal bool IsSuppressedByOverlay(State child)
        {
            int start = 0;
            if (child.IsOverlay)
            {
                int index = m_ActiveOverlays.IndexOf(child);
                if (index < 0)
                    return false;
                start = index + 1;
            }

            for (int i = start; i < m_ActiveOverlays.Count; i++)
                if (m_ActiveOverlays[i].m_OverlayMode == OverlayModes.SuspendUnderlying)
                    return true;
            return false;
        }

        /// <summary>Re-evaluates suspension of the active child and overlays, e.g. after our overlay stack changed</summary>
        internal void RefreshChildSuspension()
        {
            ActiveChild?.UpdateSuspensionStatus();
            for (int i = 0; i < m_ActiveOverlays.Count; i++)
                m_ActiveOverlays[i].UpdateSuspensionStatus();
        }

        internal void UpdateSuspensionStatus()
        {
            bool inheritedSuspension = Parent != null && (Parent.IsSuspended || Parent.IsSuppressedByOverlay(this));
            bool isSuspended = m_IsSuspendedSelf || ((m_Phase & PhaseTypes.Suspendable) != 0 && inheritedSuspension);
            if (isSuspended != IsSuspended)
            {
                IsSuspended = isSuspended;

                RefreshChildSuspension();

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
            for (int i = 0; i < m_ActiveOverlays.Count; i++) // then overlays, bottom to top
                m_ActiveOverlays[i].Update(deltaTime);
            OnUpdate(deltaTime); // then us
        }

        internal void FixedUpdate()
        {
            if (IsSuspended)
                return;

            ActiveChild?.FixedUpdate();
            for (int i = 0; i < m_ActiveOverlays.Count; i++)
                m_ActiveOverlays[i].FixedUpdate();
            OnFixedUpdate();
        }

        internal void LateUpdate(float deltaTime)
        {
            if (IsSuspended)
                return;

            ActiveChild?.LateUpdate(deltaTime);
            for (int i = 0; i < m_ActiveOverlays.Count; i++)
                m_ActiveOverlays[i].LateUpdate(deltaTime);
            OnLateUpdate(deltaTime);
        }

        internal void BeginDeactivation()
        {
            m_Phase = PhaseTypes.Exiting;

            DeactivatingEvent.Invoke();
            OnDeactivationBegun();
        }

        internal void CompleteDeactivation()
        {
            OnDeactivationComplete();
            DeactivatedEvent.Invoke();
        }

        internal void Exit()
        {
            d.AssertFormat((m_Phase & PhaseTypes.Exitable) != 0, "Trying to exit state '{0}' that's not in the Exitable (Entering, Entered or Exiting) phase! Fix me!", GetType().Name);
            d.AssertFormat(ActiveChild == null, "Trying to exit state '{0}' that has an active child '{1}'! Fix me!", GetType().Name, ActiveChild?.GetType().Name);
            d.AssertFormat(m_ActiveOverlays.Count == 0, "Trying to exit state '{0}' that has {1} active overlay(s)! Fix me!", GetType().Name, m_ActiveOverlays.Count);

            //d.LogFormat("Exiting State {0}", ToString());

            // exits that skip deactivation (immediate stops) still need to be treated as exiting, 
            // so suspension inherited from parents/overlays is dropped below and OnResume runs before OnExit
            m_Phase = PhaseTypes.Exiting;

            if (Parent != null)
            {
                if (IsOverlay)
                    Parent.m_ActiveOverlays.Remove(this);
                else if (Parent.ActiveChild == this)
                    Parent.ActiveChild = null;
            }

            Resume(); // resume ourselves before exiting to ensure we run our exit logic even if we were suspended before
                      // suspension can be considered a kind of sub-state "lite", so we need to "exit" that sub-state before we can exit the state itself
            OnExit();

            ClearActivityTracking();
            m_Phase = PhaseTypes.Exited;

            if (IsOverlay && Parent != null)
                Parent.RefreshChildSuspension(); // underlying branches may resume now that we're gone
        }

        /// <summary>Adds this state and all of its active descendants (overlays and regular children) to the list, in exit order (top overlay first, deepest first, this state last)</summary>
        internal void CollectActiveSubtree(List<State> into)
        {
            for (int i = m_ActiveOverlays.Count - 1; i >= 0; i--)
                m_ActiveOverlays[i].CollectActiveSubtree(into);
            ActiveChild?.CollectActiveSubtree(into);
            into.Add(this);
        }

        internal void ResetActivities()
        {
            foreach (IActivity activity in m_Activities)
                if (activity is Activity resettable)
                    resettable.ResetStatus();
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
                if (!ReferenceEquals(initialSubState.Parent, current))
                {
                    d.LogWarning($"Initial sub-state '{initialSubState}' of '{current}' is not its direct child! Stopping at '{current}'. Fix me!");
                    return current;
                }
                if (initialSubState.IsOverlay)
                {
                    d.LogWarning($"Initial sub-state '{initialSubState}' of '{current}' is an overlay, overlays can only be pushed! Stopping at '{current}'. Fix me!");
                    return current;
                }
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

        /// <returns>Returns the deepest currently active regular (non-overlay) descendant state relative to this state</returns>
        public State GetLeaf()
        {
            State current = this;
            while (current.ActiveChild != null)
                current = current.ActiveChild;
            return current;
        }
    }
}
