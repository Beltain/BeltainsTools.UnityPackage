using UnityEngine;

namespace BeltainsTools.EventHandling
{
    [AddComponentMenu(EventChannelBase.k_Path_CreateAssetMenu + "String EventChannelInvoker")]
    public class EventChannelInvoker_String : MonoBehaviour
    {
        [SerializeField]
        protected EventChannel_String m_Channel;

        public void Invoke(string value)
        {
            m_Channel?.Invoke(value);
        }
    }
}
