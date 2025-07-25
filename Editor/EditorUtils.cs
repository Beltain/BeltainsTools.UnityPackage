using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using BeltainsTools.Utilities;
using System.IO;
using UnityEngine.UI;

namespace BeltainsTools.Editor
{
    public static class Utils
    {
        public static Dictionary<Object, List<int>> GetAssetReferenceProperties<T>(T asset, string operationTitle = null) where T : Object
        {
            Dictionary<Object, List<int>> referenceProperties = new Dictionary<Object, List<int>>();

            string[] allAssetGUIDs = AssetDatabase.FindAssets("t:Object");

            string title = operationTitle.IsNullOrEmpty() ? "Finding References..." : operationTitle;

            EditorUtility.DisplayProgressBar(title, $"Finding references in {allAssetGUIDs.Length} objects", 0);

            int index = 0;
            foreach (string assetGUID in allAssetGUIDs)
            {
                EditorUtility.DisplayProgressBar(title, $"Finding references across all objects... {index + 1}/{allAssetGUIDs.Length} objects", ((float)(index + 1) / allAssetGUIDs.Length));
                Object dependentAsset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(assetGUID));
                index ++;

                if (dependentAsset == null || dependentAsset == asset)
                    continue;

                SerializedObject serializedObject = new SerializedObject(dependentAsset);
                SerializedProperty property = serializedObject.GetIterator();
                int iterIndex = 0;
                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == asset)
                    {
                        if (!referenceProperties.ContainsKey(dependentAsset))
                            referenceProperties[dependentAsset] = new List<int>();
                        referenceProperties[dependentAsset].Add(iterIndex);
                    }
                    iterIndex++;
                }
            }

            EditorUtility.ClearProgressBar();

            return referenceProperties;
        }

        public static void SetReferenceAsset<T>(Dictionary<Object, List<int>> properties, T asset) where T : Object
        {
            foreach (Object propObject in properties.Keys)
            {
                SerializedObject serializedObject = new SerializedObject(propObject);
                SerializedProperty iteratedProp = serializedObject.GetIterator();
                int iterIndex = 0;
                while (iteratedProp.Next(true))
                {
                    if (properties[propObject].Contains(iterIndex))
                        iteratedProp.objectReferenceValue = asset;
                    iterIndex++;
                }

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ReplaceReferences(Object oldAsset, Object newAsset)
        {
            foreach (string assetGUID in AssetDatabase.FindAssets("t:Object"))
            {
                Object dependentAsset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(assetGUID));

                if (dependentAsset == null || dependentAsset == oldAsset) continue;

                SerializedObject serializedObject = new SerializedObject(dependentAsset);
                SerializedProperty property = serializedObject.GetIterator();
                bool assetChanged = false;
                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == oldAsset)
                    {
                        property.objectReferenceValue = newAsset;
                        assetChanged = true;
                    }
                }

                if (assetChanged)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(dependentAsset);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        public static void SaveTextFile(string content, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    File.WriteAllText(path, content);
                    Debug.Log($"File saved successfully at: {path}");
                }
                catch (IOException e)
                {
                    Debug.LogError($"Failed to save file: {e.Message}");
                }
            }
        }

        public static Texture2DArray StitchTexture2DArray(IEnumerable<Texture2D> textures)
        {
            if (textures == null || textures.Count() == 0)
            {
                d.LogError("Tried to stitch texture 2D array but there were no provided textures! Aborting!");
                return null;
            }

            int texturesCount = textures.Count();
            Texture2D referenceTex = textures.First();

            Vector2Int textureDims = new Vector2Int(referenceTex.width, referenceTex.height);
            TextureFormat format = referenceTex.format;
            FilterMode filterMode = referenceTex.filterMode;
            bool useMipMaps = referenceTex.mipmapCount > 1;

            d.Assert(!textures.Any(r => r.width != textureDims.x || r.height != textureDims.y), "Failed to stitch textures into texture 2D array as provided textures were not all of the same dimension!");

            Texture2DArray textureArray = new Texture2DArray(textureDims.x, textureDims.y, texturesCount, format, useMipMaps);
            textureArray.filterMode = filterMode;

            int texI = 0;
            foreach (Texture2D tex in textures)
            {
                for (int mipI = 0; mipI < tex.mipmapCount; mipI++)
                    Graphics.CopyTexture(tex, 0, mipI, textureArray, texI, mipI);
                texI++;
            }

            textureArray.Apply(false, true);
            return textureArray;
        }

        private static string GetFolderForObject(Object obj)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
                return string.Empty; // not a valid asset path, maybe the obj doesn't exist in the asset database? ie. scene object

            if (AssetDatabase.IsValidFolder(assetPath))
                return assetPath; // if object is folder, return it

            return System.IO.Path.GetDirectoryName(assetPath);
        }



        [MenuItem("Assets/Create/" + BTInternal.PackageData.k_PrettyName + "/Texture2DArray from Selection", isValidateFunction: true, priority = 1)]
        public static bool ValidateCreateTexture2DArray()
        {
            foreach (Object textureObject in Selection.objects)
                if (!(textureObject is Texture2D)) return false;
            return Selection.objects.Length > 0;
        }

        [MenuItem("Assets/Create/" + BTInternal.PackageData.k_PrettyName + "/Texture2DArray from Selection", priority = 1)]
        public static void CreateTexture2DArray()
        {
            Texture2D[] textures = Selection.objects.Select(r => r as Texture2D).ToArray();
            Texture2DArray textureArrayResult = StitchTexture2DArray(textures);
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Texture2DArray",
                "New Texture2D Array",
                "asset",
                "Choose where to save the newly stitched Texture2D Array",
                GetFolderForObject(Selection.activeObject)
                );

            if (string.IsNullOrEmpty(path))
            {
                d.LogError("No path set for stitched Texture2DArray to be saved! Aborting");
                return;
            }


            Texture2DArray previousArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);
            Dictionary<Object, List<int>> previousArrayReferenceProps = null;
            if (previousArray != null)
                previousArrayReferenceProps = GetAssetReferenceProperties(previousArray, "Replacing references to previous Array");
            AssetDatabase.CreateAsset(textureArrayResult, path);
            if (previousArrayReferenceProps != null)
                SetReferenceAsset(previousArrayReferenceProps, textureArrayResult);

            string orderDataPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "(OrderData).asset");
            Texture2DArrayOrderData existingArrayOrderData = AssetDatabase.LoadAssetAtPath<Texture2DArrayOrderData>(orderDataPath);
            if(existingArrayOrderData != null)
            {
                existingArrayOrderData.Array = textureArrayResult;
                existingArrayOrderData.OrderedElements = textures;
                EditorUtility.SetDirty(existingArrayOrderData);
            }
            else
            {
                Texture2DArrayOrderData arrayOrderData = ScriptableObject.CreateInstance<Texture2DArrayOrderData>();
                arrayOrderData.Array = textureArrayResult;
                arrayOrderData.OrderedElements = textures;
                AssetDatabase.CreateAsset(arrayOrderData, orderDataPath);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>Same as <see cref="TryCreatePresetFromPrefabPath(MenuCommand, bool, string[])"/> but paths are generated from <see cref="Globals.k_PackageRoots"/></summary>
        internal static bool TryCreateBeltainsToolsPresetFromPrefabPath(MenuCommand menuCommand, bool keepPrefabReference, string packageRootRelativePath) 
        {
            string[] paths = new string[BTInternal.PackageData.k_PackageRoots.Length];
            for (int i = 0; i < BTInternal.PackageData.k_PackageRoots.Length; i++)
                paths[i] = Path.Combine(BTInternal.PackageData.k_PackageRoots[i], packageRootRelativePath);
            return TryCreatePresetFromPrefabPath(menuCommand, keepPrefabReference, paths);
        }

        /// <summary>Instantiate a preset prefab from a given path and menu command context (right click menu in heirarchy)</summary>
        public static bool TryCreatePresetFromPrefabPath(MenuCommand menuCommand, bool keepPrefabReference, params string[] pathsToAttempt)
        {
            GameObject prefab = null;
            List<string> attemptedPaths = new List<string>(pathsToAttempt);
            for (int i = 0; i < pathsToAttempt.Length && prefab == null; i++)
            {
                string path = pathsToAttempt[i];
                attemptedPaths.Add(path);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (i == pathsToAttempt.Length && prefab == null)
                {
                    d.LogError("Attempted to load prefab at all provided paths but none were valid!\n" +
                        $"Paths attempted:\n{string.Join(",\n", attemptedPaths)}");
                    return false;
                }
            }

            string instanceName = $"new {prefab.name}";

            GameObject parent = menuCommand.context is GameObject menuContextGO ? menuContextGO : Selection.activeGameObject;

            if (prefab.transform is RectTransform)
            {
                // it's a UI prefab, so we need to ensure it gets parented to a canvas
                if (parent == null || parent.transform.GetComponentInParents<Canvas>() == null)
                {
                    Canvas canvas = CreateDefaultCanvas(parent, includeEventSystem: false);
                    parent = canvas.gameObject;
                }
            }

            GameObject instance = keepPrefabReference ? (GameObject)PrefabUtility.InstantiatePrefab(prefab) : GameObject.Instantiate(prefab);
            instance.name = instanceName;
            if (parent != null)
                GameObjectUtility.SetParentAndAlign(instance, parent);

            Undo.RegisterCreatedObjectUndo(instance, "Create " + prefab.name);

            Selection.activeGameObject = instance;

            return true;
        }

        /// <summary>Create a default canvas, mimicking unity's default canvas created when you try to add a UI gameobject in a scene without a canvas</summary>
        public static Canvas CreateDefaultCanvas(GameObject parent, bool includeEventSystem = true, bool select = false)
        {
            // Create and configure canvasnd its components
            GameObject canvasGO = 
                new GameObject("Canvas", 
                    typeof(Canvas), 
                    typeof(CanvasScaler), 
                    typeof(GraphicRaycaster)
                    );
            if (parent != null)
                GameObjectUtility.SetParentAndAlign(canvasGO, parent);
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // configure scaler
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Default Canvas");

            // Ensure there is an EventSystem in the scene
            if (includeEventSystem && Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemGO = 
                    new GameObject("EventSystem", 
                        typeof(UnityEngine.EventSystems.EventSystem), 
                        typeof(UnityEngine.EventSystems.StandaloneInputModule)
                        );

                Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create Default Canvas Event System");
            }

            if(select)
                Selection.activeGameObject = canvasGO;

            return canvas;
        }
    }
}
