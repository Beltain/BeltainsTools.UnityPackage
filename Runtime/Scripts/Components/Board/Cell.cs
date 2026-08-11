using BeltainsTools.EventHandling;
using UnityEngine;

namespace BeltainsTools.Board
{
    /// <summary>A discrete occupiable space on a <see cref="Board"/>. Can be used to house <see cref="BeltainsTools.Board.Piece"/>s, and handle per-<see cref="Cell"/> logic</summary>
    [AddComponentMenu(Board.k_CreateAssetMenuPath + "Cell")]
    [ExecuteAlways]
    public class Cell : MonoBehaviour
    {
        private Board m_Board;
        private Vector2Int m_Index;

        private Piece m_OccupyingPiece;

        private bool m_IsBeingDestroyed = false;

        public Board Board => m_Board;
        public Vector2Int Index => m_Index;
        public Piece OccupyingPiece => m_OccupyingPiece;
        public bool IsOccupied => OccupyingPiece != null;
        public Vector3 SurfacePoint => transform.position;

        [System.NonSerialized] public BEvent<Piece> OccupierChangedEvent;
        [System.NonSerialized] public BEvent<Cell> InitialisedOnBoardEvent;
        [System.NonSerialized] public BEvent<Cell> DeinitialisedFromBoardEvent;
        [System.NonSerialized] public BEvent<Cell> DestroyedEvent;


#if UNITY_EDITOR 
        [UnityEditor.InitializeOnLoad]
        public static class DisableGizmo
        {
            static DisableGizmo() => UnityEditor.GizmoUtility.SetIconEnabled(typeof(Cell), false);
        }
#endif

        public void InitialiseOnBoard(Board board, Vector2Int index)
        {
            DeinitialiseFromBoard(); // clear existing

            d.Assert(board != null, "Trying to initialise board cell on a null board, no bueno fucko...");

            m_Board = board;
            m_Index = index;

            transform.SetParent(m_Board.CellContainer);
            RepositionOnBoard();
            Board.CellPositioningChangedEvent.Subscribe(OnBoardCellPositioningChangedEvent);

            InitialisedOnBoardEvent.Invoke(this);
        }

        public void RepositionOnBoard()
        {
            d.Assert(m_Board != null, "Trying to reposition cell on board, but the board is null. Did you forget to initialise the cell on a board?");
            transform.SetPositionAndRotation(m_Board.GetCellCenterWorld(m_Index), transform.parent.rotation);
        }

        public void DeinitialiseFromBoard()
        {
            if (m_Board == null)
                return;
            
            if (transform.parent == m_Board.CellContainer && !m_IsBeingDestroyed)
                transform.SetParent(null);

            Board.CellPositioningChangedEvent.Unsubscribe(OnBoardCellPositioningChangedEvent);

            m_Board = null;
            m_Index = Vector2Int.zero;

            DeinitialisedFromBoardEvent.Invoke(this);
        }


        public void ClearOccupier() => SetOccupier(null);
        public void SetOccupier(Piece occupier)
        {
            d.AssertFormat(occupier == null || !IsOccupied, "Trying to set occupier {0} on a cell already occupied by {1}", occupier, OccupyingPiece);
            m_OccupyingPiece = occupier;
            OccupierChangedEvent.Invoke(occupier);
        }

        private void OnBoardCellPositioningChangedEvent()
        {
            RepositionOnBoard();
        }

        private void OnDestroy()
        {
            m_IsBeingDestroyed = true;
            DeinitialiseFromBoard();
            DestroyedEvent.Invoke(this);
        }

        public static void DrawTileGizmo(float alpha, Vector3 surfacePosition, Vector2 index, Vector3 size)
        {
            Color oddColor = new Color(0.96f, 0.90f, 0.76f, alpha);
            Color evenColor = new Color(0.84f, 0.65f, 0.45f, alpha);

            Gizmos.color = // checkerboard pattern
                (index.x + index.y) % 2 == 0 ? evenColor : oddColor;
            Gizmos.DrawCube(surfacePosition, size);
        }

        private void OnDrawGizmosSelected()
        {
            DrawTileGizmo(1f, SurfacePoint, m_Index, m_Board.Grid.cellSize);

            if (!Application.isPlaying)
                return;

            Gizmos.color = IsOccupied ? Color.firebrick : Color.limeGreen;
            Gizmos.DrawSphere(SurfacePoint, 0.15f);
        }
    }
}