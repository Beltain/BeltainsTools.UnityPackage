using UnityEngine;

namespace BeltainsTools.EventHandling
{
    /// <inheritdoc cref="EventChannelBase"/>
    [CreateAssetMenu(fileName = "new string EventChannel", menuName = k_Path_CreateAssetMenu + "Channel/String")]
    public class EventChannel_String : EventChannel<string> { }
}