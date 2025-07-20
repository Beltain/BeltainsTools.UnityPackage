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
        const string k_CreateNew_PresetPrefabPath = "Packages/com.beltainjordaan.beltainstools/Editor/Prefabs/UI/ComponentPresets/UILineGraph.prefab";
        const string k_CreateNew_PresetPrefabPathAlt = "Assets/Plugins/BeltainsTools/Editor/Prefabs/UI/ComponentPresets/UILineGraph.prefab";

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
            Utils.CreatePresetFromPrefabPath(menuCommand, k_CreateNew_PresetPrefabPath, k_CreateNew_PresetPrefabPathAlt);
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
