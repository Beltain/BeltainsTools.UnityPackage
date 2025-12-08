using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace BeltainsTools.Serialization
{
    /// <summary>Base class for all save data </summary>
    public class SaveData { }

    /// <summary>Why does this sound like an o2 plan? Anyways it's meant to be the interface for any object that can return and receive save data</summary>
    public interface IDataSaver
    {
        /// <summary>Output relavent save data for this object. Return a success/fail</summary>
        public bool Serialize(out SaveData data);
        /// <summary>Set object from save data. Return a success/fail</summary>
        public bool Deserialize(in SaveData data);
    }

    public static partial class FileServices
    {
        /// <summary>Service responsible for writing/reading strings to/from a location</summary>
        public abstract class FileService
        {
            public FileService() { }

            /// <summary>Attempt to write serialized data, with an optional encryption pass beforehand</summary>
            public abstract bool WriteData(in string data, string subDirectory, string fileName, bool encrypt);
            /// <summary>Attempt to read serialized data, specifying whether or not it is encrypted data</summary>
            public abstract bool ReadData(out string data, string subDirectory, string fileName, bool encrypted);
        }
    }

    public static partial class DataServices
    {
        /// <summary>Service responsible for converting save objects into string data and back into objects from string data</summary>
        public abstract class DataService
        {
            public DataService() { }

            public abstract bool TryGetVersion(in string dataString, out int dataVersion);

            /// <summary>Attempt to serialize an object into a data string</summary>
            public abstract bool Serialize<T>(in T objectToSerialize, out string dataString) where T : new();
            /// <summary>Attempt to deseralize an object from a data string</summary>
            public abstract bool Deserialize<T>(in string dataString, out T deserializedObject) where T : new();
        }
    }

    static class StringCompressor //thank you https://stackoverflow.com/users/1315444/fubo on https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp
    {
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            byte[] compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            byte[] gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                byte[] buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
