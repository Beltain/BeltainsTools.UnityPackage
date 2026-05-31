using System.Collections;
using UnityEngine;

namespace BeltainsTools.Coroutines
{
    public class ConditionDelayedAction
    {
        /// <inheritdoc cref="Execute(System.Action, System.Func{bool}, MonoBehaviour, float)"/>
        public static void Execute(System.Action action, System.Func<bool> condition, float timeout = 0f)
        {
            CoroutineRunner.Run(ExecuteActionAfterCondition(action, condition, timeout));
        }

        /// <summary>Executes the given action once the specified condition is met. Optionally, a timeout can be set to stop waiting and abort after a certain duration.</summary>
        public static void Execute(System.Action action, System.Func<bool> condition, MonoBehaviour executor, float timeout = 0f)
        {
            executor.StartCoroutine(ExecuteActionAfterCondition(action, condition, timeout));
        }

        static IEnumerator ExecuteActionAfterCondition(System.Action action, System.Func<bool> condition, float timeout = 0f)
        {
            float startTime = Time.time;
            while (!condition())
            {
                if (timeout > 0f && Time.time - startTime >= timeout)
                    yield break; // stop waiting if we've timed out
                yield return null;
            }
            action.Invoke();
        }
    }
}
