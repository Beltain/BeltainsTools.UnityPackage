using BeltainsTools.UI;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    [CustomEditor(typeof(UIGridRenderer), editorForChildClasses: true)]
    public class UIGridRendererEditor : UnityEditor.Editor
    {
        UnityEditor.Editor m_GraphicEditor;

        UIGridRenderer m_Target;

        SerializedProperty prop_ColumnCount;
        SerializedProperty prop_RowCount;
        SerializedProperty prop_LineThickness;
        SerializedProperty prop_DrawBorderLines;



        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop_ColumnCount);
            EditorGUILayout.PropertyField(prop_RowCount);
            EditorGUILayout.PropertyField(prop_LineThickness);
            EditorGUILayout.PropertyField(prop_DrawBorderLines);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Graphic Settings", EditorStyles.boldLabel);

            m_GraphicEditor.OnInspectorGUI();
        }

        private void OnEnable()
        {
            m_Target = target as UIGridRenderer;
            if (m_Target == null)
                return;

            m_GraphicEditor = CreateEditor((UnityEngine.UI.Graphic)target, typeof(UnityEditor.UI.GraphicEditor));

            prop_ColumnCount = serializedObject.FindProperty("m_ColumnCount");
            prop_RowCount = serializedObject.FindProperty("m_RowCount");
            prop_LineThickness = serializedObject.FindProperty("m_LineThickness");
            prop_DrawBorderLines = serializedObject.FindProperty("m_DrawBorderLines");
        }

        private void OnDisable()
        {
            if (m_GraphicEditor != null)
            {
                DestroyImmediate(m_GraphicEditor);
                m_GraphicEditor = null;
            }
        }
    }
}
