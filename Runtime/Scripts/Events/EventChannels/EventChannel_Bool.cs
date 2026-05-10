using UnityEngine;

namespace BeltainsTools.EventHandling
{
    /// <inheritdoc cref="EventChannelBase"/>
    [CreateAssetMenu(fileName = "new bool EventChannel", menuName = k_Path_CreateAssetMenu + "Channel/Bool")]
    public class EventChannel_Bool : EventChannel<bool> { }
}