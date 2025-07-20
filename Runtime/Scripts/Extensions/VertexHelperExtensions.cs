using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class VertexHelperExtensions
    {
        /// <inheritdoc cref="VertexHelperUtilities.DrawTri(UnityEngine.UI.VertexHelper, Vector2, Vector2, Vector2, Color)"/>
        public static void DrawTri(this UnityEngine.UI.VertexHelper vh, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
            => VertexHelperUtilities.DrawTri(vh, p1, p2, p3, color);

        /// <inheritdoc cref="VertexHelperUtilities.DrawLine(UnityEngine.UI.VertexHelper, Vector2, Vector2, float, Color)"/>
        public static void DrawLine(this UnityEngine.UI.VertexHelper vh, Vector2 startPt, Vector2 endPt, float thickness, Color color)
            => VertexHelperUtilities.DrawLine(vh, startPt, endPt, thickness, color);

        /// <inheritdoc cref="VertexHelperUtilities.DrawQuad(UnityEngine.UI.VertexHelper, Vector2, Vector2, Vector2, Vector2, Color)"/>"/>
        public static void DrawQuad(this UnityEngine.UI.VertexHelper vh, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color color)
            => VertexHelperUtilities.DrawQuad(vh, p1, p2, p3, p4, color);
    }
}
