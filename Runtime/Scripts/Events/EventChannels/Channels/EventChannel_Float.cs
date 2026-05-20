using UnityEngine;

namespace BeltainsTools.EventHandling
{
    /// <inheritdoc cref="EventChannelBase"/>
    [CreateAssetMenu(fileName = "new float EventChannel", menuName = k_Path_CreateAssetMenu + "Channel/Float")]
    public class EventChannel_Float : EventChannel<float> { }
}
