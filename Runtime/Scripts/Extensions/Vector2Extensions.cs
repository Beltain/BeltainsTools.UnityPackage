using UnityEngine;

namespace BeltainsTools
{
    public static class Vector2Extensions
    {
        /// <summary>Cheeky func. A shorthand that interperates the Vector2 as a Range.</summary>
        public static float Random(this Vector2 vector2) => UnityEngine.Random.Range(vector2.x, vector2.y);

        /// <summary>Cheeky func. Determines whether the provided Vector2 'range' contains at least some of the other Vector2 'range'</summary>
        public static bool RangeOverlaps(this Vector2 thisRange, Vector2 otherRange) => thisRange.RangeContains(otherRange.x) || thisRange.RangeContains(otherRange.y);
        /// <summary>Cheeky func. Determines whether the provided Vector2 'range' contains the whole of the other Vector2 'range'</summary>
        public static bool RangeContains(this Vector2 thisRange, Vector2 otherRange) => thisRange.RangeContains(otherRange.x) && thisRange.RangeContains(otherRange.y);
        /// <summary>Cheeky func. Determines whether the provided Vector2 'range' contains the given value</summary>
        public static bool RangeContains(this Vector2 range, float value) => range.x > range.y ? (value <= range.x && value >= range.y) : (value <= range.y && value >= range.x);

        public static float Lerp(this Vector2 vector2, float t) => UnityEngine.Mathf.Lerp(vector2.x, vector2.y, t);
        public static float InverseLerp(this Vector2 vector2, float value) => UnityEngine.Mathf.InverseLerp(vector2.x, vector2.y, value);

        public static Vector2 SetX(this Vector2 vector, float x) => new Vector2(x, vector.y);
        public static Vector2 SetY(this Vector2 vector, float y) => new Vector2(vector.x, y);

        public static Vector3 ToVector3XY(this Vector2 vector) => new Vector3(vector.x, vector.y, 0f);
        public static Vector3 ToVector3XZ(this Vector2 vector) => new Vector3(vector.x, 0f, vector.y);
        public static Vector3 ToVector3YZ(this Vector2 vector) => new Vector3(0f, vector.x, vector.y);
    }
}
