using System.Threading.Tasks;
using UnityEngine;

namespace BeltainsTools.Coroutines
{
    public class WaitForTask : CustomYieldInstruction
    {
        private readonly Task m_Task;

        public WaitForTask(Task task)
        {
            m_Task = task;
        }

        public override bool keepWaiting => !m_Task.IsCompleted;
    }
}
