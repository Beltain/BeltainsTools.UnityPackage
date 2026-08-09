using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace BeltainsTools.Editor
{
    [CustomEditor(typeof(AssetRegisterObject), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class AssetRegisterObjectEditor : UnityEditor.Editor
    {
        SerializedProperty prop_RebuildPath;


        [DebugAction("BeltainsTools.RebuildAssetRegisters")]
        public static void RebuildAllAssetRegisters()
        {
            RebuildTargets(Resources.FindObjectsOfTypeAll<AssetRegisterObject>());
        }

        private static void RebuildTargets(IEnumerable<AssetRegisterObject> targets)
        {
            Undo.RecordObjects(targets.Cast<Object>().ToArray(), "Asset Registers Rebuild");
            foreach (AssetRegisterObject target in targets)
                RebuildTarget(target);
            d.Log($"Rebuilt ({targets.Count()}) asset registers:\n{string.Join(",\n", targets.Select(t => t.name))}");
        }

        private static void RebuildTarget(AssetRegisterObject target)
        {
            try
            {
                System.Type targetAssetType = target.GetAssetType();
                IEnumerable<object> targetAssets = Utils.GetAssetsOfTypeInProjectPaths(targetAssetType, target.m_RebuildPath);
                target.SetAssets(targetAssets);
                EditorUtility.SetDirty(target);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to rebuild assets for {target.name}: {e.Message}");
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            GUILayout.BeginHorizontal();
            bool rebuildRequested = GUILayout.Button(
                    new GUIContent
                    {
                        text = "Rebuild from path:",
                        tooltip =
                            "Assign a project path from which we will search for assets of this type and add them to the register." +
                            "\nNOTE: THIS WILL CLEAR THE EXISTING REGISTER!"
                    },
                    GUILayout.Width(inspectorWidth * 0.25f)
                );
            prop_RebuildPath.stringValue = GUILayout.TextField(prop_RebuildPath.stringValue, GUILayout.Width(inspectorWidth * 0.70f), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            if (rebuildRequested)
                RebuildTargets(targets.Select(r => r as AssetRegisterObject));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            prop_RebuildPath = serializedObject.FindProperty("m_RebuildPath");
        }
    }
}
