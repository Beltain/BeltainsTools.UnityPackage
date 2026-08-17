using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    /// <remarks>
    /// NB: This drawer assumes our SerialisableGuid is composed of four uint parts named Part1, Part2, Part3, and Part4.<br/>
    /// Should make less hard-coded in future
    /// </remarks>
    [CustomPropertyDrawer(typeof(SerialisableGuid))]
    public class SerialisableGuidDrawer : PropertyDrawer
    {
        static readonly string[] GuidParts = { "Part1", "Part2", "Part3", "Part4" };

        static GUIStyle s_CenteredGreyLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize =  EditorStyles.label.fontSize,
        };

        static SerializedProperty[] GetGuidParts(SerializedProperty property)
        {
            SerializedProperty[] values = new SerializedProperty[GuidParts.Length];
            for (int i = 0; i < GuidParts.Length; i++)
                values[i] = property.FindPropertyRelative(GuidParts[i]);
            return values;
        }

        static void CopyGuid(SerializedProperty property)
        {
            if (GetGuidParts(property).Any(x => x == null)) return;

            string guid = BuildGuidString(GetGuidParts(property));
            EditorGUIUtility.systemCopyBuffer = guid;
            d.Log($"GUID copied to clipboard: {guid}");
        }

        static bool DoesClipboardContainGuid()
        {
            string clipboard = EditorGUIUtility.systemCopyBuffer;
            return clipboard.Length == 32 && clipboard.All(c => Uri.IsHexDigit(c));
        }

        static void PasteGuid(SerializedProperty property, bool skipWarnings)
        {
            if (!DoesClipboardContainGuid())
            {
                d.LogError("Clipboard does not contain a valid GUID!");
                return;
            }

            if (IsGuidInitialized(property) && !skipWarnings &&
                !EditorUtility.DisplayDialog("Paste GUID", "Are you sure you want to overwrite the existing GUID?", "Yes", "No"))
                return;

            string clipboard = EditorGUIUtility.systemCopyBuffer;
            SerializedProperty[] guidParts = GetGuidParts(property);
            for (int i = 0; i < GuidParts.Length; i++)
                guidParts[i].uintValue = Convert.ToUInt32(clipboard.Substring(i * 8, 8), 16);
            property.serializedObject.ApplyModifiedProperties();
            d.Log($"GUID pasted from clipboard: {clipboard}");
        }

        static void ResetGuid(SerializedProperty property, bool skipWarnings)
        {
            if (IsGuidInitialized(property) && !skipWarnings &&
                !EditorUtility.DisplayDialog("Reset GUID", "Are you sure you want to reset the GUID?", "Yes", "No"))
                return;

            foreach (var part in GetGuidParts(property))
                part.uintValue = 0;

            property.serializedObject.ApplyModifiedProperties();
            d.Log("GUID has been reset.");
        }

        static void RegenerateGuid(SerializedProperty property, bool skipWarnings)
        {
            if (IsGuidInitialized(property) && !skipWarnings &&
                !EditorUtility.DisplayDialog("Regenerate GUID", "Are you sure you want to regenerate the GUID?", "Yes", "No"))
                return;

            byte[] bytes = Guid.NewGuid().ToByteArray();
            SerializedProperty[] guidParts = GetGuidParts(property);

            for (int i = 0; i < GuidParts.Length; i++)
                guidParts[i].uintValue = BitConverter.ToUInt32(bytes, i * 4);

            property.serializedObject.ApplyModifiedProperties();
            d.Log("GUID has been regenerated.");
        }

        static string BuildGuidString(SerializedProperty[] guidParts)
        {
            return new StringBuilder()
                .AppendFormat("{0:X8}", guidParts[0].uintValue)
                .AppendFormat("{0:X8}", guidParts[1].uintValue)
                .AppendFormat("{0:X8}", guidParts[2].uintValue)
                .AppendFormat("{0:X8}", guidParts[3].uintValue)
                .ToString();
        }

        static bool IsGuidInitialized(SerializedProperty property) => IsGuidInitialized(GetGuidParts(property));
        static bool IsGuidInitialized(SerializedProperty[] guidParts)
        {
            return guidParts.All(part => part != null && part.uintValue != 0);
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect fullPosition = position;
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if (IsGuidInitialized(property))
            {
                EditorGUI.LabelField(position, BuildGuidString(GetGuidParts(property)), s_CenteredGreyLabel);
            }
            else
            {
                GUIContent initialiseButtonContent = new GUIContent("Initialize GUID");
                float initialiseButtonWidth = EditorStyles.miniButton.CalcSize(initialiseButtonContent).x;
                Rect labelPosition = new Rect(position.x, position.y, position.width - initialiseButtonWidth, position.height);
                EditorGUI.LabelField(labelPosition, "NOT INITIALISED", s_CenteredGreyLabel);
                Rect buttonPosition = new Rect(position.x + position.width - initialiseButtonWidth, position.y, initialiseButtonWidth, position.height);
                if (GUI.Button(buttonPosition, initialiseButtonContent, EditorStyles.miniButtonRight))
                    RegenerateGuid(property, false);
            }
            bool hasClicked = 
                Event.current.type == EventType.MouseUp && 
                Event.current.button == 1;
            if (hasClicked && fullPosition.Contains(Event.current.mousePosition))
            {
                ShowContextMenu(property, enableForceMode: Event.current.shift);
                Event.current.Use();
            }

            EditorGUI.EndProperty();
        }

        void ShowContextMenu(SerializedProperty property, bool enableForceMode)
        {
            GenericMenu menu = new GenericMenu();

            void _AddItemToMenu(string title, SerializedProperty property, Action action, bool canBeForced = false, System.Func<bool> validator = null)
            {
                GUIContent titleContent = new GUIContent(title + (canBeForced && enableForceMode ? " (force)" : ""));

                if (validator == null || validator())
                    menu.AddItem(titleContent, false, () => action());
                else
                    menu.AddDisabledItem(titleContent);
            }

            _AddItemToMenu("Paste", property, () => PasteGuid(property, enableForceMode), canBeForced: true, validator: DoesClipboardContainGuid);
            _AddItemToMenu("Copy", property, () => CopyGuid(property));
            _AddItemToMenu("Reset", property, () => ResetGuid(property, enableForceMode), canBeForced: true);
            _AddItemToMenu("Regenerate", property, () => RegenerateGuid(property, enableForceMode), canBeForced: true);
            menu.ShowAsContext();
        }
    }
}
