using UnityEngine;

namespace BeltainsTools.EventHandling
{
    [AddComponentMenu(EventChannelBase.k_Path_CreateAssetMenu + "Int EventChannelInvoker")]
    public class EventChannelInvoker_Int : MonoBehaviour
    {
        [SerializeField]
        protected EventChannel_Int m_Channel;

        public void Invoke(int value)
        {
            m_Channel?.Invoke(value);
        }
    }
}
