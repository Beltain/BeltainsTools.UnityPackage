using UnityEngine;

namespace BeltainsTools
{
    public static class Vector3Extensions
    {
        public static Vector3 SetX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);
        public static Vector3 SetY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);
        public static Vector3 SetZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

        public static Vector3 GetX(this Vector3 vector) => new Vector3(vector.x, 0f, 0f);
        public static Vector3 GetY(this Vector3 vector) => new Vector3(0f, vector.y, 0f);
        public static Vector3 GetZ(this Vector3 vector) => new Vector3(0f, 0f, vector.z);

        public static Vector2 ToVector2XY(this Vector3 vector) => new Vector2(vector.x, vector.y);
        public static Vector2 ToVector2XZ(this Vector3 vector) => new Vector2(vector.x, vector.z);
        public static Vector2 ToVector2YZ(this Vector3 vector) => new Vector2(vector.y, vector.z);
        public static Vector2 ToVector2ZY(this Vector3 vector) => new Vector2(vector.z, vector.y);
        public static Vector2 ToVector2ZX(this Vector3 vector) => new Vector2(vector.z, vector.x);
        public static Vector2 ToVector2YX(this Vector3 vector) => new Vector2(vector.y, vector.x);

        public static bool IsNegativeInfinity(this Vector3 vector) => vector.x.IsNegativeInfinity() || vector.y.IsNegativeInfinity() || vector.z.IsNegativeInfinity();
        public static bool IsPositiveInfinity(this Vector3 vector) => vector.x.IsPositiveInfinity() || vector.y.IsPositiveInfinity() || vector.z.IsPositiveInfinity();
        public static bool IsInfinity(this Vector3 vector) => vector.x.IsInfinity() || vector.y.IsInfinity() || vector.z.IsInfinity();

        /// <summary>Translate this vector so that it's relative to the camera forward</summary>
        public static Vector3 RelativeToCameraGroundForward(this Vector3 vector, Camera cam = null)
        {
            if (cam == null)
                cam = Camera.main;
            Quaternion rotationToCameraForward = Quaternion.FromToRotation(Vector3.forward, cam.transform.forward.SetY(0).normalized);
            return rotationToCameraForward * vector;
        }
    }
}
