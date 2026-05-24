using UnityEngine;

namespace BeltainsTools
{
    public static class Vector2IntExtensions
    {
        /// <summary>Cheeky func. A shorthand that interperates the Vector2Int as a Range (inclusive, exclusive).</summary>
        public static int Random(this Vector2Int vector2Int) => UnityEngine.Random.Range(vector2Int.x, vector2Int.y);

        public static Vector2Int SetX(this Vector2Int vector, int x) => new Vector2Int(x, vector.y);
        public static Vector2Int SetY(this Vector2Int vector, int y) => new Vector2Int(vector.x, y);

        public static Vector3Int ToVector3IntXY(this Vector2Int vector) => new Vector3Int(vector.x, vector.y, 0);
        public static Vector3Int ToVector3IntXZ(this Vector2Int vector) => new Vector3Int(vector.x, 0, vector.y);
        public static Vector3Int ToVector3IntYZ(this Vector2Int vector) => new Vector3Int(0, vector.x, vector.y);
    }
}
