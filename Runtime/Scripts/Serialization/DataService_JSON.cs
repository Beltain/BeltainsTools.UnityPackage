using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace BeltainsTools.Serialization
{
    public static partial class DataServices
    {
        public class DataService_JSON : DataService
        {
            public const string k_DataVersioningVariableName = "_Version_";

            static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = HandleDeserializationError,
                Formatting = Formatting.None
            };


            public override bool TryGetVersion(in string dataString, out int dataVersion)
            {
                dataVersion = -1;
                try
                {
                    var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataString, SerializerSettings);
                    if (jsonObject != null && jsonObject.ContainsKey(k_DataVersioningVariableName))
                    {
                        dataVersion = System.Convert.ToInt32(jsonObject[k_DataVersioningVariableName]);
                        return true;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error while trying to get data version: {e}");
                }
                return false;
            }

            public override bool Deserialize(in string dataString, Type type, out object deserializedObject)
            {
                deserializedObject = JsonConvert.DeserializeObject(dataString, type, SerializerSettings);
                return true; //Can't find a way to check whether the process failed or not (not using try catch), just returning true
            }

            public override bool Serialize<T>(in T objectToSerialize, out string dataString)
            {
                dataString = JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented, SerializerSettings);
                return true; //Can't find a way to check whether the process failed or not (not using try catch), just returning true
            }

            private static void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
            {
                // Log or handle the error here
                string error = $"Error during JSON deserialization:\n\n<b>PATH</b>\n{errorArgs.ErrorContext.Path}\n\n<b>STACK TRACE</b>\n{errorArgs.ErrorContext.Error.StackTrace}\n\n<b>FULL ERROR</b>\n{errorArgs.ErrorContext.Error}";
                Debug.LogError(error);
                errorArgs.ErrorContext.Handled = true; 
            }
        }
    }
}
