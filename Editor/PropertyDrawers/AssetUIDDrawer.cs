using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    [CustomPropertyDrawer(typeof(AssetUID))]
    public class AssetUIDDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty overrideProp = property.FindPropertyRelative("m_Override");

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            string resolvedValue = AssetUID.Resolve(overrideProp.stringValue, property.serializedObject.targetObject);

            bool hasOverride = !string.IsNullOrEmpty(overrideProp.stringValue);
            if (hasOverride)
            {
                float spaceWidth = 5f;
                float halfMaxWidth = (position.width - spaceWidth) * 0.5f;
                Rect textInputPosition = new Rect(position.x, position.y, halfMaxWidth, position.height);

                EditorGUI.BeginChangeCheck();
                string newValue = EditorGUI.TextField(textInputPosition, overrideProp.stringValue);
                if (EditorGUI.EndChangeCheck())
                    overrideProp.stringValue = newValue;

                Rect resolvedTextLabelPosition = new Rect(position.x + halfMaxWidth + spaceWidth, position.y, halfMaxWidth, position.height);
                EditorGUI.SelectableLabel(resolvedTextLabelPosition, resolvedValue);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                Color previousColor = GUI.color;
                GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, 0.5f);
                string newValue = EditorGUI.TextField(position, resolvedValue);
                GUI.color = previousColor;
                if (EditorGUI.EndChangeCheck() && newValue != resolvedValue)
                    overrideProp.stringValue = newValue;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
