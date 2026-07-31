using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor.Debugging.DebugActions
{
    [DebugPages.DebugPage("Debug Actions", 10)]
    public class DebugActionsPage : DebugPages.DebugPage
    {
        List<DebugActionsMethod> m_ActionMethods;

        public override void OnEnable()
        {
            m_ActionMethods = DebugActions.DiscoverDebugActionMethods();
        }

        public override void OnGUI()
        {
            if (m_ActionMethods.Count == 0)
            {
                EditorGUILayout.HelpBox($"No debug actions found. Please assign at least one method with the {nameof(DebugActionAttribute)}.", MessageType.Warning);
                return;
            }

            foreach (DebugActionsMethod method in m_ActionMethods)
                method.OnGUI();
        }
    }
}
