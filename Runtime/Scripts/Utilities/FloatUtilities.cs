using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class FloatUtilities
    {
        public static bool Approximately(float thisFloat, float value, float epsilon = 0.0001f)
        {
            return Mathf.Abs(thisFloat - value) < epsilon;
        }
    }
}
