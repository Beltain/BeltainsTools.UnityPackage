using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace BeltainsTools.Serialization
{
    public class DataService_JSON : DataService
    {
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = HandleDeserializationError,
            Formatting = Formatting.None
        };

        public override bool OnDeserialize(in string dataString, Type type, out object deserializedObject)
        {
            deserializedObject = null;
            try
            {
                object result = JsonConvert.DeserializeObject(dataString, type, SerializerSettings);
                if (result == null || !type.IsInstanceOfType(result))
                    return false; // pretty much never gets here, but just in case, we check if the result is null or not of the expected type (ie. type is an int32, saved object was a string, etc)

                deserializedObject = result;
                return true;
            }
            catch (JsonException e)
            {
                d.LogError($"[DataService_JSON][OnDeserialize] JSON deserialization error: {e}");
                return false;
            }
        }

        public override bool OnSerialize<T>(in T objectToSerialize, out string dataString)
        {
            try
            {
                dataString = JsonConvert.SerializeObject(objectToSerialize, SerializerSettings);
                return true;
            }
            catch (JsonException e)
            {
                d.LogError($"[DataService_JSON][OnSerialize] JSON serialization error: {e}");
                dataString = null;
                return false;
            }
        }

        private static void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            // Log or handle the error here
            string error = $"Error during JSON deserialization:\n\n<b>PATH</b>\n{errorArgs.ErrorContext.Path}\n\n<b>STACK TRACE</b>\n{errorArgs.ErrorContext.Error.StackTrace}\n\n<b>FULL ERROR</b>\n{errorArgs.ErrorContext.Error}";
            d.LogError(error);
            errorArgs.ErrorContext.Handled = true; 
        }
    }
}
