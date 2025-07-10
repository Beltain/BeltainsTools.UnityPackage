using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BeltainsTools.Maths
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
    }
}
