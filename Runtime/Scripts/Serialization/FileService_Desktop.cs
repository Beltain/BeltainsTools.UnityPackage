using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

namespace BeltainsTools.Serialization
{
    public static partial class FileServices
    {
        public class FileService_Desktop : FileService
        {
            public static readonly string s_SaveRootDirectory = Application.persistentDataPath;


            public override bool WriteData(in string data, string subDirectory, string fileName, bool encrypt)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                string directory = Path.Combine(s_SaveRootDirectory, subDirectory);
                string filePath = Path.Combine(directory, fileName);

                Debug.Log($"[Save Write] Starting save to {filePath} with encryption {(encrypt ? "enabled" : "disabled")}");

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if(File.Exists(filePath))
                {
                    Debug.Log($"[Save Write] Overwriting save file at {filePath} as it already exists");
                    File.Delete(filePath);
                }

                using Stream stream = File.Create(filePath);
                stream.Close();

                string dataEncrypted = data;
                if (encrypt)
                {
                    dataEncrypted = BeltainsTools.Serialization.StringCompressor.CompressString(dataEncrypted);
                    //byte[] dataAsBytes = Encoding.UTF8.GetBytes(dataEncrypted);
                    //dataEncrypted = Convert.ToBase64String(dataAsBytes);
                }

                try
                {
                    File.WriteAllText(filePath, dataEncrypted);
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat($"[Save Write] Failed to write data to disk after  [{stopwatch.ElapsedMilliseconds}] milliseconds:\n[{e.Message}]\n[{e.StackTrace}]");
                    return false;
                }

                Debug.Log($"[Save Write] Succeeded after [{stopwatch.ElapsedMilliseconds}] milliseconds");
                return true;
            }

            public override bool ReadData(out string data, string subDirectory, string fileName, bool encrypted)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                string directory = Path.Combine(s_SaveRootDirectory, subDirectory);
                string filePath = Path.Combine(directory, fileName);

                Debug.Log($"[Save Read] Starting load at {filePath} with encryption {(encrypted ? "enabled" : "disabled")}");

                if (!Directory.Exists(directory) || !File.Exists(filePath))
                {
                    data = string.Empty;
                    Debug.Log($"[Save Read] Failed to read data after [{stopwatch.ElapsedMilliseconds}] milliseconds as the file or directory does not exist");
                    stopwatch.Stop();
                    return false;
                }

                try
                {
                    data = File.ReadAllText(filePath);
                }
                catch (Exception e)
                {
                    data = string.Empty;
                    Debug.LogErrorFormat($"[Save Read] Failed to read data from disk after [{stopwatch.ElapsedMilliseconds}] milliseconds:\n[{e.Message}]\n[{e.StackTrace}]");
                    stopwatch.Stop();
                    return false;
                }

                if (encrypted)
                {
                    data = StringCompressor.DecompressString(data);
                    //byte[] dataAsBytes = Convert.FromBase64String(data);
                    //data = Encoding.UTF8.GetString(dataAsBytes);
                }

                Debug.Log($"[Save Read] Succeeded after [{stopwatch.ElapsedMilliseconds}] milliseconds");
                stopwatch.Stop();
                return true;
            }
        }
    }
}
