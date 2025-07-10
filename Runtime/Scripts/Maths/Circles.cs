using UnityEngine;

namespace BeltainsTools.Maths
{
    public class Circles
    {
        public static Vector2 GetAngledPointOnUnitCircleD(float angleDegrees) => GetAngledPointOnUnitCircleD(Vector2.zero, angleDegrees);
        public static Vector2 GetAngledPointOnUnitCircleR(float angleRadians) => GetAngledPointOnUnitCircleR(Vector2.zero, angleRadians);
        public static Vector2 GetAngledPointOnUnitCircleD(Vector2 center, float angleDegrees) => GetAngledPointOnUnitCircleR(center, angleDegrees * Mathf.Deg2Rad);
        public static Vector2 GetAngledPointOnUnitCircleR(Vector2 center, float angleRadians)
        {
            return new Vector2(
                    center.x + Mathf.Cos(angleRadians),
                    center.y + Mathf.Sin(angleRadians)
                );
        }
    }
}
