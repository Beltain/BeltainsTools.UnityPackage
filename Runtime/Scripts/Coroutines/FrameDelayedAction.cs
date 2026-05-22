using System.Collections;
using UnityEngine;

namespace BeltainsTools.Coroutines
{
    public class FrameDelayedAction
    {
        public static void Execute(System.Action action)
        {
            CoroutineRunner.Run(ExecuteActionAfterFrame(action));
        }

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
