using UnityEngine;

namespace BeltainsTools
{
    public static class Vector3IntExtensions
    {
        public static Vector3Int SetX(this Vector3Int vector, int x) => new Vector3Int(x, vector.y, vector.z);
        public static Vector3Int SetY(this Vector3Int vector, int y) => new Vector3Int(vector.x, y, vector.z);
        public static Vector3Int SetZ(this Vector3Int vector, int z) => new Vector3Int(vector.x, vector.y, z);

        public static Vector3Int IsolateX(this Vector3Int vector) => new Vector3Int(vector.x, 0, 0);
        public static Vector3Int IsolateY(this Vector3Int vector) => new Vector3Int(0, vector.y, 0);
        public static Vector3Int IsolateZ(this Vector3Int vector) => new Vector3Int(0, 0, vector.z);

        public static Vector2Int ToVector2XY(this Vector3Int vector) => new Vector2Int(vector.x, vector.y);
        public static Vector2Int ToVector2XZ(this Vector3Int vector) => new Vector2Int(vector.x, vector.z);
        public static Vector2Int ToVector2YZ(this Vector3Int vector) => new Vector2Int(vector.y, vector.z);
        public static Vector2Int ToVector2ZY(this Vector3Int vector) => new Vector2Int(vector.z, vector.y);
        public static Vector2Int ToVector2ZX(this Vector3Int vector) => new Vector2Int(vector.z, vector.x);
        public static Vector2Int ToVector2YX(this Vector3Int vector) => new Vector2Int(vector.y, vector.x);
    }
}
