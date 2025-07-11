using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class TypeExtensions
    {
        /// <inheritdoc cref="TypeUtilities.GetTypeIdentifierString(System.Type)"/>
        public static string ToTypeIdentifierString(this System.Type type)
            => TypeUtilities.GetTypeIdentifierString(type);
    }
}
