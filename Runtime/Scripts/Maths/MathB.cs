using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BeltainsTools
{
    /// <summary>Class for generally useful mathematical operations</summary>
    public static class MathB
    {
        public static Vector3 QuadraticLerp(Vector3 pointA, Vector3 pointB, Vector3 pointC, float t)
        {
            return Vector3.Lerp(Vector3.Lerp(pointA, pointB, t), Vector3.Lerp(pointB, pointC, t), t);
        }

        public static Vector3 CubicLerp(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, float t)
        {
            return Vector3.Lerp(QuadraticLerp(pointA, pointB, pointC, t), QuadraticLerp(pointB, pointC, pointD, t), t);
        }

        /// <inheritdoc cref="BeltainsTools.Utilities.LineUtilities.GetAreDirectionsParallel(Vector2, Vector2, Vector2, Vector2)"/>
        public static bool GetAreDirectionsParallel(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB)
        {
            return BeltainsTools.Utilities.LineUtilities.GetAreDirectionsParallel(originA, directionA, originB, directionB);
        }

        /// <inheritdoc cref="BeltainsTools.Utilities.LineUtilities.GetDirectionsIntersection(Vector2, Vector2, Vector2, Vector2, out Vector2)"/>
        public static bool GetDirectionsIntersection(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB, out Vector2 intersection, bool ensureCrossesInDirection = false)
        {
            return BeltainsTools.Utilities.LineUtilities.GetDirectionsIntersection(originA, directionA, originB, directionB, out intersection, ensureCrossesInDirection);
        }

        /// <inheritdoc cref="BeltainsTools.Utilities.LineUtilities.GetLinesIntersection(Vector2, Vector2, Vector2, Vector2, out Vector2)"/>
        public static bool GetLinesIntersection(Vector2 lineAStart, Vector2 lineAEnd, Vector2 lineBStart, Vector2 lineBEnd, out Vector2 intersection)
        {
            return BeltainsTools.Utilities.LineUtilities.GetLinesIntersection(lineAStart, lineAEnd, lineBStart, lineBEnd, out intersection);
        }

        /// <summary>Returns the normalized value that represents where <paramref name="value"/> falls between <paramref name="a"/> and <paramref name="b"/></summary>
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            float delta = b - a;
            return delta.Approximately(0f) ? 0f : (value - a) / delta;
        }
    }
}
