using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class AnimationExtensions
    {
        /// <inheritdoc cref="AnimationUtilities.StopAndReset(Animation, bool)"/>
        public static void StopAndReset(this Animation anim, bool sampleFromStart = true)
            => AnimationUtilities.StopAndReset(anim, sampleFromStart);
    }
}
