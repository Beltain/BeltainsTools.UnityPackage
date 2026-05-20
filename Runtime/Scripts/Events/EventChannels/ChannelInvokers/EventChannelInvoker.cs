using UnityEngine;

namespace BeltainsTools.EventHandling
{
    [AddComponentMenu(EventChannelBase.k_Path_CreateAssetMenu + "EventChannelInvoker", order: -1)]
    public class EventChannelInvoker : MonoBehaviour
    {
        [SerializeField]
        protected EventChannel m_Channel;

        public void Invoke()
        {
            m_Channel?.Invoke();
        }
    }
}
