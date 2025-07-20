using UnityEngine;
using UnityEditor;
using BeltainsTools.UI;
using BeltainsTools;

namespace BeltainsTools.Editor
{
    [CustomEditor(typeof(UILineGraph))]
    public class UILineGraphEditor : UnityEditor.Editor
    {
        const string k_CreateNew_MenuItemPath = "GameObject/" + Globals.k_DisplayName + "/UI/LineGraph";
        const string k_CreateNew_PresetPrefabPath = "...";

        UILineGraph m_Target;

        SerializedProperty prop_GraphLineRenderer;
        SerializedProperty prop_GraphGridRenderer;
        SerializedProperty prop_VerticalAxis;
        SerializedProperty prop_HorizontalAxis;


        [MenuItem(k_CreateNew_MenuItemPath, isValidateFunction: true, priority: 0)]
        public static bool Validate_CreateNew()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.transform.GetComponentInParents<Canvas>() != null;
        }

        [MenuItem(k_CreateNew_MenuItemPath, isValidateFunction: false, priority: 0)]
        public static void CreateNew(MenuCommand menuCommand)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_CreateNew_PresetPrefabPath);
            d.AssertFormat(prefab != null, "Prefab for editor preset not found at path when trying to create through menu item! '{0}'", k_CreateNew_PresetPrefabPath);

            GameObject parent = Selection.activeGameObject;

            if (menuCommand.context as GameObject != null)
                parent = menuCommand.context as GameObject;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (parent != null)
                GameObjectUtility.SetParentAndAlign(instance, parent);
            else
                d.LogWarning("No GameObject selected. Placing in the root");

            Undo.RegisterCreatedObjectUndo(instance, "Create " + prefab.name);

            Selection.activeGameObject = instance;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(prop_GraphLineRenderer);
            EditorGUILayout.PropertyField(prop_GraphGridRenderer);
            EditorGUILayout.PropertyField(prop_VerticalAxis);
            EditorGUILayout.PropertyField(prop_HorizontalAxis);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                m_Target.ReDrawGraphStructure();
            }

        }

        private void OnEnable()
        {
            m_Target = (UILineGraph)target;

            prop_GraphLineRenderer = serializedObject.FindProperty("m_GraphLineRenderer");
            prop_GraphGridRenderer = serializedObject.FindProperty("m_GraphGridRenderer");
            prop_VerticalAxis = serializedObject.FindProperty("m_VerticalAxis");
            prop_HorizontalAxis = serializedObject.FindProperty("m_HorizontalAxis");
        }
    }
}
