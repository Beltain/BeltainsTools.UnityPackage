/******************************************************************************
  Copyright (c) 2008-2012 Ryan Juckett
  http://www.ryanjuckett.com/
 
  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.
 
  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:
 
  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
 
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
 
  3. This notice may not be removed or altered from any source
     distribution.
******************************************************************************/

using UnityEngine;

namespace BeltainsTools.Maths
{
    /// <summary>
    /// Damped srpings calculation class adapted from Ryan Juckett's paper on them:
    /// https://www.ryanjuckett.com/damped-springs/
    /// </summary>
    public static class DampedSpringMotion
    {
        /// <summary>
        /// Cached set of motion parameters that can be used to efficiently update
        /// multiple springs using the same time step, angular frequency and damping
        /// ratio.
        /// </summary>
        public struct DampedSpringMotionParams
        {
            // newPos = posPosCoef*oldPos + posVelCoef*oldVel
            public float m_posPosCoef, m_posVelCoef;
            // newVel = velPosCoef*oldPos + velVelCoef*oldVel
            public float m_velPosCoef, m_velVelCoef;
        };

        /// <summary>
        /// This function will compute the parameters needed to simulate a damped spring
        /// over a given period of time.
        /// <para>- An angular frequency is given to control how fast the spring oscillates.</para>
        /// <para>- A damping ratio is given to control how fast the motion decays.</para>
        /// <para>Damping ratio more than 1: over damped.</para>
        /// <para>Damping ratio equal to 1: critically damped.</para>
        /// <para>Damping ratio less than 1: under damped</para>
        /// </summary>
        public static DampedSpringMotionParams CalcDampedSpringMotionParams(
            float deltaTime,                                // time step to advance
            float angularFrequency,                         // angular frequency of motion
            float dampingRatio)                             // damping ratio of motion
        {
            const float epsilon = 0.0001f;
            DampedSpringMotionParams outParams;

            // force values into legal range
            if (dampingRatio < 0.0f) dampingRatio = 0.0f;
            if (angularFrequency < 0.0f) angularFrequency = 0.0f;

            // if there is no angular frequency, the spring will not move and we can
            // return identity
            if (angularFrequency < epsilon)
            {
                outParams.m_posPosCoef = 1.0f; 
                outParams.m_posVelCoef = 0.0f;
                outParams.m_velPosCoef = 0.0f; 
                outParams.m_velVelCoef = 1.0f;
                return outParams;
            }

            if (dampingRatio > 1.0f + epsilon)
            {
                // over-damped
                float za = -angularFrequency * dampingRatio;
                float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1.0f);
                float z1 = za - zb;
                float z2 = za + zb;

                float e1 = Mathf.Exp(z1 * deltaTime);
                float e2 = Mathf.Exp(z2 * deltaTime);

                float invTwoZb = 1.0f / (2.0f * zb); // = 1 / (z2 - z1)

                float e1_Over_TwoZb = e1 * invTwoZb;
                float e2_Over_TwoZb = e2 * invTwoZb;

                float z1e1_Over_TwoZb = z1 * e1_Over_TwoZb;
                float z2e2_Over_TwoZb = z2 * e2_Over_TwoZb;

                outParams.m_posPosCoef = e1_Over_TwoZb * z2 - z2e2_Over_TwoZb + e2;
                outParams.m_posVelCoef = -e1_Over_TwoZb + e2_Over_TwoZb;

                outParams.m_velPosCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2) * z2;
                outParams.m_velVelCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb;
            }
            else if (dampingRatio < 1.0f - epsilon)
            {
                // under-damped
                float omegaZeta = angularFrequency * dampingRatio;
                float alpha = angularFrequency * Mathf.Sqrt(1.0f - dampingRatio * dampingRatio);

                float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
                float cosTerm = Mathf.Cos(alpha * deltaTime);
                float sinTerm = Mathf.Sin(alpha * deltaTime);

                float invAlpha = 1.0f / alpha;

                float expSin = expTerm * sinTerm;
                float expCos = expTerm * cosTerm;
                float expOmegaZetaSin_Over_Alpha = expTerm * omegaZeta * sinTerm * invAlpha;

                outParams.m_posPosCoef = expCos + expOmegaZetaSin_Over_Alpha;
                outParams.m_posVelCoef = expSin * invAlpha;
                          
                outParams.m_velPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSin_Over_Alpha;
                outParams.m_velVelCoef = expCos - expOmegaZetaSin_Over_Alpha;
            }
            else
            {
                // critically damped
                float expTerm = Mathf.Exp(-angularFrequency * deltaTime);
                float timeExp = deltaTime * expTerm;
                float timeExpFreq = timeExp * angularFrequency;

                outParams.m_posPosCoef = timeExpFreq + expTerm;
                outParams.m_posVelCoef = timeExp;

                outParams.m_velPosCoef = -angularFrequency * timeExpFreq;
                outParams.m_velVelCoef = -timeExpFreq + expTerm;
            }

