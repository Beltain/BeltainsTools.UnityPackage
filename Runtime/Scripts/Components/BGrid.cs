using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using BeltainsTools.Editor;
#endif

namespace BeltainsTools
{
    public class BGrid : MonoBehaviour
    {
        [SerializeField] protected float m_DefaultCellSize = 1f;
        [SerializeField] protected Vector2Int m_DefaultDimensions = new Vector2Int(2, 2);

        public float CellSize { get; private set; } = 1f;
        public Vector2Int Dimensions { get; private set; } = Vector2Int.zero;
        public Cell[,] Cells { get; private set; } = null;
        public List<Vector2Int> CellCoordinates { get; private set; } = new List<Vector2Int>();
        public Vector3 OriginCellLocalPosition { get; private set; } = Vector3.zero;



        public struct Cell
        {
            /// <summary>Does this cell exist? If false then consider it invalid on the grid and count this as a placeholder.</summary>
            public bool Exists;
            /// <summary>Is this cell active? Active state can be changed throughout the lifetime of a cell for various reasons.</summary>
            public bool IsActive;

            /// <summary>Is this a real cell that exists on the grid and is currently active</summary>
            public bool ExistsAndActive => Exists && IsActive;

            public Vector2Int GridCoordinates;
            public Vector3 LocalPosition;

            public static readonly Cell none = new Cell() { Exists = false, IsActive = true };
            public static readonly Cell blank = new Cell() { Exists = true, IsActive = true };
        }

        /// <summary>Copy config from other grid and build</summary>
        public void Build(BGrid otherGrid)
            => Build(otherGrid.Dimensions, otherGrid.CellSize);
        public void Build(Vector2Int dimensions, float cellSize)
        {
            Dimensions = dimensions;
            CellSize = cellSize;

            Cells = new Cell[Dimensions.x, Dimensions.y];
            CellCoordinates.Clear();

            Vector3 dimensions3D = new Vector3(Dimensions.x * CellSize, 0f, Dimensions.y * CellSize);
            Vector3 cellDimensions3D = new Vector3(CellSize, 0f, CellSize);
            OriginCellLocalPosition = -(dimensions3D / 2f) + (cellDimensions3D / 2f);

            for (int x = 0; x < Dimensions.x; x++)
            {
                for (int y = 0; y < Dimensions.y; y++)
                {
                    Vector2Int coords = new Vector2Int(x, y);
                    CellCoordinates.Add(coords);
                    Cells[x, y] = new Cell()
                    {
                        IsActive = true,
                        Exists = true,
                        GridCoordinates = coords,
                        LocalPosition = OriginCellLocalPosition + new Vector3(x, 0f, y) * cellSize
                    };
                }
            }
        }


        public Vector2Int WorldPositionToGridCoordinate(Vector3 worldPos) => LocalPositionToGridCoordinate(transform.InverseTransformPoint(worldPos));
        public Vector2Int LocalPositionToGridCoordinate(Vector3 localPos)
        {
            Vector3 offsetFromOrigin = localPos - OriginCellLocalPosition;
            offsetFromOrigin /= CellSize;
            return new Vector2Int(Mathf.RoundToInt(offsetFromOrigin.x), Mathf.RoundToInt(offsetFromOrigin.z));
        }


        public Vector3 GridCoordinateToWorldPosition(Vector2Int coordinate) => GridSpacePositionToWorldPosition((Vector2)coordinate);
        public Vector3 GridSpacePositionToWorldPosition(Vector2 boardPos)
        {
            return transform.TransformPoint(OriginCellLocalPosition + (boardPos.ToVector3XZ() * CellSize));
        }


        public void Awake()
        {
            Build(m_DefaultDimensions, m_DefaultCellSize);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawGizmosSelected()
        {
            if (Cells == null)
                return;

            foreach (Cell cell in Cells)
            {
                Gizmos.color =
                    cell.Exists ?
                        cell.IsActive ?
                            Color.green : 
                            Color.yellow :
                        Color.red;
                Gizmos.DrawCube(transform.TransformPoint(cell.LocalPosition), Vector3.one * CellSize * 0.5f);
            }
        }
    }
}

