using UnityEngine;

namespace BeltainsTools.Juice
{
    [RequireComponent(typeof(SpringyTransform))]
    public partial class SpringyTargetFollower : MonoBehaviour
    {
        [SerializeReference, SubclassSelector]
        private TargetStrategies.TargetStrategyBase m_Target = new TargetStrategies.TargetWorldPoint();
        [SerializeReference, SubclassSelector]
        private FollowStrategies.FollowStrategyBase m_Follow = new FollowStrategies.FollowSimple();

        private SpringyTransform m_SpringyTransform;

        public TargetStrategies.TargetStrategyBase Target => m_Target;
        public FollowStrategies.FollowStrategyBase Follow => m_Follow;


        public void SetTargetStrategy<T>(T target) where T : TargetStrategies.TargetStrategyBase
        {
            m_Target = target;
        }

        public void SetFollowStrategy<T>(T followStrat) where T : FollowStrategies.FollowStrategyBase
        {
            m_Follow = followStrat;
        }

        private void Awake()
        {
            m_SpringyTransform = GetComponent<SpringyTransform>();
        }

        private void Update()
        {
            if (m_Target != null && m_Follow != null)
                m_Follow.Update(m_SpringyTransform, m_Target);
        }
    }
}
