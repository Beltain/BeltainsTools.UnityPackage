using BeltainsTools.UI;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    [CustomEditor(typeof(UILineRenderer), editorForChildClasses: true)]
    public class UILineRendererEditor : UnityEditor.Editor
    {
        UnityEditor.Editor m_GraphicEditor;

        UILineRenderer m_Target;

        SerializedProperty prop_LineStyle;

        SerializedProperty prop_Points;
        SerializedProperty prop_IsLoop;
        SerializedProperty prop_ArePointsInGraphicSpace;



        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Line Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop_LineStyle);
            EditorGUILayout.PropertyField(prop_Points);
            EditorGUILayout.PropertyField(prop_IsLoop);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prop_ArePointsInGraphicSpace);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                Undo.RecordObject(m_Target, "Convert Points Space");

                if (prop_ArePointsInGraphicSpace.boolValue)
                    m_Target.ConvertPointsToGraphicSpace();
                else
                    m_Target.ConvertPointsFromGraphicSpace();

                serializedObject.Update();
                EditorUtility.SetDirty(m_Target);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Graphic Settings", EditorStyles.boldLabel);

            m_GraphicEditor.OnInspectorGUI();
        }

        private void OnEnable()
        {
            m_Target = target as UILineRenderer;
            if (m_Target == null)
                return;

            m_GraphicEditor = CreateEditor((UnityEngine.UI.Graphic)target, typeof(UnityEditor.UI.GraphicEditor));

            prop_LineStyle = serializedObject.FindProperty("m_LineStyle");

            prop_Points = serializedObject.FindProperty("m_Points");
            prop_IsLoop = serializedObject.FindProperty("m_IsLoop");
            prop_ArePointsInGraphicSpace = serializedObject.FindProperty("m_ArePointsInGraphicSpace");
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
