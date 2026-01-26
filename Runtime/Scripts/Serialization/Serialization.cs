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

    /// <inheritdoc cref="IDataSaver{T}"/>
    public interface IDataSaver : IDataSaver<SaveData> { } // basic bitch save data saver
    /// <summary>Why does this sound like an o2 plan? Anyways it's meant to be the interface for any object that can return and receive save data</summary>
    public interface IDataSaver<T> where T : SaveData // handles save data of a specific type
    {
        /// <summary>Output relavent save data for this object. Return a success/fail</summary>
        public bool Serialize(out T data);
        /// <summary>Set object from save data. Return a success/fail</summary>
        public bool Deserialize(in T data);
    }

    public static partial class FileServices
    {
        /// <summary>Service responsible for writing/reading strings to/from a location</summary>
        public abstract class FileService
        {
            public static readonly string s_PersistentPath = Application.persistentDataPath;

            public FileService() { }


            [System.Obsolete("Please use WritePersistentData instead")]
            public bool WriteData(in string data, string subDirectory, string fileName, bool encrypt) => WritePersistentFile(data, Path.Combine(subDirectory, fileName), encrypt);
            /// <inheritdoc cref="WriteFile(in string, string, bool)"/>
            public bool WritePersistentFile(in string data, string subPath, bool encrypt) => WriteFile(data, Path.Combine(s_PersistentPath, subPath), encrypt);
            /// <summary>Attempt to write serialized data, with an optional encryption pass beforehand</summary>
            public abstract bool WriteFile(in string data, string filePath, bool encrypt);


            [System.Obsolete("Please use ReadPersistentData instead")]
            public bool ReadData(out string data, string subDirectory, string fileName, bool encrypted) => ReadPersistentFile(out data, Path.Combine(subDirectory, fileName), encrypted);
            /// <inheritdoc cref="ReadFile(out string, string, bool)"/>
            public bool ReadPersistentFile(out string data, string subPath, bool encrypted) => ReadFile(out data, Path.Combine(s_PersistentPath, subPath), encrypted);
            /// <summary>Attempt to read serialized data, specifying whether or not it is encrypted data</summary>
            public abstract bool ReadFile(out string data, string filePath, bool encrypted);

            /// <inheritdoc cref="ReadFileHash(string)"/>
            public string ReadPersistentFileHash(string subPath) => ReadFileHash(Path.Combine(s_PersistentPath, subPath));
            /// <summary>Get the hash for the file at the provided location, use to perform diff checks</summary>
            public abstract string ReadFileHash(string filePath);


            /// <inheritdoc cref="DeleteFile(string)"/>
            public bool DeletePersistentFile(string subPath) => DeleteFile(Path.Combine(s_PersistentPath, subPath));
            /// <summary>Delete the file at the given path</summary>
            public abstract bool DeleteFile(string filePath);


            /// <inheritdoc cref="GetFiles(string, string)"/>
            public string[] GetPersistentFiles(string subPath = null, string extension = null) 
                => subPath.IsNullOrEmpty() ? GetFiles(s_PersistentPath) : GetFiles(Path.Combine(s_PersistentPath, subPath));
            /// <summary>Get all files in the provided <paramref name="directory"/>, and optionally with the specified <paramref name="extension"/></summary>
            public abstract string[] GetFiles(string directory, string extension = null);
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
            public bool Deserialize<T>(in string dataString, out T deserializedObject) where T : new()
            {
                bool success = Deserialize(dataString, typeof(T), out object obj);
                deserializedObject = success ? (T)obj : default(T);
                return success;
            }
            /// <summary>Attempt to deseralize an object from a data string</summary>
            public abstract bool Deserialize(in string dataString, Type type, out object deserializedObject);
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
