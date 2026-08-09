using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    // NB:
    // CURRENTLY NOT SUPPORTED, CAN'T GET IT TO WORK WITHOUT BREAKING EXISTING CUSTOM INSPECTORS OR THROWING ERRORS ABOUT INITIALISATION OR MISSING INTERNAL ATTRIBUTES

    //public static class NoteAttributeEditorInjector
    //{
    //    [InitializeOnLoadMethod]
    //    private static void Initialize()
    //    {
    //        UnityEditor.Editor.finishedDefaultHeaderGUI -= OnPostHeaderGUI;
    //        UnityEditor.Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
    //    }

    //    private static void OnPostHeaderGUI(UnityEditor.Editor editor)
    //    {
    //        if (editor.target == null)
    //            return;

    //        IEnumerable<NoteAttribute> attributes = editor.target.GetType()
    //            .GetCustomAttributes(typeof(NoteAttribute), true)
    //            .Cast<NoteAttribute>()
    //            .OrderBy(a => a.Order);

    //        foreach (NoteAttribute attr in attributes)
    //            EditorGUILayout.HelpBox(attr.Message, attr.EditorMessageType);
    //    }
    //}
}
