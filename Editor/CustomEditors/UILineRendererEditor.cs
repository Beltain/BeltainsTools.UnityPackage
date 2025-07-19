using BeltainsTools.UI;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    [CustomEditor(typeof(UILineRenderer))]
    public class UILineRendererEditor : UnityEditor.Editor
    {
        UILineRenderer m_Target;

        SerializedProperty prop_Thickness;
        SerializedProperty prop_CornerDetail;

        SerializedProperty prop_Points;
        SerializedProperty prop_IsLoop;
        SerializedProperty prop_ArePointsInGraphicSpace;



        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Line Style", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop_Thickness);
            EditorGUILayout.PropertyField(prop_CornerDetail);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Line Data", EditorStyles.boldLabel);
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
            base.OnInspectorGUI();
        }

        private void OnEnable()
        {
            m_Target = target as UILineRenderer;

            prop_Thickness = serializedObject.FindProperty("m_Thickness");
            prop_CornerDetail = serializedObject.FindProperty("m_CornerDetail");

            prop_Points = serializedObject.FindProperty("m_Points");
            prop_IsLoop = serializedObject.FindProperty("m_IsLoop");
            prop_ArePointsInGraphicSpace = serializedObject.FindProperty("m_ArePointsInGraphicSpace");
        }
    }
}
