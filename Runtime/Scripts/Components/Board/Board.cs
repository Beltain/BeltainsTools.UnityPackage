using BeltainsTools;
using BeltainsTools.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Board
{
    /// <summary>
    /// A grid with discrete occupiable spaces (<see cref="Cell"/>s) that can be used to house <see cref="Piece"/>s.<br/>
    /// Can be configured with a <see cref="Config"/> object to determine the size and layout of the board.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Grid))]
    [AddComponentMenu(Board.k_CreateAssetMenuPath + "Board")]
    public partial class Board : Singleton<Board>
    {
        [SerializeField]
        private Config m_Config = new Config();

        [SerializeField, HideInInspector]
        private Grid m_Grid;

        private Transform m_CellContainer;

        private CellPositioningData m_BuiltCellPositioning;
        private Config m_BuiltConfig = new Config();

        public const string k_CreateAssetMenuPath = BTInternal.PackageData.Paths.CreateAssetMenu.k_Base + "Board/";

        private HashSet<Cell> m_ActiveCells = new HashSet<Cell>();
        /// <summary>Metadata object for quick lookup of <see cref="m_ActiveCells"/></summary>
        private Cell[,] m_ActiveCellsMap = null;

        private bool CellMapValid => m_ActiveCellsMap != null;

        public IReadOnlyCollection<Cell> ActiveCells => m_ActiveCells;

        public Grid Grid
        {
            get
            {
                InitialiseReferences();
                return m_Grid;
            }
        }

        public Transform CellContainer
        {
            get
            {
                InitialiseReferences();
                return m_CellContainer;
            }
        }

        [System.NonSerialized] public BEvent CellPositioningChangedEvent;
        [System.NonSerialized] public BEvent<IReadOnlyCollection<Cell>> CellsChangedEvent;


        private struct CellPositioningData : System.IEquatable<CellPositioningData>
        {
            public Vector3 CellSize;
            public Vector3 CellGap;

            public CellPositioningData(Grid grid)
            { 
                CellSize = grid.cellSize;
                CellGap = grid.cellGap;
            }

            bool IEquatable<CellPositioningData>.Equals(CellPositioningData other)
            {
                return CellSize == other.CellSize && CellGap == other.CellGap;
            }
        }


        public Vector3 GetCellCenterWorld(Vector2Int cellIndex)
            => Grid.GetCellCenterWorld(cellIndex.ToVector3IntXZ());

        public Vector3 GetCellCenterLocal(Vector2Int cellIndex)
            => Grid.GetCellCenterLocal(cellIndex.ToVector3IntXZ());

        /// <inheritdoc cref="GetEdgeCellInDirection(Cell, Vector2Int)"/>
        public Cell GetEdgeCellInDirection(Vector2Int originIndex, Vector2Int direction)
            => GetEdgeCellInDirection(GetCellAtIndex(originIndex), direction);
        /// <returns>The last cell in the direction provided from the start cell. Use this to find edge cells</returns>
        public Cell GetEdgeCellInDirection(Cell startCell, Vector2Int direction)
        {
            // normalise direction, lock to cardinal directions
            Vector2Int cardinalDirection = new Vector2Int(
                direction.x != 0 ? System.Math.Sign(direction.x) : 0,
                direction.y != 0 ? System.Math.Sign(direction.y) : 0
            );

            if (cardinalDirection == Vector2Int.zero)
                return startCell;

            Cell currentCell = startCell;
            while (TryGetCell(currentCell.Index + cardinalDirection, out Cell nextCell))
                currentCell = nextCell;
            return currentCell;
        }

        public bool TryGetCell(int x, int y, out Cell cell) => TryGetCell(new Vector2Int(x, y), out cell);
        public bool TryGetCell(Vector2Int index, out Cell cell)
        {
            cell = GetCellAtIndex(index);
            return cell != null;
        }
        public Cell GetCellAtIndex(int x, int y) => GetCellAtIndex(new Vector2Int(x, y));
        public Cell GetCellAtIndex(Vector2Int index)
        {
            if (!HasCell(index.x, index.y))
                return null;
            return CellMapValid ? m_ActiveCellsMap[index.x, index.y] : m_ActiveCells.First(c => c.Index == index);
        }

        private bool HasCell(int x, int y) => HasCell(new Vector2Int(x, y));
        private bool HasCell(Vector2Int index)
        {
            if (CellMapValid)
            {
                if ((index.x < 0 || index.y < 0 ||
                    m_ActiveCellsMap.GetLength(0) <= index.x ||
                    m_ActiveCellsMap.GetLength(1) <= index.y))
                    return false;
                return m_ActiveCellsMap[index.x, index.y] != null;
            }

            return m_ActiveCells.Any(c => c.Index == index);
        }


        public void BuildConfig(Config config)
        {
            m_BuiltConfig.CopyFrom(config);
            RegenerateCells();
        }

        private void RegenerateCells()
        {
            m_ActiveCellsMap = null; // invalidate last cell map
            bool[,] cellLayoutMap = m_BuiltConfig.GetCellLayout2D(); // get where cells should exist on this grid

            // clear unnecessary cells
            for (int i = m_ActiveCells.Count - 1; i >= 0; i--)
            {
                Cell cell = m_ActiveCells.ElementAt(i);
                if (!m_BuiltConfig.GetContainsCell(cell.Index))
                    RemoveCell(cell);
            }

            // generate missing cells
            for (int x = 0; x < cellLayoutMap.GetLength(0); x++)
            {
                for (int y = 0; y < cellLayoutMap.GetLength(1); y++)
                {
                    if (cellLayoutMap[x, y] && !m_ActiveCells.Any(r => r.Index == new Vector2Int(x, y)))
                        AddCellAtIndex(new Vector2Int(x, y));
                }
            }

            // refresh active cells map mothafuckaaaaaa     
            m_ActiveCellsMap = new Cell[cellLayoutMap.GetLength(0), cellLayoutMap.GetLength(1)];
            foreach (Cell cell in m_ActiveCells)
                m_ActiveCellsMap[cell.Index.x, cell.Index.y] = cell;

            CellsChangedEvent.Invoke(ActiveCells);
        }


        private void AddCellAtIndex(Vector2Int index)
        {
            d.Assert(!HasCell(index), "Trying to add a cell at an index that already has a cell! What the FRACK?????!?!?!?!?");

            Cell newCell = new GameObject($"Cell [{index.x}, {index.y}]").AddComponent<Cell>();
            newCell.gameObject.hideFlags = HideFlags.DontSave;
            newCell.transform.SetParent(CellContainer);
            newCell.DestroyedEvent.Subscribe(OnActiveCellDestroyed);
            newCell.InitialiseOnBoard(this, index);
            m_ActiveCells.Add(newCell);
        }

        private void RemoveAllCells()
        {
            for (int i = m_ActiveCells.Count - 1; i >= 0; i--)
                RemoveCell(m_ActiveCells.ElementAt(i));
        }

        private void RemoveCell(Cell cell) => RemoveCellAtIndex(cell.Index);
        private void RemoveCellAtIndex(Vector2Int index)
        {
            d.Assert(HasCell(index), "Trying to remove a cell at an index that doesn't have a cell! What the FRICK?????!?!?!?!?");
            GameObject objectToDestroy = GetCellAtIndex(index).gameObject;
            if (Application.isPlaying)
                Destroy(objectToDestroy);
            else
                DestroyImmediate(objectToDestroy);
        }

        private void OnActiveCellDestroyed(Cell cell)
        {
            cell.DestroyedEvent.Unsubscribe(OnActiveCellDestroyed);
            m_ActiveCells.Remove(cell);
        }


        private void InitialiseReferences() 
        {
            if (m_Grid == null)
                m_Grid = GetComponent<Grid>();

            if (m_CellContainer == null)
            {
                m_CellContainer = new GameObject("_Cells").transform;
                m_CellContainer.gameObject.hideFlags = HideFlags.DontSave;
                m_CellContainer.SetParent(transform);
                m_CellContainer.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        protected override void Awake()
        {
            if (Application.isPlaying)
                base.Awake();

            InitialiseReferences();
            BuildConfig(m_Config);
        }

        private void Update()
        {
            CellPositioningData cellPositioning = new CellPositioningData(Grid);
            if (!cellPositioning.Equals(m_BuiltCellPositioning))
            {
                m_BuiltCellPositioning = cellPositioning;
                CellPositioningChangedEvent.Invoke();
            } 

            if (!m_BuiltConfig.Equals(m_Config))
                BuildConfig(m_Config);
        }

        private void OnDisable()
        {
            RemoveAllCells();
        }

        protected override void OnDestroy()
        {
            if (Application.isPlaying)
                base.OnDestroy();
        }

        private void DrawBoardGizmos(float alpha = 1.0f)
        {
            bool[,] cellLayoutMap = m_Config.GetCellLayout2D();
            for (int x = 0; x < cellLayoutMap.GetLength(0); x++)
            {
                for (int y = 0; y < cellLayoutMap.GetLength(1); y++)
                {
                    if (cellLayoutMap[x, y])
                        Cell.DrawTileGizmo(alpha, Grid.GetCellCenterWorld(new Vector3Int(x, 0, y)), new Vector2(x, y), Grid.cellSize);
                }
            }
        }

        private void OnDrawGizmos()
        {
            DrawBoardGizmos(0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            DrawBoardGizmos();
        }
    }

}