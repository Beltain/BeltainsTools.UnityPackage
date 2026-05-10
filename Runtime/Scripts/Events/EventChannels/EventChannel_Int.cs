using UnityEngine;

namespace BeltainsTools.EventHandling
{
    /// <inheritdoc cref="EventChannelBase"/>
    [CreateAssetMenu(fileName = "new int EventChannel", menuName = k_Path_CreateAssetMenu + "Channel/Int")]
    public class EventChannel_Int : EventChannel<int> { }
}