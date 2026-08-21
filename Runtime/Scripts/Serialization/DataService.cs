using System;
using System.Text;

namespace BeltainsTools.Serialization
{
    /// <summary>Service responsible for converting save objects into string data and back into objects from string data</summary>
    public abstract class DataService
    {
        protected const uint k_MagicNumber = 0xDA7ADA7A; // Shared magic number for data validation

        private readonly bool CompressionEnabled;

        /// <summary>Create a <see cref="DataService"/> that handles de/serialisation of objects into string data. With an option to enable compression</summary>
        /// <param name="compressionEnabled">Should the data be compressed during serialization</param>
        public DataService(bool compressionEnabled = true)
        {
            CompressionEnabled = compressionEnabled;
        }

        public bool TryGetVersion(in string dataString, out int dataVersion)
        {
            dataVersion = 0;
            try
            {
                ReadHeader(dataString, out dataVersion, out _);
                return true;
            }
            catch (System.Exception e)
            {
                d.LogError($"[DataService][GetVersion] Error while trying to get data version: {e}");
                return false;
            }
        }

        /// <inheritdoc cref="OnSerialize{T}(in T, out string)"/>
        /// <remarks>Override <paramref name="dataVersion"/> to specify the version of the object being serialized</remarks>
        public bool Serialize<T>(in T objectToSerialize, out string dataString, int dataVersion = 0) where T : new()
        {
            // serialize the object into a data string
            d.Log("[DataService][Serialize] Attempting to serialize object into data string...");
            bool success = OnSerialize(objectToSerialize, out dataString);
            if (success)
                d.Log("[DataService][Serialize] Object serialized successfully.");
            else
                d.LogError("[DataService][Serialize] Object serialization failed!");

            // compress object data if required
            if (success && CompressionEnabled)
                dataString = StringCompressor.CompressString(dataString);

            // create header with magic number and version
            byte[] header = new byte[sizeof(uint) + sizeof(int)];
            BitConverter.GetBytes(k_MagicNumber).CopyTo(header, 0);
            BitConverter.GetBytes(dataVersion).CopyTo(header, sizeof(uint));

            // stamp the header onto the data string
            byte[] stampedData = new byte[header.Length + dataString.Length];
            header.CopyTo(stampedData, 0);
            Encoding.UTF8.GetBytes(dataString).CopyTo(stampedData, header.Length);
            dataString = Encoding.UTF8.GetString(stampedData);

            return success;
        }

        /// <summary>Attempt to serialize an object into a data string</summary>
        public abstract bool OnSerialize<T>(in T objectToSerialize, out string dataString) where T : new();

        /// <inheritdoc cref="OnDeserialize(in string, System.Type, out object)"/>
        public bool Deserialize<T>(in string dataString, out T deserializedObject, out int dataVersion) where T : new()
        {
            bool success = Deserialize(dataString, typeof(T), out object obj, out dataVersion);
            deserializedObject = success ? (T)obj : default(T);
            return success;
        }

        /// <inheritdoc cref="OnDeserialize(in string, System.Type, out object)"/>
        /// <remarks>Check <paramref name="dataVersion"/> to get the version of the object being deserialized</remarks>
        public bool Deserialize(in string dataString, Type type, out object deserializedObject, out int dataVersion)
        {
            string data = dataString;
            try
            {
                d.Log("[DataService][Deserialize] Reading header from data string...");
                ReadHeader(dataString, out dataVersion, out data);
                d.Log($"[DataService][Deserialize] Header read successfully. Data version: {dataVersion}");
            }
            catch (System.Exception e)
            {
                d.LogError($"[DataService][Deserialize] Error while trying to read header: {e}");
                deserializedObject = null;
                dataVersion = 0;
                return false;
            }

            if (CompressionEnabled)
            {
                // try decompress the string,
                // if it fails (previously uncompressed perhaps),
                // we just use the original string
                d.Log("[DataService][Deserialize] Attempting to decompress data string...");
                if (StringCompressor.TryDecompressString(data, out string dataDecompressed))
                {
                    d.Log("[DataService][Deserialize] Data string decompressed successfully.");
                    data = dataDecompressed;
                }
                else
                {
                    d.LogWarning("[DataService][Deserialize] Data string decompression failed. Using original data string.");
                }
            }

            d.Log("[DataService][Deserialize] Attempting to deserialize data string into object...");
            bool success = OnDeserialize(data, type, out deserializedObject);

            if(success)
                d.Log("[DataService][Deserialize] Data string deserialized successfully.");
            else
                d.LogError("[DataService][Deserialize] Data string deserialization failed!");

            return success;
        }

        /// <summary>Attempt to deseralize an object from a data string</summary>
        public abstract bool OnDeserialize(in string dataString, Type type, out object deserializedObject);


        private void ReadHeader(in string data, out int dataVersion, out string dataPayload)
        {
            byte[] dataStringBytes = Encoding.UTF8.GetBytes(data);

            uint magicNumber = BitConverter.ToUInt32(dataStringBytes, 0);
            if (magicNumber != k_MagicNumber)
                throw new System.Exception($"Invalid magic number: {magicNumber} when trying to read header for data string.\nData:\n{data}");
            
            dataVersion = BitConverter.ToInt32(dataStringBytes, sizeof(uint));
            dataPayload = Encoding.UTF8.GetString(dataStringBytes, sizeof(uint) + sizeof(int), dataStringBytes.Length - (sizeof(uint) + sizeof(int)));
        }
    }
}
