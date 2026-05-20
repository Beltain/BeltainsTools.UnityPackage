using System.Collections;
using UnityEngine;

namespace BeltainsTools.Coroutines
{
    public class TimeDelayedAction
    {
        public static void Execute(System.Action action, float delay, MonoBehaviour executor)
        {
            executor.StartCoroutine(ExecuteActionAfterTime(action, delay));
        }

        static IEnumerator ExecuteActionAfterTime(System.Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action.Invoke();
        }
    }
}
