using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class RectUtilities
    {
        public static Rect Encapsulate(Rect rect, Vector2 point)
        {
            float xMin = Mathf.Min(rect.xMin, point.x);
            float yMin = Mathf.Min(rect.yMin, point.y);
            float xMax = Mathf.Max(rect.xMax, point.x);
            float yMax = Mathf.Max(rect.yMax, point.y);

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        /// <inheritdoc cref="LerpUnclamped(Rect, Vector2)"/>
        public static Vector2 Lerp(Rect rect, Vector2 pointT) => LerpUnclamped(rect, pointT.ClampAxes01());
        /// <summary>Determines where the point falls within the rect by interpolating each axis component of the <paramref name="pointT"/> value</summary>
        public static Vector2 LerpUnclamped(Rect rect, Vector2 pointT)
        {
            return new Vector2(
                Mathf.LerpUnclamped(rect.xMin, rect.xMax, pointT.x),
                Mathf.LerpUnclamped(rect.yMin, rect.yMax, pointT.y)
            );
        }

        /// <inheritdoc cref="InverseLerpUnclamped(Rect, Vector2)"/>
        public static Vector2 InverseLerp(Rect rect, Vector2 point) => InverseLerpUnclamped(rect, point).ClampAxes01();
        /// <summary>Inverse lerp the <paramref name="point"/>'s x and y values between the rect min and max values to determine where it falls within the <paramref name="rect"/></summary>
        public static Vector2 InverseLerpUnclamped(Rect rect, Vector2 point)
        {
            return new Vector2(
                MathB.InverseLerpUnclamped(rect.xMin, rect.xMax, point.x),
                MathB.InverseLerpUnclamped(rect.yMin, rect.yMax, point.y)
            );
        }
    }
}
