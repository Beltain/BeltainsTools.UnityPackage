using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class RectExtensions
    {
        /// <inheritdoc cref="RectUtilities.Encapsulate(Rect, Vector2)"/>
        public static Rect Encapsulate(this Rect rect, Vector2 point)
            => RectUtilities.Encapsulate(rect, point);
    }
}
