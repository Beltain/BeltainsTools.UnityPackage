using UnityEngine;

namespace BeltainsTools.Juice
{
    public class Wiggler : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)]
        private float m_StartingIntensity = 0f;
        [SerializeField, Tooltip("The angle of rotation at 100% intensity")] 
        private float m_MaxRotation = 40f;
        [SerializeField, Tooltip("The rate of rotation at 100% intensity")] 
        private float m_MaxOscillationRate = 1f;
        [SerializeField]
        private DampedSpring.Float m_RotationSpring = new DampedSpring.Float(15f, 0.65f, 0f);

        private float m_Intensity = 0f;

        public void SetIntensity(float intensity, bool instantly = false)
        {
            m_Intensity = intensity;

            if (instantly)
                m_RotationSpring.Set(m_Intensity);
        }

        private void Awake()
        {
            m_RotationSpring.Init(m_StartingIntensity, r => transform.localRotation = Quaternion.AngleAxis(r, Vector3.forward));
            SetIntensity(m_StartingIntensity, instantly: true);
        }

        private void Update()
        {
            float oscillationMaxAngle = m_MaxRotation * m_Intensity;
            float oscillationRate = m_MaxOscillationRate * m_Intensity;

            if (oscillationMaxAngle == 0f || oscillationRate == 0f)
            {
                m_RotationSpring.SetTarget(0f);
            }
            else
            {
                float oscillation = Mathf.Sin(Time.time * oscillationRate) * oscillationMaxAngle;
                m_RotationSpring.SetTarget(oscillation);
            }

            m_RotationSpring.Update();
        }
    }
}
