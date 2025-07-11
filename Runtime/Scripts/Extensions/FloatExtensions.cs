using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class FloatExtensions
    {
        public static bool IsNegativeInfinity(this float value) => float.IsNegativeInfinity(value);
        public static bool IsPositiveInfinity(this float value) => float.IsPositiveInfinity(value);
        public static bool IsInfinity(this float value) => float.IsInfinity(value);

        /// <inheritdoc cref="FloatUtilities.Approximately(float, float, float)"/>
        public static bool Approximately(this float thisFloat, float value, float epsilon = 0.0001f)
            => FloatUtilities.Approximately(thisFloat, value, epsilon);
    }
}
