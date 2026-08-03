using BeltainsTools.BTInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    public static class IO
    {
        public static readonly string projectPath = System.IO.Directory.GetParent(Application.dataPath).FullName;

        /// <returns>The full path to the specified subPath within the project folder. eg. subPath = "Assets/MyFolder/SubFolder" returns "C:/Documents/YourProject/Assets/MyFolder/SubFolder"</returns>
        public static string GetProjectPath(string subPath)
        {
            if (string.IsNullOrEmpty(subPath))
                return projectPath;
            return System.IO.Path.Combine(projectPath, subPath);
        }


        /// <summary>Deletes all files in the specified directory after prompting the user for confirmation. This method is only available in the Unity Editor.</summary>
        public static void DeleteAllFilesInDirectory(string directoryPath)
        {
            if (!System.IO.Directory.Exists(directoryPath))
            {
                d.LogWarning($"Directory does not exist: {directoryPath}. Deleting 0 files...");
                return;
            }

            string[] files = System.IO.Directory.GetFiles(directoryPath);

            if (files.Length == 0)
            {
                d.Log($"No files found at {directoryPath}, deleting 0 files...");
                return;
            }

            if (!EditorUtility.DisplayDialog("WARNING: DELETING FILES",
                $"Are you sure you want to delete all files at \"{directoryPath}\"?\nThis cannot be undone.",
                "Yeee", "No, wait.."))
                return;

            int deletedCount = 0;

            foreach (string file in files)
            {
                try
                {
                    System.IO.File.Delete(file);
                    deletedCount++;
                }
                catch (System.Exception ex)
                {
                    d.LogError($"Failed to delete file '{file}': {ex.Message}");
                }
            }

            d.Log($"Deleted {deletedCount} files from {directoryPath}.");
        }


        /// <summary>Ensures that the given relative path exists in the project folder, creating missing folders as needed.</summary>
        /// <param name="path">Path relative to the project folder, e.g., "Assets/MyFolder/SubFolder"</param>
        public static void EnsureProjectPathExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            // Normalize path and remove file name if present
            string normalizedPath = path.Replace("\\", "/");
            if (System.IO.Path.HasExtension(normalizedPath))
                normalizedPath = System.IO.Path.GetDirectoryName(normalizedPath).Replace("\\", "/");

            string[] folders = normalizedPath.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = $"{currentPath}/{folders[i]}";
                string nextPathOnDisk = GetProjectPath(nextPath).Replace('/', System.IO.Path.DirectorySeparatorChar);
                if (!System.IO.Directory.Exists(nextPathOnDisk))
                    System.IO.Directory.CreateDirectory(nextPathOnDisk);
                currentPath = nextPath;
            }
        }



        [System.Obsolete("Correcting naming convention. Use GetObjectsInAssetsFolder<T> instead...")]
        /// <inheritdoc cref="GetObjectsInAssetsFolder(string, string, System.Type, bool)"/>
        public static IEnumerable<T> GetObjectsInProjectPath<T>(string assetsPath, string fileExtension, bool includeSubfolders = true) where T : Object
            => GetObjectsInAssetsFolder<T>(assetsPath, fileExtension, includeSubfolders);

        [System.Obsolete("Correcting naming convention. Use GetObjectsInAssetsFolder instead...")]
        /// <inheritdoc cref="GetObjectsInAssetsFolder(string, string, System.Type, bool)"/>
        public static IEnumerable<Object> GetObjectsInProjectPath(string assetsPath, string fileExtension, System.Type type, bool includeSubfolders = true)
            => GetObjectsInAssetsFolder(assetsPath, fileExtension, type, includeSubfolders);

        /// <inheritdoc cref="GetObjectsInAssetsFolder(string, string, System.Type, bool)"/>
        public static IEnumerable<UnityEngine.Object> GetObjectsInAssetsFolder(string assetsPath, string fileExtension, bool includeSubfolders = true)
            => GetObjectsInAssetsFolder(assetsPath, fileExtension, typeof(UnityEngine.Object), includeSubfolders);

        /// <inheritdoc cref="GetObjectsInAssetsFolder(string, string, System.Type, bool)"/>
        public static IEnumerable<T> GetObjectsInAssetsFolder<T>(string assetsPath, string fileExtension, bool includeSubfolders = true) where T : Object
        {
            foreach (UnityEngine.Object obj in GetObjectsInAssetsFolder(assetsPath, fileExtension, typeof(T), includeSubfolders))
                yield return obj as T;
        }

        /// <summary>
        /// Get a collection of objects of the given type in the given path (relative to assets/. Eg. Prefabs/... though also supports "Assets/Prefabs/..." format) found in through files of the given extension (eg. .prefab)
        /// </summary>
        /// <param name="assetsPath">The path to search within relative to the assets folder. Eg. "Prefabs" or "Assets/Prefabs"</param>
        /// <param name="type">The type of object to search for. Eg. typeof(GameObject)</param>
        /// <param name="fileExtension">The file extensions to search through. Eg. ".asset"</param>
        /// <returns>All assets found at the specified path with the specified file extension, or, if that directory does not exist, null</returns>
        public static IEnumerable<UnityEngine.Object> GetObjectsInAssetsFolder(string assetsPath, string fileExtension, System.Type type, bool includeSubfolders = true)
        {
            // Normalize path to be relative to Assets (strip leading "Assets/" or "Assets\\")
            string normalizedPath = assetsPath.Replace("\\", "/");
            if (normalizedPath.StartsWith("Assets/"))
                normalizedPath = normalizedPath.Substring("Assets/".Length);
            else if (normalizedPath == "Assets")
                normalizedPath = string.Empty;

            string fullPath = string.IsNullOrEmpty(normalizedPath)
                ? Application.dataPath
                : Application.dataPath + "/" + normalizedPath;

            if (!System.IO.Directory.Exists(fullPath))
                yield break;

            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(fullPath);
            System.IO.FileInfo[] fileInfos = directoryInfo.GetFiles($"*{fileExtension}", includeSubfolders ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);

            for (int i = 0; i < fileInfos.Length; i++)
            {
                string objectPath = fileInfos[i].FullName.Replace("\\", "/");
                int assetsIndex = objectPath.IndexOf("Assets/");
                if (assetsIndex < 0) assetsIndex = objectPath.IndexOf("Assets");
                if (assetsIndex >= 0)
                    objectPath = objectPath.Substring(assetsIndex);

                UnityEngine.Object loadedObject = AssetDatabase.LoadAssetAtPath(objectPath, type);
                if (loadedObject != null)
                    yield return loadedObject;
            }
        }


        /// <inheritdoc cref="GetOrCreateEditorSessionDataObject(System.Type, string)"/>
        public static T GetOrCreateEditorSessionDataObject<T>(string fileSubPathWithExt) where T : UnityEngine.Object
            => GetOrCreateEditorSessionDataObject(typeof(T), fileSubPathWithExt) as T;

        /// <summary>Gets or creates an editor session data object of the specified type at the specified path within the editor session data folder</summary>
        public static UnityEngine.Object GetOrCreateEditorSessionDataObject(System.Type objectType, string fileSubPathWithExt)
        {
            string assetPath = GetEditorSessionDataAssetPath(fileSubPathWithExt);

            EnsureProjectPathExists(assetPath);
            UnityEngine.Object loadedObject = AssetDatabase.LoadAssetAtPath(assetPath, objectType) as UnityEngine.Object;
            if (loadedObject == null)
                return CreateEditorSessionDataObject(objectType, fileSubPathWithExt);

            return loadedObject;
        }

        /// <inheritdoc cref="CreateEditorSessionDataObject(System.Type, string)"/>
        public static T CreateEditorSessionDataObject<T>(string fileSubPathWithExt) where T : UnityEngine.Object
            => CreateEditorSessionDataObject(typeof(T), fileSubPathWithExt) as T;

        /// <summary>Creates an editor session data object of the specified type at the specified path within the editor session data folder</summary>
        public static UnityEngine.Object CreateEditorSessionDataObject(System.Type type, string fileSubPathWithExt)
        {
            string assetPath = GetEditorSessionDataAssetPath(fileSubPathWithExt);

            DeleteEditorSessionDataObject(fileSubPathWithExt);

            UnityEngine.Object instance = ScriptableObject.CreateInstance(type);
            EnsureProjectPathExists(assetPath);
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return instance;
        }

        /// <summary>Deletes an editor session data object at the specified path within the editor session data folder</summary>
        public static void DeleteEditorSessionDataObject(string fileSubPathWithExt)
        {
            string assetPath = GetEditorSessionDataAssetPath(fileSubPathWithExt);

            if (!System.IO.File.Exists(assetPath))
                return;
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetEditorSessionDataAssetPath(string subPath)
            => System.IO.Path.Combine(PackageData.Paths.Assets.k_Editor_SessionData, subPath);
    }
}
