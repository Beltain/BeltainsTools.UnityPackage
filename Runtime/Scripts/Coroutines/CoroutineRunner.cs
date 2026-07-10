using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BeltainsTools.Coroutines
{
    public class CoroutineRunner : MonoBehaviour
    {
        private const string k_GOInstanceName = "_CoroutineRunner";
        private const float k_CleanupTimeout = 120f;

        private static CoroutineRunner s_Instance;

        private HashSet<RunHandle> m_RunningHandles = new HashSet<RunHandle>();
        private IEnumerator m_CleanupTimeoutCo = null;



        public static RunHandle Run(Task asyncTask, System.Action<bool> onCompleteCallback = null)
        {
            IEnumerator RunAsyncActionCo(Task async)
            {
                yield return new WaitForTask(async);
            }

            return Run(RunAsyncActionCo(asyncTask), onCompleteCallback);
        }

        public static RunHandle Run(IEnumerator coroutine, System.Action<bool> onCompleteCallback = null)
        {
            EnsureInstance();
            return s_Instance.Run_Internal(coroutine, onCompleteCallback);
        }

        private static void EnsureInstance()
        {
            if (s_Instance == null)
            {
                s_Instance = new GameObject(k_GOInstanceName, typeof(CoroutineRunner)).GetComponent<CoroutineRunner>();
                DontDestroyOnLoad(s_Instance.gameObject);
            }
        }

        private RunHandle Run_Internal(IEnumerator coroutine, System.Action<bool> onCompleteCallback = null)
        {
            StopCleanupTimeout(); // new coroutine is starting, stop the cleanup timeout if it was running

            RunHandle handle = new RunHandle(coroutine, this, (RunHandle completedRunHandle, bool isComplete) => 
            { 
                onCompleteCallback?.Invoke(isComplete);
                OnHandleStopped(completedRunHandle, isComplete);
            });
            handle.Start();
            m_RunningHandles.Add(handle);
            return handle;
        }

        private void OnHandleStopped(RunHandle handle, bool isComplete)
        {
            m_RunningHandles.Remove(handle);

            if (m_RunningHandles.Count == 0)
                StartCleanupTimeout();
        }


        private void StartCleanupTimeout()
        {
            if (m_CleanupTimeoutCo != null)
                return;

            m_CleanupTimeoutCo = CleanupAfterTimeout();
            StartCoroutine(m_CleanupTimeoutCo);
        }

        private void StopCleanupTimeout()
        {
            if (m_CleanupTimeoutCo == null)
                return;
            StopCoroutine(m_CleanupTimeoutCo);
        }

        private IEnumerator CleanupAfterTimeout()
        {
            yield return new WaitForSeconds(k_CleanupTimeout);
            CleanupInstance();
        }

        private void CleanupInstance()
        {
            if (s_Instance != null)
            {
                Destroy(s_Instance.gameObject);
                s_Instance = null;
            }
        }
    }


    public class RunHandle : System.IDisposable
    {
        private readonly IEnumerator m_Co = null;
        private readonly MonoBehaviour m_Runner = null;
        private readonly System.Action<RunHandle, bool> m_OnStoppedCallback;

        private IEnumerator m_RunnerCo = null;

        private bool IsRunning => m_RunnerCo != null;

        /// <param name="onStoppedCallback">outputs true if completed, false if stopped before completion</param>
        public RunHandle(IEnumerator coroutine, MonoBehaviour runner, System.Action<RunHandle, bool> onStoppedCallback)
        {
            m_Co = coroutine;
            m_Runner = runner;
            m_OnStoppedCallback = onStoppedCallback;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            m_RunnerCo = RunCoroutineCo();
            m_Runner.StartCoroutine(m_RunnerCo);
        }

        private IEnumerator RunCoroutineCo()
        {
            yield return m_Co;
            Stop(isComplete: true);
        }

        public void Stop(bool isComplete)
        {
            if (!IsRunning)
                return;

            m_Runner.StopCoroutine(m_RunnerCo);
            m_RunnerCo = null;

            m_OnStoppedCallback?.Invoke(this, isComplete);
        }

        public void Dispose()
        {
            Stop(isComplete: false);
        }
    }
}
