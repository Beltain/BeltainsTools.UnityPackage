using BeltainsTools.Juice;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    // Replace with your actual derived class type.
    [CustomPropertyDrawer(typeof(DampedSpring.Base), useForChildren: true)]
    public class SpringBasePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin property and draw label
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            // Store original indent level and set to 0
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Locate child properties
            SerializedProperty frequencyProp = property.FindPropertyRelative("m_Frequency");
            SerializedProperty dampingProp = property.FindPropertyRelative("m_Damping01");

            float spacing = 4f;
            float labelWidth = 35f;
            float fieldsWidth = (position.width - spacing * 3 - labelWidth * 2);

            Rect freqLabelRect = new Rect(position.x, position.y, labelWidth, position.height);
            Rect freqFieldRect = new Rect(freqLabelRect.xMax + spacing, position.y, fieldsWidth * 0.3f, position.height);

            Rect dampLabelRect = new Rect(freqFieldRect.xMax + spacing, position.y, labelWidth, position.height);
            Rect dampFieldRect = new Rect(dampLabelRect.xMax + spacing, position.y, fieldsWidth * 0.7f, position.height);

            EditorGUI.LabelField(freqLabelRect, "Freq");
            EditorGUI.PropertyField(freqFieldRect, frequencyProp, GUIContent.none);

            EditorGUI.LabelField(dampLabelRect, "Damp");
            EditorGUI.Slider(dampFieldRect, dampingProp, 0f, 1f, GUIContent.none);

            // Restore indent
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
