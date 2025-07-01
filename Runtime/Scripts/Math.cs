using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BeltainsTools
{
    public static class MathB
    {
        public static class Vector3
        {
            public static UnityEngine.Vector3 QuadraticInterpolation(UnityEngine.Vector3 pointA, UnityEngine.Vector3 pointB, UnityEngine.Vector3 pointC, float t)
            {
                return UnityEngine.Vector3.Lerp(UnityEngine.Vector3.Lerp(pointA, pointB, t), UnityEngine.Vector3.Lerp(pointB, pointC, t), t);
            }

            public static UnityEngine.Vector3 CubicInterpolation(UnityEngine.Vector3 pointA, UnityEngine.Vector3 pointB, UnityEngine.Vector3 pointC, UnityEngine.Vector3 pointD, float t)
            {
                return UnityEngine.Vector3.Lerp(QuadraticInterpolation(pointA, pointB, pointC, t), QuadraticInterpolation(pointB, pointC, pointD, t), t);
            }
        }
    }
}
