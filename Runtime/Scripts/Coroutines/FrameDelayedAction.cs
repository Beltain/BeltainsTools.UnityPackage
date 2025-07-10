using System.Collections;
using UnityEngine;

namespace BeltainsTools.Coroutines
{
    public class FrameDelayedAction //Should get this to work without a specified executor (On a beltainsTools monobehaviour maybe) to avoid "can't run coroutine because object is inactive" bullshit
    {
        public static void Execute(System.Action action, MonoBehaviour executor)
        {
            executor.StartCoroutine(ExecuteActionAfterFrame(action));
        }

        static IEnumerator ExecuteActionAfterFrame(System.Action action)
        {
            yield return null;
            action.Invoke();
        }
    }
}
