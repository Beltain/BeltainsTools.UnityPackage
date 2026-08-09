using UnityEngine;

namespace BeltainsTools.Juice
{
    /// <summary>A <see cref="MonoBehaviour"/> component that tests an objects acceleration and applies squash and stretch scale modifiers to the transform.</summary>
    public class SquasherAndStretcher : MonoBehaviour
    {
        [SerializeField] protected DampedSpring.Vector3 m_ScaleSpring = new DampedSpring.Vector3(4f, 0.5f);
        [SerializeField] protected AnimationCurve m_ScaleCurveX = AnimationCurve.Constant(-1f, 1f, 1f);
        [SerializeField] protected AnimationCurve m_ScaleCurveY = AnimationCurve.Constant(-1f, 1f, 1f);
        [SerializeField] protected AnimationCurve m_ScaleCurveZ = AnimationCurve.Constant(-1f, 1f, 1f);
        [SerializeField] protected Vector3 m_MaxAcceleration = Vector3.one * 5f;

        [SerializeField] protected Affector[] m_Affectors = new Affector[] { new Affector(Vector3.down, 1f, localSpace: false) };

        Vector3 m_LastVelocity = Vector3.zero;
        Vector3 m_LastPosition = Vector3.zero;

        [System.Serializable]
        protected struct Affector
        {
            [SerializeField]
            Vector3 m_AccelerationDirection;
            [SerializeField]
            bool m_InLocalSpace;
            [SerializeField]
            float m_AccelerationScale;
            [SerializeField, Tooltip("What scale should be applied at min (-1) and max (1) acceleration at the provided scale and direction")]
            AnimationCurve m_ScaleApplicationCurve;
            [SerializeField, Tooltip("In which direction should our scale result be applied? If left to 0 it will be the worldspace acceleration direction")]
            Vector3 m_OverrideScaleApplicationDirection;
            [SerializeField]
            bool m_OverrideScaleApplicationInLocalSpace;

            public Affector(Vector3 direction, float accelerationScale, AnimationCurve scaleCurve = default, bool localSpace = false, Vector3 scaleApplicationDir = default, bool scaleApplicationDirAppliedLocally = true)
            {
                m_AccelerationDirection = direction;
                m_AccelerationScale = accelerationScale;
                m_ScaleApplicationCurve = scaleCurve == default ? AnimationCurve.Constant(-1f, 1f, 1f) : scaleCurve;
                m_InLocalSpace = localSpace;
                m_OverrideScaleApplicationDirection = scaleApplicationDir;
                m_OverrideScaleApplicationInLocalSpace = scaleApplicationDirAppliedLocally;
            }

            public void Apply(Vector3 acceleration, Transform transform, float deltaTime, ref Vector3 localScaleResult)
            {
                Vector3 accelerationTestDir = (m_InLocalSpace ? transform.TransformDirection(m_AccelerationDirection) : m_AccelerationDirection).normalized;

                float relativeComponent = Vector3.Dot(acceleration, accelerationTestDir.normalized);
                float normalisedComponent = Mathf.Clamp(deltaTime == 0 ? 0 : (relativeComponent / (m_AccelerationScale * deltaTime)), -1f, 1f);

                Vector3 scaleApplicationDir = m_OverrideScaleApplicationDirection != default ?
                    (m_OverrideScaleApplicationInLocalSpace ? m_OverrideScaleApplicationDirection : transform.InverseTransformDirection(m_OverrideScaleApplicationDirection)) :
                    (m_InLocalSpace ? m_AccelerationDirection : transform.InverseTransformDirection(m_AccelerationDirection)).normalized;
                if (Vector3.Dot(scaleApplicationDir, Vector3.one) < 0f)
                {
                    // our application direction is negative, so flip it for scale to work
                    normalisedComponent *= -1f;
                    scaleApplicationDir *= -1f;
                }
                float scaleComponent = m_ScaleApplicationCurve.Evaluate(normalisedComponent);
                localScaleResult = localScaleResult.SetComponent(scaleApplicationDir, scaleComponent);
            }
        }


        public void NudgeBy(Vector3 nudgeScaleBy)
        {
            SetScale(transform.localScale + nudgeScaleBy);
        }

        public void NudgeTo(Vector3 newScale)
        {
            SetScale(newScale);
        }

        private void SetScale(Vector3 newScale)
        {
            transform.localScale = new Vector3(
                    Mathf.Max(0f, newScale.x),
                    Mathf.Max(0f, newScale.y),
                    Mathf.Max(0f, newScale.z)
                );
            m_ScaleSpring.SetCurrent(transform.localScale);
        }

        private void Awake()
        {
            m_ScaleSpring.Init(transform.localScale);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;

            m_LastVelocity = Vector3.zero;
            m_LastPosition = transform.position;
        }

        private void Update()
        {
            // get current velocity / acceleration
            Vector3 currentVelocity = transform.position - m_LastPosition;
            Vector3 acceleration = currentVelocity - m_LastVelocity;

            // accumulate affector forces
            Vector3 localSpaceResult = Vector3.one;
            for (int i = 0; i < m_Affectors.Length; i++)
                m_Affectors[i].Apply(acceleration, transform, Time.deltaTime, ref localSpaceResult);

            // apply to spring
            m_ScaleSpring.SetTarget(localSpaceResult);
            m_ScaleSpring.Update();
            transform.localScale = m_ScaleSpring;

            // record for next frame
            m_LastVelocity = currentVelocity;
            m_LastPosition = transform.position;
        }
    }
}
