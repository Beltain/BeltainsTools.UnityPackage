using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class AnimationCurveUtilities
    {
        public static AnimationCurve EaseIn(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            return new AnimationCurve(
                new Keyframe(timeStart, valueStart, 0f, 0f),
                new Keyframe(timeEnd, valueEnd, 2f, 2f)
            );
        }

        public static AnimationCurve EaseOut(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            return new AnimationCurve(
                new Keyframe(timeStart, valueStart, 2f, 2f),
                new Keyframe(timeEnd, valueEnd, 0f, 0f)
            );
        }
    }
}
