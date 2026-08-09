using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BeltainsTools.Editor
{
    [CustomPropertyDrawer(typeof(NoteAttribute))]
    public class NoteAttributeDecoratorDrawer : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            NoteAttribute noteAttribute = (NoteAttribute)attribute;
            return new HelpBox(noteAttribute.Message, noteAttribute.EditorUIElementsMessageType);
        }
    }
}
