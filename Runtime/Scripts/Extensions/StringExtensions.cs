using BeltainsTools.Utilities;
using UnityEngine;

namespace BeltainsTools
{
    public static class StringExtensions
    {
        /// <inheritdoc cref="StringUtilities.GetIsEmpty(string)"/>
        public static bool IsEmpty(this string str)
            => StringUtilities.GetIsEmpty(str);

        /// <inheritdoc cref="StringUtilities.GetIsNullOrEmpty(string)"/>
        public static bool IsNullOrEmpty(this string str)
            => StringUtilities.GetIsNullOrEmpty(str);

    }
}
