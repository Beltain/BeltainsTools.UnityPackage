using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class TypeUtilities
    {
        public static string GetTypeIdentifierString(System.Type type) //used to be a GUID thing, didn't work out
        {
            return $"Type.{type.Namespace}.{type.Name}";
        }
    }
}
