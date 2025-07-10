using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class GridUtilities
    {
        public struct CellEdge
        {
            public Vector2Int CellPosition;
            public Vector2 StartVertex;
            public Vector2 EndVertex;
            public Vector2 Center;
            public Vector2 Normal;


            public CellEdge(Vector2Int cellPosition, Vector2 normal, Vector2 tangent, float tileSize)
            {
                float halfCellSize = tileSize / 2f;

                CellPosition = cellPosition;

                StartVertex = CellPosition + (normal * halfCellSize) - (tangent * halfCellSize);
                EndVertex = CellPosition + (normal * halfCellSize) + (tangent * halfCellSize);

                Center = (StartVertex + EndVertex) / 2f;

                Normal = normal.normalized;
                Normal = new Vector2(Mathf.RoundToInt(Normal.x), Mathf.RoundToInt(Normal.y));
            }

            public void AlignWithNormal(Vector2 normal)
            {
                Quaternion alignmentRot = Quaternion.AngleAxis(Quaternion.Angle(Quaternion.LookRotation(Normal), Quaternion.LookRotation(normal)), Vector3.forward); //Quaternion.FromToRotation(Normal, normal);
                Vector2 newStartVertex = alignmentRot * (StartVertex - Center);
                Vector2 newEndVertex = alignmentRot * (EndVertex - Center);
                StartVertex = Center + newStartVertex;
                EndVertex = Center + newEndVertex;
                Normal = normal;
            }

            public bool NormalsAligned(Vector2 normal)
            {
                return Normal == normal || Normal == -normal;
            }

            public bool ConnectsWith(CellEdge otherEdge, bool inline)
            {
                return
                    (!inline || NormalsAligned(otherEdge.Normal)) &&
                    (StartVertex == otherEdge.EndVertex || StartVertex == otherEdge.StartVertex || EndVertex == otherEdge.StartVertex || EndVertex == otherEdge.EndVertex);
            }

            public Vector2 GetSharedVertex(CellEdge otherEdge)
            {
                Vector2 sharedVertex =
                    otherEdge.StartVertex == EndVertex ?
                    EndVertex :
                    otherEdge.EndVertex == StartVertex ?
                        StartVertex :
                        Vector2.negativeInfinity;

                d.Assert(!float.IsNegativeInfinity(sharedVertex.x));

                return sharedVertex;
            }


            public bool IsEssentiallyEqual(CellEdge otherEdge)
            {
                if (otherEdge.Equals(this))
                    return true;
                return otherEdge.StartVertex == EndVertex && otherEdge.EndVertex == StartVertex && NormalsAligned(otherEdge.Normal);
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != typeof(CellEdge))
                    return false;
                CellEdge other = (CellEdge)obj;
                return other.StartVertex == StartVertex && other.EndVertex == EndVertex && other.Normal == Normal;
            }

            public override int GetHashCode()
                => base.GetHashCode();
        }

        public static IEnumerable<CellEdge> GetCellEdges(IEnumerable<Vector2Int> cellPositions, float tileSize = 1f)
        {
            HashSet<Vector2Int> cellLookup = cellPositions.ToHashSet();
            Quaternion stepRotation = Quaternion.Euler(0f, 0f, 90f);
            List<CellEdge> outsideEdges = new List<CellEdge>();

            //Get all outside edges from area cells
            foreach (Vector2Int cell in cellPositions)
            {
                Vector2 normal = Vector2.up;
                Vector2 tangent = Vector2.right;
                for (int i = 0; i < 4; i++)
                {
                    CellEdge edge = new CellEdge(cell, normal, tangent, tileSize);

                    if (!cellLookup.Contains(Vector2Int.RoundToInt(cell + normal)))
                    {
                        outsideEdges.Add(edge);
                    }

                    normal = stepRotation * normal;
                    tangent = stepRotation * tangent;
                }
            }

            return outsideEdges;
        }
    }
}
