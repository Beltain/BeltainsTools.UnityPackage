using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BeltainsTools.Editor
{
#if UNITY_EDITOR
    public class Bandles
    {
        public static bool ClickableCubeHandle(int controlID, Vector3 position, Quaternion rotation, float size, Color colorSelected)
        {
            int id = GUIUtility.GetControlID(controlID, FocusType.Passive);
            bool result = false;

            switch (Event.current.GetTypeForControl(id))
            {
                case EventType.MouseUp:
                    if (HandleUtility.nearestControl == id && Event.current.button == 0)
                    {
                        Event.current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);
                        result = true;
                    }
                    break;

                case EventType.Repaint:
                    Color currentColour = Handles.color;
                    Handles.color = id == HandleUtility.nearestControl ?
                        colorSelected :
                        Handles.color;

                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                    Handles.CubeHandleCap(id, position, rotation, size, EventType.Repaint);

                    Handles.color = currentColour;
                    break;

                case EventType.Layout:
                    HandleUtility.AddControl(id, HandleUtility.DistanceToCube(position, rotation, size));
                    break;
            }

            return result;
        }
    }
#endif
}