using UnityEngine;

namespace BeltainsTools.EventHandling
{
    [AddComponentMenu(EventChannelBase.k_Path_CreateAssetMenu + "Object EventChannelInvoker")]
    public class EventChannelInvoker_Object : MonoBehaviour
    {
        [SerializeField]
        protected EventChannel_Object m_Channel;

        public void Invoke(object value)
        {
            m_Channel?.Invoke(value);
        }
    }
}
