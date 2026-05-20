using UnityEngine;

namespace BeltainsTools.EventHandling
{
    [AddComponentMenu(EventChannelBase.k_Path_CreateAssetMenu + "Bool EventChannelInvoker")]
    public class EventChannelInvoker_Bool : MonoBehaviour
    {
        [SerializeField]
        protected EventChannel_Bool m_Channel;

        public void Invoke(bool value)
        {
            m_Channel?.Invoke(value);
        }
    }
}
