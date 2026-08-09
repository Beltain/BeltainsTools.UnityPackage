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

        public static bool Approximately(float valueA, float valueB, float epsilon = 0.0001f)
        {
            return Mathf.Abs(valueA - valueB) < epsilon;
        }

        /// <returns>True if the directions are parallel, false otherwise</returns>
        public static bool GetDirectionsParallel(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB)
        {
            float denominator = directionA.x * directionB.y - directionA.y * directionB.x;
            return Approximately(denominator, 0f);
        }

        /// <summary>
        /// Get the intersection point between two directed lines.<br/>
        /// Use <paramref name="ensureCrossesInDirection"/> to only check for intersections that are in the direction of the provided vectors, and not behind the origin points.
        /// </summary>
        /// <returns>True if the lines intersect, false otherwise</returns>
        public static bool GetDirectionsIntersect(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB, out Vector2 intersection, bool ensureCrossesInDirection = false)
        {
            intersection = Vector2.zero;

            if (GetDirectionsParallel(originA, directionA, originB, directionB))
                return false;

            float denominator = directionA.x * directionB.y - directionA.y * directionB.x;
            Vector2 diff = originB - originA;
            float t = (diff.x * directionB.y - diff.y * directionB.x) / denominator;

            intersection = originA + t * directionA;

            if (ensureCrossesInDirection)
            {
                // make sure our intersection is actually in the directions provided
                Vector2 aToI = (intersection - originA).normalized;
                Vector2 bToI = (intersection - originB).normalized;
                return Vector2.Dot(aToI, directionA) >= 0f && Vector2.Dot(bToI, directionB) >= 0f;
            }
            else
            {
                return true;
            }
        }

        /// <summary>Get the intersection point between two fixed length lines defined by their start and end points.</summary>
        /// <returns>True if the lines intersect, false otherwise</returns>
        public static bool GetLinesIntersect(Vector2 lineAStart, Vector2 lineAEnd, Vector2 lineBStart, Vector2 lineBEnd, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            float A1 = lineAEnd.y - lineAStart.y;
            float B1 = lineAStart.x - lineAEnd.x;
            float C1 = A1 * lineAStart.x + B1 * lineAStart.y;

            float A2 = lineBEnd.y - lineBStart.y;
            float B2 = lineBStart.x - lineBEnd.x;
            float C2 = A2 * lineBStart.x + B2 * lineBStart.y;

            float denominator = A1 * B2 - A2 * B1;

            if (Approximately(denominator, 0f))
            {
                // Lines are parallel (or coincident)
                return false;
            }

            float x = (B2 * C1 - B1 * C2) / denominator;
            float y = (A1 * C2 - A2 * C1) / denominator;

            intersection = new Vector2(x, y);
            return true;
        }

        /// <summary>Returns the normalized value that represents where <paramref name="value"/> falls between <paramref name="a"/> and <paramref name="b"/></summary>
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            float delta = b - a;
            return Approximately(delta, 0f) ? 0f : (value - a) / delta;
        }
    }
}
