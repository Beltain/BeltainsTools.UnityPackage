using BeltainsTools.BTInternal;
using BeltainsTools.EventHandling;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Board
{
    /// <summary>Component that handles occupation logic on a <see cref="Board"/> <see cref="Cell"/></summary>
    [AddComponentMenu(Board.k_CreateAssetMenuPath + "Piece")]
    public class Piece : MonoBehaviour
    {
        private Cell m_Cell;

        public Cell Cell => m_Cell;
        public bool IsOnBoard => Cell != null;

        [System.NonSerialized] private BEvent<Cell> CellChangedEvent;

        public void SetOnCell(Cell cell)
        {
            if (m_Cell == cell)
                return;

            if (m_Cell != null)
            {
                m_Cell.ClearOccupier();
                if (transform.parent == m_Cell.transform)
                    transform.SetParent(null, true);
                m_Cell.DeinitialisedFromBoardEvent.Unsubscribe(OnCellDeinitialisedFromBoard);
            }

            m_Cell = cell;

            if (m_Cell != null)
            {
                m_Cell.SetOccupier(this);
                transform.SetParent(m_Cell.transform, true);
                m_Cell.DeinitialisedFromBoardEvent.Subscribe(OnCellDeinitialisedFromBoard);
            }

            CellChangedEvent.Invoke(m_Cell);
        }

        private void OnCellDeinitialisedFromBoard(Cell cell)
        {
            SetOnCell(null);
            throw new System.NotImplementedException("Implement handling of cell deinitialisation for piece! Clean us up!");
        }

        private void OnRecycle()
        {
            SetOnCell(null);
        }

        private void OnDestroy()
        {
            SetOnCell(null);
        }
    }
}