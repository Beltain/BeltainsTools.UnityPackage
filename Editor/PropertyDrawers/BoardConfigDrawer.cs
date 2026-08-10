using UnityEngine;
using UnityEditor;
using BeltainsTools.Board;

[CustomPropertyDrawer(typeof(Board.Config))]
public class BoardConfigDrawer : PropertyDrawer
{
    public static Vector2Int DelayedVector2IntField(Rect position, GUIContent label, Vector2Int value)
    {
        Rect fieldRect = EditorGUI.PrefixLabel(position, label);

        float spacing = 4f;
        float labelW = 14f;
        float fieldW = (fieldRect.width - spacing - labelW * 2) / 2f;

        float x = fieldRect.x;

        // X field
        EditorGUI.LabelField(new Rect(x, fieldRect.y, labelW, fieldRect.height), "W");
        x += labelW + 1;
        int newX = EditorGUI.DelayedIntField(new Rect(x, fieldRect.y, fieldW, fieldRect.height), value.x);
        x += fieldW + spacing;

        // Y field
        EditorGUI.LabelField(new Rect(x, fieldRect.y, labelW, fieldRect.height), "H");
        x += labelW - 1;
        int newY = EditorGUI.DelayedIntField(new Rect(x, fieldRect.y, fieldW, fieldRect.height), value.y);

        return new Vector2Int(newX, newY);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Board.Config target = (Board.Config)fieldInfo.GetValue(property.serializedObject.targetObject);

        EditorGUI.BeginProperty(position, label, property);

        // Draw the foldout
        Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        SerializedProperty sizeProp = property.FindPropertyRelative("Size");
        Rect sizeValueRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        position.y += sizeValueRect.height + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.BeginChangeCheck();
        Vector2Int inputSize = DelayedVector2IntField(sizeValueRect, GUIContent.none, sizeProp.vector2IntValue);
        if (EditorGUI.EndChangeCheck())
            target.SetSize(inputSize);

        // Draw size value inline with foldout when collapsed
        if (property.isExpanded)
        {
            // Draw a field that displays a 2D array of toggles for the Layout field
            SerializedProperty layoutProp = property.FindPropertyRelative("Layout");

            Vector2Int size = sizeProp.vector2IntValue;
            float toggleSize = EditorGUIUtility.singleLineHeight;

            Rect layoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position.y += layoutRect.height * size.y + EditorGUIUtility.standardVerticalSpacing;
            Rect layoutValueRect = EditorGUI.PrefixLabel(layoutRect, new GUIContent("Layout"));

            EditorGUI.BeginChangeCheck();
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int index = y * size.x + x;
                    if (index < layoutProp.arraySize)
                    {
                        Rect toggleRect = new Rect(layoutValueRect.x + x * toggleSize, layoutValueRect.y + (size.y - 1 - y) * toggleSize, toggleSize, toggleSize);
                        SerializedProperty elementProp = layoutProp.GetArrayElementAtIndex(index);
                        elementProp.boolValue = EditorGUI.Toggle(toggleRect, elementProp.boolValue);
                    }
                }
            }

            Rect quickActionsRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            quickActionsRect = EditorGUI.PrefixLabel(quickActionsRect, new GUIContent(" "));
            position.y += quickActionsRect.height + EditorGUIUtility.standardVerticalSpacing;
            Rect clearAllButtonRect = new Rect(quickActionsRect.x, quickActionsRect.y, quickActionsRect.width * 0.5f, quickActionsRect.height);
            if (GUI.Button(clearAllButtonRect, new GUIContent("Clear All ❌"), EditorStyles.miniButton))
            {
                for (int y = 0; y < size.y; y++)
                    for (int x = 0; x < size.x; x++)
                        layoutProp.GetArrayElementAtIndex(y * size.x + x).boolValue = false;
            }
            Rect fillAllButtonRect = new Rect(quickActionsRect.x + quickActionsRect.width * 0.5f, quickActionsRect.y, quickActionsRect.width * 0.5f, quickActionsRect.height);
            if (GUI.Button(fillAllButtonRect, new GUIContent("Fill All \U0001faa3"), EditorStyles.miniButton))
            {
                for (int y = 0; y < size.y; y++)
                    for (int x = 0; x < size.x; x++)
                        layoutProp.GetArrayElementAtIndex(y * size.x + x).boolValue = true;
            }

            EditorGUI.EndChangeCheck();
            property.serializedObject.ApplyModifiedProperties();
        }

        property.serializedObject.ApplyModifiedProperties();
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;
        SerializedProperty sizeProp = property.FindPropertyRelative("Size");
        Vector2Int size = sizeProp.vector2IntValue;
        float toggleSize = EditorGUIUtility.singleLineHeight;
        float startY = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        float actionButtonsHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        return startY + (size.y) * toggleSize + actionButtonsHeight;
    }
}