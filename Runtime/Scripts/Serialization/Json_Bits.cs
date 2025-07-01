using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Serialization.JsonBits
{
    /// <summary>Crazy that unity doesn't provide one for this, but anyways here's a Json converter for the common <see cref="Vector3"/> struct</summary>
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            float x = jsonObject.GetValue("x").Value<float>();
            float y = jsonObject.GetValue("y").Value<float>();
            float z = jsonObject.GetValue("z").Value<float>();
            return new Vector3(x, y, z);
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject
            {
                { "x", value.x },
                { "y", value.y },
                { "z", value.z }
            };
            jsonObject.WriteTo(writer);
        }
    }

    /// <summary>Crazy that unity doesn't provide one for this, but anyways here's a Json converter for the common <see cref="Vector2"/> struct</summary>
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            float x = jsonObject.GetValue("x").Value<float>();
            float y = jsonObject.GetValue("y").Value<float>();
            return new Vector2(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject
            {
                { "x", value.x },
                { "y", value.y }
            };
            jsonObject.WriteTo(writer);
        }
    }

    /// <summary>Crazy that unity doesn't provide one for this, but anyways here's a Json converter for the common <see cref="Vector3Int"/> struct</summary>
    public class Vector3IntConverter : JsonConverter<Vector3Int>
    {
        public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            int x = jsonObject.GetValue("x").Value<int>();
            int y = jsonObject.GetValue("y").Value<int>();
            int z = jsonObject.GetValue("z").Value<int>();
            return new Vector3Int(x, y, z);
        }

        public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject
            {
                { "x", value.x },
                { "y", value.y },
                { "z", value.z }
            };
            jsonObject.WriteTo(writer);
        }
    }

    /// <summary>Crazy that unity doesn't provide one for this, but anyways here's a Json converter for the common <see cref="Vector2Int"/> struct</summary>
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            int x = jsonObject.GetValue("x").Value<int>();
            int y = jsonObject.GetValue("y").Value<int>();
            return new Vector2Int(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject
            {
                { "x", value.x },
                { "y", value.y }
            };
            jsonObject.WriteTo(writer);
        }
    }


    /// <summary>Crazy that unity doesn't provide one for this, but anyways here's a Json converter for the common <see cref="Quaternion"/> struct</summary>
    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            float x = jsonObject.GetValue("x").Value<float>();
            float y = jsonObject.GetValue("y").Value<float>();
            float z = jsonObject.GetValue("z").Value<float>();
            float w = jsonObject.GetValue("w").Value<float>();
            return new Quaternion(x, y, z, w);
        }

        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject
            {
                { "x", value.x },
                { "y", value.y },
                { "z", value.z },
                { "w", value.w }
            };
            jsonObject.WriteTo(writer);
        }
    }
}