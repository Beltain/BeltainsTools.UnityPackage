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
            deserializedObject = JsonConvert.DeserializeObject(dataString, type, SerializerSettings);
            return true; //Can't find a way to check whether the process failed or not (not using try catch), just returning true
        }

        public override bool OnSerialize<T>(in T objectToSerialize, out string dataString)
        {
            dataString = JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented, SerializerSettings);
            return true; //Can't find a way to check whether the process failed or not (not using try catch), just returning true
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
