using System;
using UnityEngine;

namespace BeltainsTools
{
    /// <summary>
    /// Displays a permanent help box in the Unity Inspector.
    /// Can be applied to <see cref="UnityEngine.Object"/> or fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class NoteAttribute : PropertyAttribute
    {
        public string Message { get; }
        public MessageTypes MessageType { get; }
        public int Order { get; set; }

#if UNITY_EDITOR
        public UnityEditor.MessageType EditorMessageType => MessageType switch
        {
            NoteAttribute.MessageTypes.Normal => UnityEditor.MessageType.Info,
            NoteAttribute.MessageTypes.Warning => UnityEditor.MessageType.Warning,
            NoteAttribute.MessageTypes.Severe => UnityEditor.MessageType.Error,
            _ => UnityEditor.MessageType.None
        };
#endif


        public enum MessageTypes
        {
            None,
            Normal,
            Warning,
            Severe
        }

        public NoteAttribute(string message, MessageTypes messageType = MessageTypes.Normal)
        {
            Message = message;
            MessageType = messageType;
            Order = 0;
        }
    }
}