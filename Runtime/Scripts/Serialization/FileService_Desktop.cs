using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;

namespace BeltainsTools.Serialization
{
    public static partial class FileServices
    {
        public class FileService_Desktop : FileService
        {
            public override bool WriteFile(in string data, string filePath, bool encrypt)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                string directory = Path.GetDirectoryName(filePath);

                Debug.Log($"[WriteFile] Starting save to {filePath} with encryption {(encrypt ? "enabled" : "disabled")}");

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (File.Exists(filePath))
                {
                    Debug.Log($"[WriteFile] Overwriting save file at {filePath} as it already exists");
                    DeleteFile(filePath);
                }

                using Stream stream = File.Create(filePath);
                stream.Close();

                string dataEncrypted = data;
                if (encrypt)
                {
                    dataEncrypted = BeltainsTools.Serialization.StringCompressor.CompressString(dataEncrypted);
                }

                try
                {
                    File.WriteAllText(filePath, dataEncrypted);
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat($"[WriteFile] Failed to write data to disk after  [{stopwatch.ElapsedMilliseconds}] milliseconds:\n[{e.Message}]\n[{e.StackTrace}]");
                    return false;
                }

                Debug.Log($"[WriteFile] Succeeded after [{stopwatch.ElapsedMilliseconds}] milliseconds");
                return true;
            }

            public override bool ReadFile(out string data, string filePath, bool encrypted)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                string directory = Path.GetDirectoryName(filePath);

                Debug.Log($"[ReadFile] Starting load at {filePath} with encryption {(encrypted ? "enabled" : "disabled")}");

                if (!Directory.Exists(directory) || !File.Exists(filePath))
                {
                    data = string.Empty;
                    Debug.Log($"[ReadFile] Failed to read data after [{stopwatch.ElapsedMilliseconds}] milliseconds as the file or directory does not exist");
                    stopwatch.Stop();
                    return false;
                }

                try
                {
                    data = File.ReadAllText(filePath);
                }
                catch (System.Exception e)
                {
                    data = string.Empty;
                    Debug.LogErrorFormat($"[ReadFile] Failed to read data from disk after [{stopwatch.ElapsedMilliseconds}] milliseconds:\n[{e.Message}]\n[{e.StackTrace}]");
                    stopwatch.Stop();
                    return false;
                }

                if (encrypted)
                {
                    data = StringCompressor.DecompressString(data);
                }

                Debug.Log($"[ReadFile] Succeeded after [{stopwatch.ElapsedMilliseconds}] milliseconds");
                stopwatch.Stop();
                return true;
            }

            public override string ReadFileHash(string filePath)
            {
                using (SHA256 sha256 = SHA256.Create())
                using (Stream stream = File.OpenRead(filePath))
                {
                    byte[] byteHash = sha256.ComputeHash(stream);
                    return System.BitConverter.ToString(byteHash).Replace("-", "").ToLowerInvariant();
                }
            }

            public override bool DeleteFile(string filePath)
            {
                File.Delete(filePath);
                return !File.Exists(filePath);
            }

            public override string[] GetFiles(string directory, string extension = null)
            {
                if (!Directory.Exists(directory))
                    return new string[0];

                if (extension.IsNullOrEmpty())
                    return Directory.GetFiles(directory);

                return Directory.GetFiles(directory, $"*.{extension}");
            }
        }
    }
}
