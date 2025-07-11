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
    }
}
