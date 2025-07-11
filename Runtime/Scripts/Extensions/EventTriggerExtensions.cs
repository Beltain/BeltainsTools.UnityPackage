using UnityEngine;
using BeltainsTools.Utilities;
using UnityEngine.EventSystems;

namespace BeltainsTools
{
    public static class EventTriggerExtensions
    {
        /// <inheritdoc cref="EventTriggerUtilities.AddListener{T}(EventTrigger, EventTriggerType, System.Action{T})"/>
        public static void AddListener<T>(this EventTrigger trigger, EventTriggerType type, System.Action<T> listener) where T : BaseEventData
            => EventTriggerUtilities.AddListener<T>(trigger, type, listener);

        /// <inheritdoc cref="EventTriggerUtilities.RemoveListener{T}(EventTrigger, EventTriggerType, System.Action{T})"/>
        public static void RemoveListener<T>(this EventTrigger trigger, EventTriggerType type, System.Action<T> listener) where T : BaseEventData
            => EventTriggerUtilities.RemoveListener<T>(trigger, type, listener);
    }
}
