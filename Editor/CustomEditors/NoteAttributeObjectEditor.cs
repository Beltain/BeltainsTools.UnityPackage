using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    [CustomEditor(typeof(UnityEngine.Object), true)]
    [CanEditMultipleObjects]
    public class NoteAttributeObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawClassHelpBoxes(target.GetType());
            base.OnInspectorGUI();
        }

        protected void DrawClassHelpBoxes(Type type)
        {
            IEnumerable<NoteAttribute> attributes = type.GetCustomAttributes(typeof(NoteAttribute), true)
                .Cast<NoteAttribute>()
                .OrderBy(a => a.Order);

            foreach (NoteAttribute attr in attributes)
                EditorGUILayout.HelpBox(attr.Message, attr.EditorMessageType);
        }
    }
}
