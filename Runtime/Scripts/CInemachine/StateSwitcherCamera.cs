using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace BeltainsTools.Cinemachine
{
    /// <summary>
    /// Essentially cinemachine's <see cref="CinemachineStateDrivenCamera"/>, but without the need for an animator, and with the ability to switch states directly via code.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [ExcludeFromPreset]
    [AddComponentMenu("Blink/Cinemachine/StateSwitcherCamera")]
    public class StateSwitcherCamera : CinemachineCameraManagerBase
    {
        [System.Serializable]
        public struct Instruction
        {
            [Tooltip("The unique identifier of the animation state")]
            public string StateUID;
            /// <summary>The virtual camera to activate when the animation state becomes active</summary>
            [Tooltip("The virtual camera to activate when the animation state becomes active")]
            [ChildCameraProperty]
            public CinemachineVirtualCameraBase Camera;
            /// <summary>How long to wait (in seconds) before activating the camera.
            /// This filters out very short state durations</summary>
            [Tooltip("How long to wait (in seconds) before activating the camera. "
                + "This filters out very short state durations")]
            public float ActivateAfter;
            /// <summary>The minimum length of time (in seconds) to keep a camera active</summary>
            [Tooltip("The minimum length of time (in seconds) to keep a camera active")]
            public float MinDuration;

            private int m_StateHash;

            public int StateHash => m_StateHash;

            public void InitialiseHash()
            {
                m_StateHash = Animator.StringToHash(StateUID);
            }
        };

        /// <summary>The set of available camera "states" we can switch between</summary>
        [Tooltip("All available states we can switch between, the top state is treated as the default")]
        public Instruction[] Instructions;

        /// <summary>Internal API for the Inspector editor.  This implements nested states.</summary>
        [System.Serializable]
        internal struct ParentHash
        {
            /// <summary>Internal API for the Inspector editor</summary>
            public int Hash;
            /// <summary>Internal API for the Inspector editor</summary>
            public int HashOfParent;
        }

        /// <summary>Internal API for the Inspector editor</summary>
        [HideInInspector, SerializeField, NoSaveDuringPlay] private List<ParentHash> HashOfParent = new();

        private int m_RequestedStateHash = int.MinValue;

        private float m_ActivationTime = 0;
        private int m_ActiveInstructionIndex;
        private float m_PendingActivationTime = 0;
        private int m_PendingInstructionIndex;
        private Dictionary<int, List<int>> m_InstructionDictionary;
        private Dictionary<int, int> m_StateParentLookup;

        public void SetState(string stateUID) => SetState(Animator.StringToHash(stateUID));
        public void SetState(int stateHash)
        {
            m_RequestedStateHash = stateHash;
        }

        private int GetRequestedStateHash()
        {
            if (m_RequestedStateHash == int.MinValue)
            {
                if (Instructions?.Length != 0)
                    m_RequestedStateHash = Instructions[0].StateHash;
                else
                    return int.MinValue;
            }

            return m_RequestedStateHash;
        }

        /// <summary>Internal API for the Inspector editor</summary>
        internal void SetParentHash(List<ParentHash> list)
        {
            HashOfParent.Clear();
            HashOfParent.AddRange(list);
        }

        /// <summary>Internal API for the inspector editor.</summary>
        internal void ValidateInstructions()
        {
            Instructions ??= System.Array.Empty<Instruction>();
            for (int i = 0; i < Instructions.Length; i++)
            {
                ref Instruction instruction = ref Instructions[i];
                instruction.InitialiseHash();
            }

            m_InstructionDictionary = new Dictionary<int, List<int>>();
            for (int i = 0; i < Instructions.Length; ++i)
            {
                if (!m_InstructionDictionary.TryGetValue(Instructions[i].StateHash, out List<int> list))
                {
                    list = new List<int>();
                    m_InstructionDictionary[Instructions[i].StateHash] = list;
                }
                list.Add(i);
            }

            // Create the parent lookup
            m_StateParentLookup = new Dictionary<int, int>();
            for (int i = 0; HashOfParent != null && i < HashOfParent.Count; ++i)
                m_StateParentLookup[HashOfParent[i].Hash] = HashOfParent[i].HashOfParent;

            // Zap the cached current instructions
            m_ActivationTime = m_PendingActivationTime = 0;
            ResetLiveChild();
        }

        /// <inheritdoc />
        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
        {
            if (!PreviousStateIsValid)
                ValidateInstructions();

            List<CinemachineVirtualCameraBase> children = ChildCameras;
            if (children == null || children.Count == 0)
            {
                m_ActivationTime = 0;
                return null;
            }

            CinemachineVirtualCameraBase fallbackCam = children[0];


            // quick sanity check, in case number of instructions changed
            if (m_ActiveInstructionIndex < 0 || m_ActiveInstructionIndex >= Instructions.Length)
            {
                m_ActiveInstructionIndex = 0;
                m_ActivationTime = 0;
            }
            if (!PreviousStateIsValid || m_PendingInstructionIndex < 0 || m_PendingInstructionIndex >= Instructions.Length)
            {
                m_PendingInstructionIndex = 0;
                m_PendingActivationTime = 0;
            }

            // Get the current animation state
            int hash = GetRequestedStateHash();

            // If we don't have an instruction for this state, find a suitable default state
            while (hash != int.MinValue && !m_InstructionDictionary.ContainsKey(hash))
                hash = m_StateParentLookup.ContainsKey(hash) ? m_StateParentLookup[hash] : int.MinValue;

            // Get the highest-priority active camera from the instruction list
            int newInstrIndex = -1;
            if (m_InstructionDictionary.ContainsKey(hash))
            {
                List<int> instrList = m_InstructionDictionary[hash];

                // Find the instruction whose camera is active and has the highest priority
                int bestPriority = int.MinValue;
                for (int i = 0; i < instrList.Count; ++i)
                {
                    int index = instrList[i];
                    CinemachineVirtualCameraBase cam = index < Instructions.Length ? Instructions[index].Camera : null;
                    if (cam != null && cam.isActiveAndEnabled && cam.Priority.Value > bestPriority)
                    {
                        newInstrIndex = index;
                        bestPriority = cam.Priority.Value;
                    }
                }
            }

            // Process it.  If no new camera is desired, we just ignore this state
            float now = CinemachineCore.CurrentTime;
            if (newInstrIndex >= 0)
            {
                // If it's neither active nor pending, we must take action
                if (m_ActivationTime == 0)
                {
                    // No current camera, actibvate immediately
                    m_ActiveInstructionIndex = newInstrIndex;
                    m_ActivationTime = now;
                    m_PendingActivationTime = 0;
                }
                else if (m_ActiveInstructionIndex != newInstrIndex
                    && (m_PendingActivationTime == 0 || m_PendingInstructionIndex != newInstrIndex))
                {
                    // Make it pending
                    m_PendingInstructionIndex = newInstrIndex;
                    m_PendingActivationTime = now;
                }
            }

            // Process the pending instruction
            if (m_PendingActivationTime != 0)
            {
                // Has it been pending long enough, and are we allowed to switch away
                // from the active action?
                if ((now - m_PendingActivationTime) > Instructions[m_PendingInstructionIndex].ActivateAfter
                    && (now - m_ActivationTime) > Instructions[m_ActiveInstructionIndex].MinDuration)
                {
                    // Yes, activate it now
                    m_ActiveInstructionIndex = m_PendingInstructionIndex;
                    m_ActivationTime = now;
                    m_PendingActivationTime = 0;
                }
            }

            if (m_ActivationTime != 0)
                return Instructions[m_ActiveInstructionIndex].Camera;
            return fallbackCam;
        }

        /// <summary>
        /// Call this to cancel the current wait time for the pending instruction and activate
        /// the pending instruction immediately.
        /// </summary>
        public void CancelWait()
        {
            if (m_PendingActivationTime != 0 && m_PendingInstructionIndex >= 0 && m_PendingInstructionIndex < Instructions.Length)
            {
                m_ActiveInstructionIndex = m_PendingInstructionIndex;
                m_ActivationTime = CinemachineCore.CurrentTime;
                m_PendingActivationTime = 0;
            }
        }

        /// <inheritdoc />
        protected override void Reset()
        {
            base.Reset();
            Instructions = null;
            DefaultBlend = new(CinemachineBlendDefinition.Styles.EaseInOut, 0.5f);
            CustomBlends = null;
        }
    }
}
