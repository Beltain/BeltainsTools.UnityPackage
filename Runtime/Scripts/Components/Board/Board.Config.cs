using System;
using UnityEngine;

namespace BeltainsTools.Board
{
    public partial class Board
    {
        [System.Serializable]
        public class Config : System.IEquatable<Config>
        {
            public Vector2Int Size = new Vector2Int(4, 4);
            public bool[] Layout = new bool[16];

            private const bool k_DefaultActiveState = true;

            public void CopyFrom(Config other)
            {
                SetSize(other.Size);
                SetCellLayout2D(other.GetCellLayout2D());
            }

            public void SetSize(int width, int height) => SetSize(new Vector2Int(width, height));
            public void SetSize(Vector2Int size)
            {
                if (size == Size)
                    return;

                bool[,] previousLayout2D = GetCellLayout2D();
                Size = size;

                if (Layout == null)
                    Layout = new bool[Size.x * Size.y];
                else
                    System.Array.Resize(ref Layout, Size.x * Size.y);

                SetCellLayout2D(previousLayout2D);
            }

            public bool GetContainsCell(int x, int y) => GetContainsCell(new Vector2Int(x, y));
            public bool GetContainsCell(Vector2Int index)
            {
                bool inRange = index.x >= 0 && index.y >= 0 && index.x < Size.x && index.y < Size.y;
                if (!inRange)
                    return false;
                bool isActive = Layout[index.y * Size.x + index.x];
                return isActive;
            }

            /// <returns>The current layout as a 2D array</returns>
            public bool[,] GetCellLayout2D()
            {
                bool[,] cellLayout2D = new bool[Size.x, Size.y];
                if (Layout == null)
                    return cellLayout2D;

                for (int y = 0; y < Size.y; y++)
                {
                    for (int x = 0; x < Size.x; x++)
                    {
                        int index = y * Size.x + x;
                        if (index < Layout.Length)
                            cellLayout2D[x, y] = Layout[index];
                    }
                }
                return cellLayout2D;
            }

            /// <summary>
            /// Apply the provided layout2D onto our layout array. 
            /// If the provided layout2D is larger than our current layout, it will be truncated.
            /// </summary>
            public void SetCellLayout2D(bool[,] cellLayout2D)
            {
                Vector2Int otherSize = new Vector2Int(cellLayout2D.GetLength(0), cellLayout2D.GetLength(1));
                for (int y = 0; y < Size.y; y++)
                {
                    for (int x = 0; x < Size.x; x++)
                    {
                        int index = y * Size.x + x;
                        bool isActive = index < Layout.Length && x < otherSize.x && y < otherSize.y ? cellLayout2D[x, y] : k_DefaultActiveState;
                        Layout[index] = isActive;
                    }
                }
            }

            bool IEquatable<Config>.Equals(Config other)
            {
                if (Size != other.Size)
                    return false;

                for (int i = 0; i < Layout.Length; i++)
                {
                    if (Layout[i] != other.Layout[i])
                        return false;
                }

                return true;
            }
        }
    }
}