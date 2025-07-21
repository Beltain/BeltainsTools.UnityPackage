using UnityEngine;
using UnityEditor;
using BeltainsTools.UI;
using BeltainsTools;

namespace BeltainsTools.Editor
{
    [CustomEditor(typeof(UILineGraph))]
    public class UILineGraphEditor : UnityEditor.Editor
    {
        UILineGraph m_Target;

        SerializedProperty prop_GraphLineRenderer;
        SerializedProperty prop_GraphGridRenderer;
        SerializedProperty prop_VerticalAxis;
        SerializedProperty prop_HorizontalAxis;
        SerializedProperty prop_AxesEncapsulateData;
        SerializedProperty prop_DataPoints;

        [MenuItem("GameObject / " + Globals.k_PrettyName + " / UI / LineGraph", priority = 1)]
        public static void CreateNew(MenuCommand menuCommand)
        {
            Utils.TryCreateBeltainsToolsPresetFromPrefabPath(menuCommand, keepPrefabReference: false, "Editor/Prefabs/UI/ComponentPresets/UILineGraph.prefab");
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
                m_Target.RedrawGraphStructure();
                EditorUtility.SetDirty(m_Target);
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(prop_AxesEncapsulateData);
            EditorGUILayout.PropertyField(prop_DataPoints);
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                m_Target.RedrawGraphData();
                EditorUtility.SetDirty(m_Target);
            }
        }

        private void OnEnable()
        {
            m_Target = (UILineGraph)target;

            prop_GraphLineRenderer = serializedObject.FindProperty("m_GraphLineRenderer");
            prop_GraphGridRenderer = serializedObject.FindProperty("m_GraphGridRenderer");
            prop_VerticalAxis = serializedObject.FindProperty("m_VerticalAxis");
            prop_HorizontalAxis = serializedObject.FindProperty("m_HorizontalAxis");
            prop_AxesEncapsulateData = serializedObject.FindProperty("m_AxesEncapsulateData");
            prop_DataPoints = serializedObject.FindProperty("m_DataPoints");
        }
    }
}
