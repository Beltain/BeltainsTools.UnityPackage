using UnityEditor;
using UnityEngine;
using BeltainsTools.DataStructures;
using System.Linq;

namespace BeltainsTools.Editor
{
    [CustomPropertyDrawer(typeof(FlaggedValue<>), true)]
    public class FlaggedValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty flagProp = property.FindPropertyRelative("IsFlagged");
            SerializedProperty valueProp = property.FindPropertyRelative("Value");

            float spacing = 8f;
            float boolWidth = 16f;

            // Look for FlagLabel attribute
            string flagLabel = fieldInfo.GetCustomAttributes(true).Where(r => r is FlagLabelAttribute).Select(r => ((FlagLabelAttribute)r).FlagLabel).FirstOrDefault();
            float flagLabelWidth = 0f;
            if (!string.IsNullOrEmpty(flagLabel))
                flagLabelWidth = EditorStyles.label.CalcSize(new GUIContent(flagLabel)).x + spacing * 0.5f;

            Rect valueRect = new Rect(position.x, position.y, position.width - boolWidth - flagLabelWidth - spacing, position.height);
            Rect flagLabelRect = new Rect(position.x + position.width - boolWidth - flagLabelWidth, position.y, flagLabelWidth, position.height);
            Rect boolRect = new Rect(position.x + position.width - boolWidth, position.y, boolWidth, position.height);

            EditorGUI.PropertyField(valueRect, valueProp, label, true);
            if (!string.IsNullOrEmpty(flagLabel))
                EditorGUI.LabelField(flagLabelRect, flagLabel);
            EditorGUI.PropertyField(boolRect, flagProp, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProp = property.FindPropertyRelative("Value");
            return EditorGUI.GetPropertyHeight(valueProp, label, true);
        }
    }
}
