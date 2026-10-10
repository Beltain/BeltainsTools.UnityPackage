using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BeltainsTools.StateMachines.HSM
{
    public class TransitionSequencer
    {
        public readonly StateMachine Machine;
        private readonly SequencingModes m_SequencingMode;

        private readonly PhaseExecutor m_PhaseExecutor = new PhaseExecutor(); // handles the sub-stages of a transition, the "phases"
        private readonly Queue<Request> m_Queue = new Queue<Request>(); // requests waiting for the active one to finish, FIFO
        private Request m_Active; // request currently running
        private bool m_HasActive = false;

        // resolved when a request begins (not when it's requested) so queued requests always plan against the current tree
        private readonly List<State> m_ExitList = new List<State>(); // child first
        private readonly List<State> m_EnterList = new List<State>(); // parent first

        private int m_LockDepth = 0; // while > 0 we're walking the tree, requests are queued and processed once the walk completes
        private bool m_Pumping = false;

        public bool IsTransitioning => m_HasActive;
        public bool HasPendingRequests => m_Queue.Count > 0;

        public enum SequencingModes
        {
            Parallel,
            Sequential,
        }

        private enum RequestTypes
        {
            Transition,
            PushOverlay,
            PopOverlay,
        }

        private struct Request
        {
            public RequestTypes Type;
            public State From;
            public State To;
            public System.Action<State, State, bool> Callback;

            public void Complete(bool success) => Callback?.Invoke(From, To, success);
        }

        /// <summary>Executor for the sub-stages of transitions, their "phases"</summary>
        private class PhaseExecutor 
        {
            List<PhaseStep> m_Steps = new List<PhaseStep>();
            ISequence m_Sequence;
            CancellationTokenSource m_SequencerCancellationTokenSrc;
            System.Action m_Callback;

            public bool IsRunning => m_Sequence != null;

            public void Start(IEnumerable<PhaseStep> steps, SequencingModes sequencingMode, System.Action callback)
            {
                d.Assert(!IsRunning, "Trying to start a sequenced execution of phase steps for a state transition while one is already running! Fix me!");

                m_Steps.Clear();
                m_Steps.AddRange(steps);
                if (m_Steps.Count == 0)
                {
                    callback?.Invoke(); // nothing to wait on, don't burn a frame
                    return;
                }

                m_Callback = callback;
                m_SequencerCancellationTokenSrc = new CancellationTokenSource();
                m_Sequence = BuildSequencer(sequencingMode, m_Steps, m_SequencerCancellationTokenSrc);
                m_Sequence.Start();
            }

            public void Tick()
            {
                if (!IsRunning)
                    return;
                if (!m_Sequence.Tick())
                    return; // just a normal update
                End(); // we done
            }

            private void End()
            {
                System.Action callback = m_Callback;
                Reset();
                callback?.Invoke();
            }

            /// <summary>Cancels any running steps, the completion callback is NOT invoked</summary>
            public void Cancel()
            {
                if (!IsRunning)
                    return;
                m_SequencerCancellationTokenSrc?.Cancel();
                Reset();
            }

            private void Reset()
            {
                m_Steps.Clear();
                m_Sequence = null;
                m_SequencerCancellationTokenSrc = null; // not disposed, cancelled tasks may still observe the token
                m_Callback = null;
            }

            private static ISequence BuildSequencer(SequencingModes mode, List<PhaseStep> phaseSteps, CancellationTokenSource ct)
            {
                return mode switch
                {
                    SequencingModes.Parallel => new ParallelSequence(phaseSteps, ct.Token),
                    SequencingModes.Sequential => new SequentialSequence(phaseSteps, ct.Token),
                    _ => throw new System.NotImplementedException()
                };
            }
        }

        private static IEnumerable<PhaseStep> GatherPhaseSteps(IEnumerable<State> stateChain, bool deactivate)
        {
            foreach (State state in stateChain)
            {
                foreach (PhaseStep step in state.PrepareActivitySteps(deactivate))
                    yield return step;
            }
        }

        public TransitionSequencer(StateMachine machine, SequencingModes sequencingMode)
        {
            Machine = machine;
            m_SequencingMode = sequencingMode;
        }

        public void RequestTransition(State from, State to, System.Action<State, State, bool> transitionCompleteCallback = null)
        {
            Enqueue(new Request { Type = RequestTypes.Transition, From = from, To = to, Callback = transitionCompleteCallback });
        }

        /// <summary>Request the given overlay (and its initial sub-states) be pushed onto its host, which must be active</summary>
        public void RequestPushOverlay(State overlay, System.Action<State, State, bool> completeCallback = null)
        {
            Enqueue(new Request { Type = RequestTypes.PushOverlay, From = overlay?.Parent, To = overlay, Callback = completeCallback });
        }

        /// <summary>Request the given overlay (and all of its active descendants) be popped off its host</summary>
        public void RequestPopOverlay(State overlay, System.Action<State, State, bool> completeCallback = null)
        {
            Enqueue(new Request { Type = RequestTypes.PopOverlay, From = overlay, To = overlay?.Parent, Callback = completeCallback });
        }

        private void Enqueue(Request request)
        {
            m_Queue.Enqueue(request);
            Pump();
        }

        /// <summary>Defers request processing until the matching <see cref="Unlock"/>, used while walking the state tree so it isn't mutated mid-walk</summary>
        internal void Lock() => m_LockDepth++;
        internal void Unlock()
        {
            d.Assert(m_LockDepth > 0, "Unbalanced TransitionSequencer Unlock! Fix me!");
            if (m_LockDepth > 0 && --m_LockDepth == 0)
                Pump();
        }

        private void Pump()
        {
            if (m_Pumping || m_LockDepth > 0 || m_HasActive)
                return;

            m_Pumping = true;
            try
            {
                while (!m_HasActive && m_Queue.Count > 0)
                    Begin(m_Queue.Dequeue());
            }
            finally
            {
                m_Pumping = false;
            }
        }

        private void Begin(Request request)
        {
            m_ExitList.Clear();
            m_EnterList.Clear();
            if (!TryPlan(request, m_ExitList, m_EnterList))
            {
                m_ExitList.Clear();
                m_EnterList.Clear();
                request.Complete(false);
                return;
            }

            m_Active = request;
            m_HasActive = true;

            // mark deactivation started for the old states, from bottom to top
            foreach (State state in m_ExitList)
                state.BeginDeactivation();

            m_PhaseExecutor.Start(GatherPhaseSteps(m_ExitList, deactivate: true), m_SequencingMode, EndExitPhase);
        }

        private void EndExitPhase()
        {
            // child first, so no state is exited while it still has an active child or overlay
            foreach (State state in m_ExitList)
            {
                state.CompleteDeactivation();
                state.Exit();
            }

            // parent first
            foreach (State state in m_EnterList)
                state.Enter();
            foreach (State state in m_EnterList)
                state.BeginActivation();

            m_PhaseExecutor.Start(GatherPhaseSteps(m_EnterList, deactivate: false), m_SequencingMode, EndEnterPhase);
        }

        private void EndEnterPhase()
        {
            // mark activation for the new states as complete, from top to bottom
            foreach (State state in m_EnterList)
                state.CompleteActivation();

            Request finished = m_Active;
            m_Active = default;
            m_HasActive = false;
            m_ExitList.Clear();
            m_EnterList.Clear();

            finished.Complete(true);
            Pump();
        }

        /// <summary>Resolves a request against the current tree into the states to exit (child first) and enter (parent first)</summary>
        /// <returns>False if the request is invalid or stale</returns>
        private bool TryPlan(Request request, List<State> exitList, List<State> enterList)
        {
            State root = Machine.RootState;
            switch (request.Type)
            {
                case RequestTypes.Transition:
                {
                    if (request.From != null && !request.From.IsLive)
                        return false; // requester was exited by an earlier request, drop it

                    if (request.To == null) // full exit of the state machine
                    {
                        if (!root.IsActive)
                            return false;
                        root.CollectActiveSubtree(exitList);
                        return true;
                    }

                    if (!BelongsToMachine(request.To))
                    {
                        d.LogWarning($"Trying to transition to '{request.To}' which isn't part of this state machine ('{root}')! Fix me!");
                        return false;
                    }

                    State target = request.To.GetLowestInitialSubState(); // transition into the deepest initial substate
                    List<State> path = new List<State>(target.GetAncestors());
                    path.Reverse(); // root -> target

                    int firstInactive = 0;
                    while (firstInactive < path.Count && path[firstInactive].IsLive)
                        firstInactive++;

                    for (int i = firstInactive; i < path.Count; i++)
                    {
                        if (path[i].IsOverlay)
                        {
                            d.LogWarning($"Trying to transition into '{target}' through inactive overlay '{path[i]}'! Overlays must be pushed, not transitioned to. Fix me!");
                            return false;
                        }
                    }

                    if (firstInactive == path.Count)
                    {
                        target.ActiveChild?.CollectActiveSubtree(exitList); // already there, just collapse anything below the target
                        return true;
                    }

                    if (firstInactive > 0)
                        path[firstInactive - 1].ActiveChild?.CollectActiveSubtree(exitList); // replace the branch that diverges from the target path (incl. its overlays)

                    for (int i = firstInactive; i < path.Count; i++)
                        enterList.Add(path[i]);
                    return true;
                }

                case RequestTypes.PushOverlay:
                {
                    State overlay = request.To;
                    if (overlay == null || !overlay.IsOverlay)
                    {
                        d.LogWarning($"Trying to push '{overlay}' as an overlay, but it isn't declared as an [OverlayState] field on its parent! Fix me!");
                        return false;
                    }
                    if (!BelongsToMachine(overlay))
                    {
                        d.LogWarning($"Trying to push overlay '{overlay}' which isn't part of this state machine ('{root}')! Fix me!");
                        return false;
                    }
                    if (overlay.IsLive)
                        return true; // already pushed
                    if (!overlay.Parent.IsLive)
                    {
                        d.LogWarning($"Trying to push overlay '{overlay}' but its host '{overlay.Parent}' isn't active! Fix me!");
                        return false;
                    }

                    State target = overlay.GetLowestInitialSubState();
                    enterList.AddRange(overlay.WalkDownTo(target, inclusive: true));
                    return true;
                }

                case RequestTypes.PopOverlay:
                {
                    State overlay = request.From;
                    if (overlay == null || !overlay.IsOverlay)
                    {
                        d.LogWarning($"Trying to pop '{overlay}' as an overlay, but it isn't declared as an [OverlayState] field on its parent! Fix me!");
                        return false;
                    }
                    if (!overlay.IsLive)
                        return false; // already popped, or exited along with its host

                    overlay.CollectActiveSubtree(exitList);
                    return true;
                }
            }
            return false;
        }

        private bool BelongsToMachine(State state)
        {
            State top = state;
            while (top.Parent != null)
                top = top.Parent;
            return ReferenceEquals(top, Machine.RootState);
        }

        /// <summary>
        /// Immediately applies a transition, skipping all activities. Only valid when no transition is running.
        /// </summary>
        internal void ApplyImmediate(State from, State to)
        {
            if (m_HasActive)
            {
                d.LogWarning($"Trying to immediately change state to '{to}' while a transition is running! Ignoring. Fix me!");
                return;
            }

            List<State> exitList = new List<State>();
            List<State> enterList = new List<State>();
            if (!TryPlan(new Request { Type = RequestTypes.Transition, From = from, To = to }, exitList, enterList))
                return;

            Lock();
            try
            {
                foreach (State state in exitList)
                {
                    state.Exit();
                    state.ResetActivities();
                }
                foreach (State state in enterList)
                    state.Enter();
                foreach (State state in enterList)
                    state.BeginActivation();
                foreach (State state in enterList)
                    state.CompleteActivation();
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>Cancels the running transition and all queued requests (failing their callbacks), and immediately exits the whole active tree</summary>
        internal void CancelAll()
        {
            List<Request> failed = new List<Request>();

            Lock();
            try
            {
                m_PhaseExecutor.Cancel();
                if (m_HasActive)
                    failed.Add(m_Active);
                m_Active = default;
                m_HasActive = false;
                m_ExitList.Clear();
                m_EnterList.Clear();

                if (Machine.RootState.IsActive)
                {
                    List<State> exitList = new List<State>();
                    Machine.RootState.CollectActiveSubtree(exitList);
                    foreach (State state in exitList)
                    {
                        state.Exit();
                        state.ResetActivities();
                    }
                }

                failed.AddRange(m_Queue); // includes anything requested by exit callbacks above
                m_Queue.Clear();
            }
            finally
            {
                m_LockDepth--; // release without pumping, the queue is intentionally empty
            }

            foreach (Request request in failed)
                request.Complete(false);
        }

        public void Update(float deltaTime)
        {
            if (m_HasActive)
            {
                m_PhaseExecutor.Tick();
                return;
            }

            Pump();
            if (m_HasActive)
                return;

            Lock();
            try { Machine.Update_Internal(deltaTime); }
            finally { Unlock(); }
        }

        public override string ToString()
        {
            if (!m_HasActive)
                return $"Transition: None ({m_Queue.Count} pending)";

            string phase = m_PhaseExecutor.IsRunning ? "Running" : "Starting";
            return $"Transition ({m_Active.Type}) ({m_Queue.Count} pending) [{phase}]:\n" +
                $"    {m_ExitList.FirstOrDefault()?.ToStringWithAncestors(includeActivationStatus: false) ?? "null"} ...to...\n" +
                $"    {m_EnterList.LastOrDefault()?.ToStringWithAncestors(includeActivationStatus: false) ?? "null"}";
        }
    }
}