            return outParams;
        }

        /// <summary>
        /// This function will update the supplied position and velocity values over
        /// according to the motion parameters.
        /// </summary>
        public static void UpdateDampedSpringMotion(
	        ref float pPos,                                     // position value to update
	        ref float pVel,                                     // velocity value to update
	        float equilibriumPos,                               // position to approach
            DampedSpringMotionParams motionParams)             // motion parameters to use
        {
            float oldPos = pPos - equilibriumPos; // update in equilibrium relative space
            float oldVel = pVel;

            pPos = (oldPos * motionParams.m_posPosCoef) + (oldVel * motionParams.m_posVelCoef) + equilibriumPos;
            pVel = (oldPos * motionParams.m_velPosCoef) + (oldVel * motionParams.m_velVelCoef);
        }

        /// <summary>Calculate a spring motion development for a given deltaTime</summary>
        public static void CalcDampedSimpleHarmonicMotion(ref float position, ref float velocity,
            float equilibriumPosition, float deltaTime, float angularFrequency, float dampingRatio)
        {
            DampedSpringMotionParams motionParams = CalcDampedSpringMotionParams(deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref position, ref velocity, equilibriumPosition, motionParams);
        }

        /// <summary>Calculate a spring motion development for a given deltaTime</summary>
        public static void CalcDampedSimpleHarmonicMotion(ref Vector2 position, ref Vector2 velocity,
            Vector2 equilibriumPosition, float deltaTime, float angularFrequency, float dampingRatio)
        {
            DampedSpringMotionParams motionParams = CalcDampedSpringMotionParams(deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref position.x, ref velocity.x, equilibriumPosition.x, motionParams);
            UpdateDampedSpringMotion(ref position.y, ref velocity.y, equilibriumPosition.y, motionParams);
        }

        /// <summary>Calculate a spring motion development for a given deltaTime</summary>
        public static void CalcDampedSimpleHarmonicMotion(ref Vector3 position, ref Vector3 velocity,
            Vector3 equilibriumPosition, float deltaTime, float angularFrequency, float dampingRatio)
        {
            DampedSpringMotionParams motionParams = CalcDampedSpringMotionParams(deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref position.x, ref velocity.x, equilibriumPosition.x, motionParams);
            UpdateDampedSpringMotion(ref position.y, ref velocity.y, equilibriumPosition.y, motionParams);
            UpdateDampedSpringMotion(ref position.z, ref velocity.z, equilibriumPosition.z, motionParams);
        }

        /// <summary>Calculate a spring motion development for a given deltaTime</summary>
        public static void CalcDampedSimpleHarmonicMotion(ref Vector4 position, ref Vector4 velocity,
            Vector4 equilibriumPosition, float deltaTime, float angularFrequency, float dampingRatio)
        {
            DampedSpringMotionParams motionParams = CalcDampedSpringMotionParams(deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref position.x, ref velocity.x, equilibriumPosition.x, motionParams);
            UpdateDampedSpringMotion(ref position.y, ref velocity.y, equilibriumPosition.y, motionParams);
            UpdateDampedSpringMotion(ref position.z, ref velocity.z, equilibriumPosition.z, motionParams);
            UpdateDampedSpringMotion(ref position.w, ref velocity.w, equilibriumPosition.w, motionParams);
        }

        /// <summary>Calculate a spring motion development for a given deltaTime</summary>
        public static void CalcDampedSimpleHarmonicMotion(ref Quaternion position, ref Quaternion velocity,
            Quaternion equilibriumPosition, float deltaTime, float angularFrequency, float dampingRatio)
        {
            DampedSpringMotionParams motionParams = CalcDampedSpringMotionParams(deltaTime, angularFrequency, dampingRatio);
            UpdateDampedSpringMotion(ref position.x, ref velocity.x, equilibriumPosition.x, motionParams);
            UpdateDampedSpringMotion(ref position.y, ref velocity.y, equilibriumPosition.y, motionParams);
            UpdateDampedSpringMotion(ref position.z, ref velocity.z, equilibriumPosition.z, motionParams);
            UpdateDampedSpringMotion(ref position.w, ref velocity.w, equilibriumPosition.w, motionParams);
        }
    }
}

