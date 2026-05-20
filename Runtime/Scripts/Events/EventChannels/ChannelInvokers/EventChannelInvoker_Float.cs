using UnityEngine;

namespace BeltainsTools.EventHandling
{
    [AddComponentMenu(EventChannelBase.k_Path_CreateAssetMenu + "Float EventChannelInvoker")]
    public class EventChannelInvoker_Float : MonoBehaviour
    {
        [SerializeField]
        protected EventChannel_Float m_Channel;

        public void Invoke(float value)
        {
            m_Channel?.Invoke(value);
        }
    }
}
