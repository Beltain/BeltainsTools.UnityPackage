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
    static Dictionary<string, TelemetryMessage> s_TelemetryMessages = new Dictionary<string, TelemetryMessage>();
    static bool s_TelemetryMessagesUpdatedThisTick = false;

    public static BEvent<IEnumerable<TelemetryMessage>> TelemetryMessagesUpdatedEvent;

    public static IEnumerable<TelemetryMessage> GetTrackedMessages()
    {
        return s_TelemetryMessages.Values;
    }

    public struct TelemetryMessage
    {
        public readonly string ID;
        public readonly string Message;
        public readonly string Group;

        public TelemetryMessage(string id, string message, string group = null)
        {
            ID = id;
            Message = message;
            Group = group;
        }

        public static implicit operator string(TelemetryMessage message)
        {
            return message.Message;
        }
    }

    /// <summary>A disposable handle that wraps a common usage of <see cref="d.SetTelemetryMessage(string, string)"/> and <see cref="d.RemoveTelemetryMessage(string)"/></summary>
    public class TelemetryHandle : System.IDisposable
    {
        private string m_UID;
        private string m_Group;
        private string m_MessageFormat;

        private string m_Message;

        private bool m_IsDisposed;

        public string Message
        {
            get => m_Message;
            protected set
            {
                m_Message = value;
                if (!string.IsNullOrEmpty(m_Message))
                    d.SetTelemetryMessage(m_UID, m_Message, m_Group);
                else
                    d.RemoveTelemetryMessage(m_UID);
            }
        }

        public static TelemetryHandle Create(object owner, string uidOverride = null) => Create(owner.GetType().Name, uidOverride);
        public static TelemetryHandle Create(string group = null, string uidOverride = null) => CreateFormat("{0}", group, uidOverride);
        public static TelemetryHandle CreateFormat(string format, object owner, string uidOverride = null) => CreateFormat(format, owner.GetType().Name, uidOverride);
        public static TelemetryHandle CreateFormat(string format, string group = null, string uidOverride = null) => new TelemetryHandle(format, uidOverride ?? System.Guid.NewGuid().ToString(), group);
        public static TelemetryHandle CreateTitled(string title, object owner, string uidOverride = null) => CreateTitledFormat(title, "{0}", owner.GetType().Name, uidOverride);
        public static TelemetryHandle CreateTitled(string title, string group = null, string uidOverride = null) => CreateTitledFormat(title, "{0}", group, uidOverride);
        public static TelemetryHandle CreateTitledFormat(string title, string valueFormat, object owner, string uidOverride = null) => CreateTitledFormat(title, valueFormat, owner.GetType().Name, uidOverride);
        public static TelemetryHandle CreateTitledFormat(string title, string valueFormat, string group = null, string uidOverride = null) => new TelemetryHandle($"{title}: {valueFormat}", uidOverride ?? System.Guid.NewGuid().ToString(), group);

        protected TelemetryHandle(string format, string uid, string group = null)
        {
            m_MessageFormat = format;
            m_UID = uid;
            m_Group = group;
        }

        ~TelemetryHandle()
        {
            OnDispose(disposing: false);
        }

        public void Set(object value)
        {
            Message = string.Format(m_MessageFormat, value);
        }

        public void Dispose()
        {
            OnDispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void OnDispose(bool disposing)
        {
            if (m_IsDisposed)
                return;
            Clear();
            m_IsDisposed = true;
        }

        public void Clear()
        {
            d.RemoveTelemetryMessage(m_UID);
        }
    }

    static void TickTrackedObjects()
    {
        if (!s_TelemetryMessagesUpdatedThisTick)
            return;
        s_TelemetryMessagesUpdatedThisTick = false;

        TelemetryMessagesUpdatedEvent.Invoke(GetTrackedMessages());
    }

    [System.Obsolete("Use SetTelemetryMessage, or .Set() telemetry objects instead.")]
    public static void Track(string id, string message) => SetTelemetryMessage(id, message);
    public static void SetTelemetryMessage(string id, string message, string group = null)
    {
        if (!DebugActive)
            return;
        s_TelemetryMessages[id] = new TelemetryMessage(id, message, group);
        s_TelemetryMessagesUpdatedThisTick = true;
    }

    [System.Obsolete("Use RemoveTelemetryMessage, or .Clear() telemetry objects instead.")]
    public static void StopTracking(string id) => RemoveTelemetryMessage(id);
    public static void RemoveTelemetryMessage(string id)
    {
        if (!DebugActive)
            return;
        s_TelemetryMessages.Remove(id);
        s_TelemetryMessagesUpdatedThisTick = true;
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