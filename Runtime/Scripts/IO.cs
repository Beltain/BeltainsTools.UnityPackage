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

        /// <summary>Ensures that the given relative path exists in the project folder, creating missing folders as needed.</summary>
        /// <param name="path">Path relative to the project folder, e.g., "Assets/MyFolder/SubFolder"</param>
        public static void EnsureProjectPathExists(string path)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(path))
                return;

            // Normalize path and remove file name if present
            string normalizedPath = path.Replace("\\", "/");
            if (System.IO.Path.HasExtension(normalizedPath))
                normalizedPath = System.IO.Path.GetDirectoryName(normalizedPath).Replace("\\", "/");

            string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

            string[] folders = normalizedPath.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = $"{currentPath}/{folders[i]}";
                string nextPathOnDisk = System.IO.Path.Combine(projectRoot, nextPath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (!System.IO.Directory.Exists(nextPathOnDisk))
                    System.IO.Directory.CreateDirectory(nextPathOnDisk);
                currentPath = nextPath;
            }
#endif
        }

        /// <summary>
        /// Get a collection of objects of the given type in the given path (relative to assets/. Eg. Prefabs/... though also supports "Assets/Prefabs/..." format) found in through files of the given extension (eg. .prefab)
        /// </summary>
        /// <param name="path">The path to search within relative to the assets folder. Eg. "Prefabs" or "Assets/Prefabs"</param>
        /// <param name="fileExtension">The file extensions to search through. Eg. ".asset"</param>
        /// <returns>List of assets found at the specified path with the specified file extension, or, if that directory does not exist, null</returns>
        public static IEnumerable<T> GetObjectsInProjectPath<T>(string path, string fileExtension, bool includeSubfolders = true) where T : Object
        {
        #if UNITY_EDITOR
            // Normalize path to be relative to Assets (strip leading "Assets/" or "Assets\\")
            string normalizedPath = path.Replace("\\", "/");
            if (normalizedPath.StartsWith("Assets/"))
                normalizedPath = normalizedPath.Substring("Assets/".Length);
            else if (normalizedPath == "Assets")
                normalizedPath = string.Empty;

            string fullPath = string.IsNullOrEmpty(normalizedPath)
                ? Application.dataPath
                : Application.dataPath + "/" + normalizedPath;

            if (!System.IO.Directory.Exists(fullPath))
                return null;

            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(fullPath);
            System.IO.FileInfo[] fileInfos = directoryInfo.GetFiles($"*{fileExtension}", includeSubfolders ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);

            List<T> newAssetCollection = new List<T>();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                string objectPath = fileInfos[i].FullName.Replace("\\", "/");
                int assetsIndex = objectPath.IndexOf("Assets/");
                if (assetsIndex < 0) assetsIndex = objectPath.IndexOf("Assets");
                if (assetsIndex >= 0)
                    objectPath = objectPath.Substring(assetsIndex);

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
