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
            public static class Assets
            {
                public const string k_Editor = "Assets/Editor/";
                public const string k_Editor_SessionData = k_Editor + Editor.k_Session_Data;
            }

            public static class Editor
            {
                public const string k_Session_Data = "Session/Data/";
            }

            public static class CreateAssetMenu
            { 
                public const string k_Base = k_PrettyName + "/";
                public const string k_Events = k_Base + "Events/";
                public const string k_Cinemachine = k_Base + "Cinemachine/";
            }

            public static class MenuItem
            {
                public const string k_Window = "Window/" + k_PrettyName + "/";
            }
        }
    }
}
