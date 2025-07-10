using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class AssetDatabaseUtilities
    {
#if UNITY_EDITOR
        public static IEnumerable<T> GetPrefabsOfTypeInFolder<T>(string path) where T : Component
            => GetPrefabsOfTypeInFolder(path, typeof(T)).Select(r => r.GetComponent<T>());
        public static IEnumerable<Component> GetPrefabsOfTypeInFolder(string path, System.Type type)
        {
            string[] gUIDs = AssetDatabase.FindAssets("t:prefab", new string[] { path });
            IEnumerable<string> objectsPaths = gUIDs.Select(r => AssetDatabase.GUIDToAssetPath(r));
            IEnumerable<Component> components = objectsPaths.Select(r => AssetDatabase.LoadAssetAtPath<Component>(r));
            components = components.Where(r => r.GetComponent(type) != null);
            return components.Select(r => r.GetComponent(type));
        }

        public static IEnumerable<T> GetAllAssetsOfType<T>() where T : Object
        {
            string[] gUIDs = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            IEnumerable<string> objectsPaths = gUIDs.Select(r => AssetDatabase.GUIDToAssetPath(r));
            return objectsPaths.Select(r => AssetDatabase.LoadAssetAtPath<T>(r));
        }


        public static void EmptyFolderObjects(string folderPath)
        {
            if (!folderPath.EndsWith('/'))
                folderPath += '/';

            if (!AssetDatabase.IsValidFolder(folderPath))
                return; //no folder to empty

            string[] assetsGUIDsInFolder = AssetDatabase.FindAssets("", new string[] { folderPath });
            foreach (string assetGUID in assetsGUIDsInFolder)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

                if (AssetDatabase.IsValidFolder(assetPath))
                    continue; //we dont clear folders or their contents if they're not the given folder

                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.Refresh();
        }

        public static Sprite CreateSpriteInFolder(Sprite sprite, string folderPath)
        {
            d.Assert(folderPath.StartsWith("Assets/"));

            string fullTexturePath = folderPath + sprite.name + ".png";
            string fullSpriteAssetPath = folderPath + sprite.name + ".asset";
            string directoryPath = System.IO.Path.GetDirectoryName(fullTexturePath);

            d.Assert(System.IO.Directory.Exists(directoryPath), $"No valid directory at {directoryPath} ({folderPath})! Cannot save sprite!");

            //Save and reload texture
            Texture2D texture = sprite.texture;
            byte[] textureBytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(fullTexturePath, textureBytes);
            AssetDatabase.Refresh();
            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullTexturePath);
            //

            TextureImporter textureImporter = AssetImporter.GetAtPath(fullTexturePath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.SaveAndReimport();
            AssetDatabase.SaveAssets();

            foreach (Object loadedAsset in AssetDatabase.LoadAllAssetsAtPath(fullTexturePath))
            {
                if (loadedAsset is Sprite)
                    return loadedAsset as Sprite;
            }

            //Sprite savedSprite = Sprite.Create(savedTexture, sprite.rect, new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height));
            //AssetDatabase.CreateAsset(savedSprite, fullSpriteAssetPath);
            //AssetDatabase.SaveAssets();

            d.LogError($"Something went wrong when trying to create sprite {sprite.name} at path {fullTexturePath}");

            return null;//AssetDatabase.LoadAssetAtPath<Sprite>(fullSpriteAssetPath);
        }
#endif
    }
}
