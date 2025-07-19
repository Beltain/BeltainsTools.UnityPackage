using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class RectExtensions
    {
        /// <inheritdoc cref="RectUtilities.Encapsulate(Rect, Vector2)"/>
        public static Rect Encapsulate(this Rect rect, Vector2 point)
            => RectUtilities.Encapsulate(rect, point);

        /// <inheritdoc cref="RectUtilities.LerpUnclamped(Rect, Vector2)"/>
        public static Vector2 LerpUnclamped(this Rect rect, Vector2 pointT)
            => RectUtilities.LerpUnclamped(rect, pointT);
        /// <inheritdoc cref="RectUtilities.Lerp(Rect, Vector2)"/>
        public static Vector2 Lerp(this Rect rect, Vector2 pointT)
            => RectUtilities.Lerp(rect, pointT);

        /// <inheritdoc cref="RectUtilities.InverseLerp(Rect, Vector2)"/>
        public static Vector2 InverseLerp(this Rect rect, Vector2 point)
            => RectUtilities.InverseLerp(rect, point);
        /// <inheritdoc cref="RectUtilities.InverseLerpUnclamped(Rect, Vector2)"/>
        public static Vector2 InverseLerpUnclamped(this Rect rect, Vector2 point)
            => RectUtilities.InverseLerpUnclamped(rect, point);
    }
}
