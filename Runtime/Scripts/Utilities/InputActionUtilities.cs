using UnityEngine;
using UnityEngine.InputSystem;

namespace BeltainsTools.Utilities
{
    public static class InputActionUtilities
    {
        /// <inheritdoc cref="BeltainsTools.Utilities.InputActionUtilities.RereferenceInputAction(ref InputActionReference, InputActionAsset)"/>
        public static void RereferenceInputAction(ref InputActionProperty actionProp, InputActionAsset inputActionsAsset)
        {
            if (actionProp.reference == null || actionProp.reference.action == null)
                return;

            InputActionReference reference = ScriptableObject.CreateInstance<UnityEngine.InputSystem.InputActionReference>();
            InputAction action = inputActionsAsset.FindAction(actionProp.reference.action.name);
            reference.Set(action);
            actionProp = new InputActionProperty(reference);
        }

        /// <summary>Set the action reference of this property to be the matching instance from the input actions asset.</summary>
        /// <remarks>This is so that we can control the referenced action and ensure it's enabled/disabled according to the actions asset</remarks>
        public static void RereferenceInputAction(ref InputActionReference actionRef, InputActionAsset inputActionsAsset)
        {
            if (actionRef == null || actionRef.action == null)
                return;

            actionRef = ScriptableObject.CreateInstance<UnityEngine.InputSystem.InputActionReference>();
            actionRef.Set(inputActionsAsset.FindAction(actionRef.action.name));
        }
    }
}
