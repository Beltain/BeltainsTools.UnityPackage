using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    [CustomPropertyDrawer(typeof(NoteAttribute))]
    public class NoteAttributeDecoratorDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            NoteAttribute noteAttribute = (NoteAttribute)attribute;
            return EditorStyles.helpBox.CalcHeight(new GUIContent(noteAttribute.Message), EditorGUIUtility.currentViewWidth);
        }

        public override void OnGUI(Rect position)
        {
            // Just draw a classic Unity Help Box for now
            NoteAttribute noteAttribute = (NoteAttribute)attribute;
            EditorGUI.HelpBox(position, noteAttribute.Message, noteAttribute.EditorMessageType);
        }
    }
}
