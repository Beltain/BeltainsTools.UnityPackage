using UnityEngine;

namespace BeltainsTools.EventHandling
{
    /// <inheritdoc cref="EventChannelBase"/>
    [CreateAssetMenu(fileName = "new object EventChannel", menuName = k_Path_CreateAssetMenu + "Channel/Object")]
    public class EventChannel_Object : EventChannel<object> { }
}