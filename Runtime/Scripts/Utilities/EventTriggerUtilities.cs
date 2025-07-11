using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeltainsTools.Utilities
{
    public static class EventTriggerUtilities
    {
        public static void AddListener<T>(EventTrigger trigger, EventTriggerType type, System.Action<T> listener) where T : BaseEventData
        {
            UnityEngine.Events.UnityAction<BaseEventData> callback = (data) => listener.Invoke((T)data);

            // Ensure entry
            EventTrigger.Entry entry = trigger.triggers.FirstOrDefault(r => r.eventID == type);
            if (entry == null)
            {
                entry = new EventTrigger.Entry() { eventID = type };
                trigger.triggers.Add(entry);
            }

            // Add listener
            entry.callback.AddListener(callback);
        }

        public static void RemoveListener<T>(EventTrigger trigger, EventTriggerType type, System.Action<T> listener) where T : BaseEventData
        {
            UnityEngine.Events.UnityAction<BaseEventData> callback = (data) => listener.Invoke((T)data);

            // Remove from every existing entry with the corresponding type
            foreach (EventTrigger.Entry triggerEntry in trigger.triggers.Where(r => r.eventID == type && r.callback.GetPersistentEventCount() > 0))
            {
                triggerEntry.callback.RemoveListener(callback);
            }
        }
    }
}
