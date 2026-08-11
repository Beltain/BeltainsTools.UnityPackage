using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

namespace BeltainsTools.Board
{
    [ExecuteAlways]
    [RequireComponent(typeof(Board))]
    [AddComponentMenu(BTInternal.PackageData.Paths.CreateAssetMenu.k_Cinemachine + "Targetting/BoardCinemachineTarget")]
    public class BoardCinemachineTarget : MonoBehaviour
    {
        [SerializeField]
        private Vector3 m_CellCameraFocusOffset = Vector3.up * 0.5f;

        [SerializeField, HideInInspector]
        private Board m_Board;
        [SerializeField, HideInInspector]
        private CinemachineTargetGroup m_CinemachineTargetGroup;

        private Transform m_TargetTransformsContainer;
        private List<Transform> m_TargetTransforms = new List<Transform>();

        private Vector3 m_PreviousCellCameraFocusOffset = Vector3.negativeInfinity;
        private Vector3 m_TargetsAverageLocalPos = Vector3.zero;


        private void EnsureInitialised()
        {
            if (m_Board == null)
                m_Board = GetComponent<Board>();

            if (m_TargetTransformsContainer == null)
            {
                m_TargetTransformsContainer = new GameObject("_CinemachineTargeter_Targets").transform;
                m_TargetTransformsContainer.gameObject.hideFlags = HideFlags.DontSave;
                m_TargetTransformsContainer.SetParent(transform);
                m_TargetTransformsContainer.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            if (m_CinemachineTargetGroup == null)
            {
                m_CinemachineTargetGroup = new GameObject("_CinemachineTargetGroup").AddComponent<CinemachineTargetGroup>();
                m_CinemachineTargetGroup.transform.SetParent(transform);
                m_CinemachineTargetGroup.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        private void RebuildTargets()
        {
            m_CinemachineTargetGroup.Targets.Clear();

            int numCells = m_Board.ActiveCells.Count;

            ResizeTargetTransforms(numCells);

            float boardCellSize = Mathf.Max(m_Board.Grid.cellSize.x, m_Board.Grid.cellSize.z) * 0.5f;
            for (int i = 0; i < numCells; i++)
            {
                Cell cell = m_Board.ActiveCells.ElementAt(i);
                m_TargetTransforms[i].name = cell.Index.ToString();
                m_CinemachineTargetGroup.AddMember(m_TargetTransforms[i], 1f, boardCellSize);
            }

            RepositionTargets();
        }

        private void ResizeTargetTransforms(int numRequired)
        {
            for (int i = m_TargetTransforms.Count - 1; i > numRequired - 1; i--) // cull unnecessary target transforms
            {
                DestroyImmediate(m_TargetTransforms[i].gameObject);
                m_TargetTransforms.RemoveAt(i);
            }

            for (int i = m_TargetTransforms.Count; i < numRequired; i++) // add missing necessary target transforms
            {
                Transform targetTransform = new GameObject("_TargetTransform").transform;
                targetTransform.SetParent(m_TargetTransformsContainer);
                targetTransform.gameObject.hideFlags = HideFlags.DontSave;
                m_TargetTransforms.Add(targetTransform);
            }
        }

        private void RepositionTargets()
        {
            m_TargetsAverageLocalPos = Vector3.zero;
            for (int i = 0; i < m_Board.ActiveCells.Count; i++)
            {
                Cell cell = m_Board.ActiveCells.ElementAt(i);
                m_TargetTransforms[i].position = cell.SurfacePoint + cell.transform.TransformDirection(m_CellCameraFocusOffset);
                m_TargetsAverageLocalPos += m_TargetTransforms[i].localPosition;
            }
            m_TargetsAverageLocalPos /= m_Board.ActiveCells.Count;
        }

        private void OnBoardCellsChangedEvent(IReadOnlyCollection<Cell> activeCells)
        {
            RebuildTargets();
        }

        private void OnBoardCellPositioningChangedEvent()
        {
            RebuildTargets();
        }


        private void OnEnable()
        {
            EnsureInitialised();
            RebuildTargets();
            m_Board.CellsChangedEvent.Subscribe(OnBoardCellsChangedEvent);
            m_Board.CellPositioningChangedEvent.Subscribe(OnBoardCellPositioningChangedEvent);
        }

        private void Update()
        {
            if (m_PreviousCellCameraFocusOffset != m_CellCameraFocusOffset)
            { 
                m_PreviousCellCameraFocusOffset = m_CellCameraFocusOffset;
                RepositionTargets();
            }
        }

        private void OnDisable()
        {
            m_Board.CellsChangedEvent.Unsubscribe(OnBoardCellsChangedEvent);
            m_Board.CellPositioningChangedEvent.Unsubscribe(OnBoardCellPositioningChangedEvent);

            m_CinemachineTargetGroup.Targets.Clear();
            ResizeTargetTransforms(0);
        }

        private void OnDrawGizmosSelected()
        {
            if (m_Board == null || m_TargetTransformsContainer == null)
                return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(m_TargetTransformsContainer.TransformPoint(m_TargetsAverageLocalPos), 0.1f);
        }
    }
}
