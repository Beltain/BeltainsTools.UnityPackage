using System;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    [CustomPropertyDrawer(typeof(EnumArrayAttribute))]
    public class EnumArrayDrawer : PropertyDrawer
    {
        Type m_CachedType;
        string[] m_EnumNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent content)
        {
            EnumArrayAttribute enumAttribute = (EnumArrayAttribute)attribute;

            if (enumAttribute != null && enumAttribute.m_EnumType.IsEnum)
            {
                if (IsArray(property))
                {
                    if (enumAttribute.m_EnumType != m_CachedType)
                    {
                        m_CachedType = enumAttribute.m_EnumType;
                        string[] names = Enum.GetNames(enumAttribute.m_EnumType);
                        m_EnumNames = new string[names.Length];
                        for (int i = 0; i < names.Length; i++)
                        {
                            m_EnumNames[i] = ObjectNames.NicifyVariableName(names[i]);
                        }
                    }

                    if (m_EnumNames.Length > 0)
                    {
                        int startIndex = enumAttribute.m_FirstValue != int.MinValue ? enumAttribute.m_FirstValue : 0;
                        int arrLength = Mathf.Min(enumAttribute.m_LastValueExclusive - startIndex, (m_EnumNames.Length - startIndex));
                        ResizeArray(property, arrLength);

                        int nameIndex = startIndex + GetElementIndex(property);
                        string name = m_EnumNames[nameIndex];

                        EditorGUI.PropertyField(position, property, new GUIContent(name), true);

                        return;
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public bool IsArray(SerializedProperty property)
        {
            return property.propertyPath.Contains(".Array");
        }

        void ResizeArray(SerializedProperty property, int length)
        {
            string path = property.propertyPath;
            int index = path.LastIndexOf(".Array", StringComparison.Ordinal);
            string arrayPath = path.Substring(0, index);

            SerializedObject serializedObject = property.serializedObject;
            SerializedProperty arrayProperty = serializedObject.FindProperty(arrayPath);

            if (arrayProperty.arraySize != length)
            {
                arrayProperty.arraySize = length;
                serializedObject.ApplyModifiedProperties();
            }
        }

        int GetElementIndex(SerializedProperty property)
        {
            string path = property.propertyPath;
            int start = path.LastIndexOf("[", StringComparison.Ordinal) + 1;
            int end = path.LastIndexOf("]", StringComparison.Ordinal);

            string indexString = path.Substring(start, end - start);

            return int.Parse(indexString);
        }
    }
}