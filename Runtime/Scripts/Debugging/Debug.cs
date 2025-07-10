using BeltainsTools.EventHandling;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>Debugging wrapper class for ease of use with, extending, and giving us more control over default debug behaviours</summary>
public static class d
{
    static ulong s_DebugFrameCount = 0;

    static bool DebugActive = Application.isEditor || Debug.isDebugBuild;
        

    /// <summary>Run the update method for the debug system once</summary>
    public static void Tick()
    {
        if (!DebugActive)
            return;

        //Do last tick requested stuff
        TickTrackedObjects();

        //Increment Frame
        s_DebugFrameCount++;
    }




    #region Tracking_____Tracking_____Tracking_____Tracking_____Tracking_____Tracking_____Tracking_____Tracking
    static Dictionary<string, TrackedObjectData> s_TrackedObjects = new Dictionary<string, TrackedObjectData>();
    static bool s_TrackedObjectsUpdatedThisTick = false;

    public static BEvent<IEnumerable<string>> TrackedObjectsUpdatedEvent;

    public static IEnumerable<string> GetTrackedMessages()
    {
        return s_TrackedObjects.Values.Select(r => r.Message).Reverse();
    }

    class TrackedObjectData
    {
        public string Message { get; private set; } = "";

        public void UpdateMessage(string message)
        {
            Message = message;
        }
    }


    static void TickTrackedObjects()
    {
        if (!s_TrackedObjectsUpdatedThisTick)
            return;
        s_TrackedObjectsUpdatedThisTick = false;

        TrackedObjectsUpdatedEvent.Invoke(GetTrackedMessages());
    }


    /// <summary>No reason to use this over <see cref="Track(string, object)"/>. Keeping for posterity...</summary>
    [System.Obsolete]
    public static void TrackFormat(string id, string messageFormat, params object[] args) => Track(id, string.Format(messageFormat, args));
    public static void Track(string id, object message)
    {
        if (!DebugActive)
            return;

        if (!s_TrackedObjects.ContainsKey(id))
            s_TrackedObjects.Add(id, new TrackedObjectData());

        s_TrackedObjects[id].UpdateMessage((string)message);
        s_TrackedObjectsUpdatedThisTick = true;
    }

    public static void StopTracking(string id)
    {
        if (!DebugActive)
            return;

        if (!s_TrackedObjects.ContainsKey(id))
            return;
        s_TrackedObjects.Remove(id);
        s_TrackedObjectsUpdatedThisTick = true;
    }
    #endregion ________________________________________________________________________________________________


    #region Unity.Debug Overrides_____Unity.Debug Overrides_____Unity.Debug Overrides_____Unity.Debug Overrides
    #region Log_____Log_____Log_____Log_____Log_____Log_____Log_____Log_____Log_____Log
    public static void Log(object message)
    {
        if (!DebugActive)
            return;
        Debug.Log(message);
    }

    public static void Log(object message, Object context)
    {
        if (!DebugActive)
            return;
        Debug.Log(message, context);
    }

    public static void LogFormat(string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogFormat(format, args);
    }

    public static void LogFormat(Object context, string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogFormat(context, format, args);
    }

    public static void LogFormat(LogType logType, LogOption logOptions, Object context, string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogFormat(logType, logOptions, context, format, args);
    }
    #endregion ________________________________________________________________________

    #region LogError_____LogError_____LogError_____LogError_____LogError_____LogError
    public static void LogError(object message)
    {
        if (!DebugActive)
            return;
        Debug.LogError(message);
    }

    public static void LogError(object message, Object context)
    {
        if (!DebugActive)
            return;
        Debug.LogError(message, context);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogErrorFormat(format, args);
    }

    public static void LogErrorFormat(Object context, string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogErrorFormat(context, format, args);
    }
    #endregion ______________________________________________________________________

    #region LogException_____LogException_____LogException_____LogException_____LogException
    public static void LogException(System.Exception exception)
    {
        Debug.LogException(exception);
    }

    public static void LogException(System.Exception exception, Object context)
    {
        Debug.LogException(exception, context);
    }
    #endregion _____________________________________________________________________________

    #region LogWarning_____LogWarning_____LogWarning_____LogWarning_____LogWarning
    public static void LogWarning(object message)
    {
        if (!DebugActive)
            return;
        Debug.LogWarning(message);
    }

    public static void LogWarning(object message, Object context)
    {
        if (!DebugActive)
            return;
        Debug.LogWarning(message, context);
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogWarningFormat(format, args);
    }

    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogWarningFormat(context, format, args);
    }
    #endregion ___________________________________________________________________

    #region Assertions_____Assertions_____Assertions_____Assertions_____Assertions
    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition)
    {
        if (!DebugActive)
            return;
        if (!condition)
        {
            Debug.LogAssertion("Assertion failed");
        }
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, Object context)
    {
        if (!DebugActive)
            return;
        if (!condition)
        {
            Debug.LogAssertion("Assertion failed", context);
        }
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, object message)
    {
        if (!DebugActive)
            return;
        if (!condition)
        {
            Debug.LogAssertion(message);
        }
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, string message)
    {
        if (!DebugActive)
            return;
        if (!condition)
        {
            Debug.LogAssertion(message);
        }
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, object message, Object context)
    {
        if (!DebugActive)
            return;
        if (!condition)
        {
            Debug.LogAssertion(message, context);
        }
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, string message, Object context)
    {
        if (!DebugActive)
            return;
        if (!condition)
        {
            Debug.LogAssertion(message, context);
        }
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void AssertFormat(bool condition, string format, params object[] args)
    {
        if (!DebugActive)
            return;
        if (!condition)
        {
            Debug.LogAssertion(string.Format(format, args));
        }
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void AssertFormat(bool condition, Object context, string format, params object[] args)
    {
        if (!DebugActive)
            return;
        if (!condition)
        {
            Debug.LogAssertion(string.Format(format, args), context);
        }
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertion(object message)
    {
        if (!DebugActive)
            return;
        Debug.LogAssertion(message);
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertion(object message, Object context)
    {
        if (!DebugActive)
            return;
        Debug.LogAssertion(message, context);
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertionFormat(string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogAssertionFormat(format, args);
    }

    [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertionFormat(Object context, string format, params object[] args)
    {
        if (!DebugActive)
            return;
        Debug.LogAssertionFormat(context, format, args);
    }
    #endregion ___________________________________________________________________
    #endregion ________________________________________________________________________________________________
}