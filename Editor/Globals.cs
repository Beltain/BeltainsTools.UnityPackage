using UnityEngine;

namespace BeltainsTools.Editor
{
    internal static class Globals
    {
        internal const string k_PackageName = "BeltainsTools";
        internal const string k_PrettyName = "\u270e" /*Pencil icon*/ + " " + k_PackageName;

        internal static readonly string[] k_PackageRoots = new string[]
        {
            "Packages/com.beltainjordaan.beltainstools",
            "Assets/Plugins/BeltainsTools"
        };
    }
}
