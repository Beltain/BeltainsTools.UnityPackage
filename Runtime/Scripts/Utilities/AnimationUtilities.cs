using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class AnimationUtilities
    {
        public static void StopAndReset(Animation anim, bool sampleFromStart = true)
        {
            foreach (AnimationState animState in anim)
            {
                if (animState.clip == anim.clip)
                {
                    animState.time = sampleFromStart ? 0f : 1f;
                    anim.Play();
                    anim.Sample();
                    break;
                }
            }
            anim.Stop();
        }
    }
}
