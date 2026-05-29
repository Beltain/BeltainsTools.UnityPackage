using UnityEditor;
using UnityEngine;
using BeltainsTools.DataStructures;

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

            float boolWidth = 18f;
            float spacing = 5f;
            Rect valueRect = new Rect(position.x, position.y, position.width - boolWidth - spacing, position.height);
            Rect boolRect = new Rect(position.x + position.width - boolWidth, position.y, boolWidth, position.height);

            EditorGUI.PropertyField(valueRect, valueProp, label, true);
            flagProp.boolValue = EditorGUI.Toggle(boolRect, flagProp.boolValue);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProp = property.FindPropertyRelative("Value");
            return EditorGUI.GetPropertyHeight(valueProp, label, true);
        }
    }
}
