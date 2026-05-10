using UnityEngine;

namespace BeltainsTools.BTInternal
{
    public static class PackageData
    {
        public const string k_PackageName = "BeltainsTools";
        public const string k_PrettyName = "\u212C" /*Pencil icon*/ + " " + k_PackageName;

        public static readonly string[] k_PackageRoots = new string[]
        {
            "Packages/com.beltainjordaan.beltainstools",
            "Assets/Plugins/BeltainsTools"
        };

        public static class Paths
        {
            public static class CreateAssetMenu
            { 
                public const string k_Base = k_PrettyName + "/";
                public const string k_Events = k_Base + "Events/";
            }
        }
    }
}
