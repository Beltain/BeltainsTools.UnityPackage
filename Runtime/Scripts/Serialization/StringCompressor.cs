using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BeltainsTools.Serialization
{
    /// <summary>Provides methods for compressing and decompressing strings using GZip compression.</summary>
    /// <remarks>
    /// This class uses a magic number to validate compressed data and ensures that only data compressed by this class is decompressed.<br/>
    /// Huge thanks to <see href="https://stackoverflow.com/users/1315444/fubo">fubo</see> on <see href="https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp">Stack Overflow</see>
    /// </remarks>
    internal static class StringCompressor
    {
        public const uint s_MagicNumber = 0xC0C0C0C0; // Magic number for compression validation

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

            byte[] gZipBuffer = new byte[compressedData.Length + 4 + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(s_MagicNumber), 0, gZipBuffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 4, 4);
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 8, compressedData.Length);
            memoryStream.Close();
            return Convert.ToBase64String(gZipBuffer);
        }

        public static bool TryDecompressString(string compressedText, out string decompressedText)
        {
            decompressedText = null;
            if (!IsCompressedString(compressedText))
                return false;

            try
            {
                decompressedText = DecompressString(compressedText);
                return true;
            }
            catch (Exception ex)
            {
                d.LogError($"Failed to decompress string: {ex.Message}");
                return false;
            }
        }

        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                uint magicNumber = BitConverter.ToUInt32(gZipBuffer, 0);
                if (magicNumber != s_MagicNumber)
                    throw new InvalidDataException("Invalid magic number in compressed data");

                int dataLength = BitConverter.ToInt32(gZipBuffer, 4);
                memoryStream.Write(gZipBuffer, 8, gZipBuffer.Length - 8);

                byte[] buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        public static bool IsCompressedString(string text)
        {
            try
            {
                byte[] gZipBuffer = Convert.FromBase64String(text);
                uint magicNumber = BitConverter.ToUInt32(gZipBuffer, 0);
                return magicNumber == s_MagicNumber;
            }
            catch
            {
                return false;
            }
        }
    }
}
