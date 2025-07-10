using UnityEngine;

namespace BeltainsTools.Juice
{
    /// <summary>Get a non-random offset from a center point that appears to 'wobble' when incremented</summary>
    public class PositionWobbler
    {
        //When moving this character to their follow position, we add a little 'wobble' to make for more natural following motion.
        const int k_NumPreWobbleCircles = 3; //how many circles we use to calculate the wobble offset
        const float k_WobbleMagnitude = 0.25f; //How wobbly are we?

        float[] m_PreWobbleAngles = null; //we get some random angles which act as a seed for our wobble calculation
        float[] m_PreWobbleAngleRotationSpeeds = null; //we get some random periods which determine the speed of the wobble on the different circles

        public PositionWobbler()
        {
            m_PreWobbleAngles = new float[k_NumPreWobbleCircles];
            m_PreWobbleAngleRotationSpeeds = new float[k_NumPreWobbleCircles];

            for (int i = 0; i < k_NumPreWobbleCircles; i++)
            {
                m_PreWobbleAngles[i] = Random.Range(0f, 360f);
                m_PreWobbleAngleRotationSpeeds[i] = 360f /*Full rot*/ / /*Period*/ Random.Range(1.25f, 2.5f);
            }
        }

        public Vector3 GetOffset()
        {
            Vector2 offsetSum = Vector2.zero;
            for (int i = 0; i < m_PreWobbleAngles.Length; i++)
            {
                Vector2 wobbleCircleCenterOffset = BeltainsTools.Maths.Circles.GetAngledPointOnUnitCircleD(((float)i / m_PreWobbleAngles.Length) * 360f) * k_WobbleMagnitude;
                offsetSum += wobbleCircleCenterOffset + BeltainsTools.Maths.Circles.GetAngledPointOnUnitCircleD(m_PreWobbleAngles[i]) * k_WobbleMagnitude;
            }
            return (offsetSum / m_PreWobbleAngles.Length).ToVector3XZ();
        }

        public void Increment()
        {
            for (int i = 0; i < k_NumPreWobbleCircles; i++) //Increment wobble
            {
                m_PreWobbleAngles[i] += m_PreWobbleAngleRotationSpeeds[i] * Time.deltaTime;
                m_PreWobbleAngles[i] %= 360f;
            }
        }
    }
}
