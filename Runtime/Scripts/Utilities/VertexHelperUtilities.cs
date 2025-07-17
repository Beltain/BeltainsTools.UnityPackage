using UnityEngine;
using UnityEngine.UI;

namespace BeltainsTools.Utilities
{
    public static class VertexHelperUtilities
    {
        public static void DrawTri(VertexHelper vh, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            int vertIndex = vh.currentVertCount;

            vh.AddVert(p1, color, Vector2.zero);
            vh.AddVert(p2, color, Vector2.zero);
            vh.AddVert(p3, color, Vector2.zero);

            vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
        }

        public static void DrawQuad(VertexHelper vh, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color color)
        {
            int vertIndex = vh.currentVertCount;

            vh.AddVert(p1, color, Vector2.zero);
            vh.AddVert(p2, color, Vector2.zero);
            vh.AddVert(p3, color, Vector2.zero);
            vh.AddVert(p4, color, Vector2.zero);

            vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
            vh.AddTriangle(vertIndex + 2, vertIndex + 3, vertIndex);
        }
    }
}
