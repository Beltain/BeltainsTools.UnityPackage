using UnityEngine;

namespace BeltainsTools.BTInternal
{
    public static class PackageData
    {
        public const string k_PackageName = "BeltainsTools";
        public const string k_PrettyName = "\u270e" /*Pencil icon*/ + " " + k_PackageName;

        public static readonly string[] k_PackageRoots = new string[]
        {
            "Packages/com.beltainjordaan.beltainstools",
            "Assets/Plugins/BeltainsTools"
        };
    }
}
