using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeltainsTools
{
    public static class IO
    {
        [System.Obsolete("Not entirely supported, keeping for reference")]
        public static IEnumerable GetObjectsInProjectPathOfType(System.Type type, string path, string fileExtension, bool includeSubfolders = true)
        {
#if UNITY_EDITOR
            // Get the generic type definition
            System.Reflection.MethodInfo method = typeof(IO).GetMethod("GetObjectsInProjectPath",
                                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            // Build a method with the specific type argument you're interested in
            method = method.MakeGenericMethod(type);
            // The "null" is because it's a static method
            return (IEnumerable)method.Invoke(null, new object[] { path, fileExtension, includeSubfolders });
#else
            throw new System.NotSupportedException("Attempted to use GetObjectsInProjectPathOfType outside of editor environment! This is not supported!");
#endif
        }

        /// <summary>
        /// Get a collection of objects of the given type in the given path (relative to assets/. Eg. Prefabs/...) found in through files of the given extension (eg. .prefab)
        /// </summary>
        /// <param name="path">The path to search within relative to the assets folder. Eg. Prefabs</param>
        /// <param name="fileExtension">The file extensions to search through. Eg. ".prefab"</param>
        public static IEnumerable<T> GetObjectsInProjectPath<T>(string path, string fileExtension, bool includeSubfolders = true) where T : Object
        {
#if UNITY_EDITOR
            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(Application.dataPath + "/" + path);
            System.IO.FileInfo[] fileInfos = directoryInfo.GetFiles($"*{fileExtension}", includeSubfolders ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);

            List<T> newAssetCollection = new List<T>();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                string objectPath = fileInfos[i].FullName.Remove(0, fileInfos[i].FullName.IndexOf("Assets")); //Strip project folder from path

                T loadedObject = AssetDatabase.LoadAssetAtPath<T>(objectPath);
                if (loadedObject != null)
                {
                    newAssetCollection.Add(loadedObject);
                }
            }
            return newAssetCollection;
#else
            throw new System.NotSupportedException("Attempted to use GetObjectsInProjectPath outside of editor environment! This is not supported!");
#endif
        }
    }
}
