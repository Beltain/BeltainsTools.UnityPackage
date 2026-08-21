using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;

namespace BeltainsTools.Serialization
{
    public class FileService_Desktop : FileService
    {
        public FileService_Desktop(string rootPath, string extension) : base(rootPath, extension) { }

        protected override bool OnWriteToFile(string fullFilePath, string[] dataPayloads)
        {
            // prepare directory
            string directory = Path.GetDirectoryName(fullFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // handle file overwrite by writing to a temporary file first, then replacing the original file with the temporary file
            string writePath = fullFilePath;
            if (File.Exists(writePath))
            {
                d.Log($"[FileService][Write] Overwriting file at {writePath}, creating temporary file...");
                writePath = writePath + ".tmp";
                if (File.Exists(writePath))
                    File.Delete(writePath); // delete existing tmp file if any
            }

            // do the write operation to the temporary file (or the original file if it doesn't exist)
            try
            {
                d.Log($"[FileService][Write] Starting file write to {writePath} with {dataPayloads.Length} payloads...");
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                using (BinaryWriter writer = new BinaryWriter(File.Open(writePath, FileMode.Create, FileAccess.Write)))
                {
                    writer.Write(k_MagicNumber); // write magic number for validation
                    for (int i = 0; i < dataPayloads.Length; i++) // write each payload to the output file
                    {
                        byte[] payloadBytes = Encoding.UTF8.GetBytes(dataPayloads[i]);
                        writer.Write(payloadBytes.Length); // length prefix for each payload
                        writer.Write(payloadBytes);
                    }
                }

                d.Log($"[FileService][Write] ...file write at {writePath} completed successfully after [{stopwatch.ElapsedMilliseconds}] milliseconds!");
                stopwatch.Stop();
            }
            catch (System.Exception e)
            {
                d.LogError($"[FileService][Write] Failed to write file at {writePath}: {e}");
                return false;
            }

            // cleanup and replace the original file with the temporary file if they are different
            if (string.Compare(writePath, fullFilePath) != 0)
            {
                try
                {
                    File.Replace(writePath, fullFilePath, destinationBackupFileName: null);
                    d.Log($"[FileService][Write] ...file at {fullFilePath} overwritten successfully with file at {writePath}");
                }
                catch (System.Exception e)
                {
                    d.LogError($"[FileService][Write] Failed to overwrite file at {fullFilePath} with file at {writePath}: {e}");
                    return false;
                }
            }
            return true;
        }

        protected override bool OnReadFromFile(string fullFilePath, out string[] dataPayloads)
        {
            string directory = Path.GetDirectoryName(fullFilePath);

            if (!Directory.Exists(directory) || !File.Exists(fullFilePath))
            {
                dataPayloads = System.Array.Empty<string>();
                d.Log($"[FileService][Read] Failed to read data as the file or directory does not exist: {fullFilePath}");
                return false;
            }

            try
            {
                d.Log($"[FileService][Read] Starting file read from {fullFilePath}...");
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                using (BinaryReader reader = new BinaryReader(File.Open(fullFilePath, FileMode.Open, FileAccess.Read)))
                {
                    if (reader.ReadUInt32() != k_MagicNumber) // validate magic number
                        throw new System.Exception("Invalid magic number, file may be corrupted or not written by this service");

                    List<string> payloads = new List<string>();
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        int length = reader.ReadInt32(); // read length prefix
                        byte[] payloadBytes = reader.ReadBytes(length);
                        string payload = Encoding.UTF8.GetString(payloadBytes);
                        payloads.Add(payload);
                    }
                    dataPayloads = payloads.ToArray();
                }

                d.Log($"[FileService][Read] ...file read at {fullFilePath} succeeded with {dataPayloads.Length} payloads after [{stopwatch.ElapsedMilliseconds}] milliseconds");
                stopwatch.Stop();
            }
            catch (System.Exception e)
            {
                dataPayloads = System.Array.Empty<string>();
                d.LogError($"[FileService][Read] Failed to read data from disk: {e}");
                return false;
            }

            return true;
        }

        protected override string OnReadFileHash(string fullFilePath)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    if (!File.Exists(fullFilePath))
                        throw new System.Exception($"File not found at {fullFilePath} for hash computation");

                    using (Stream stream = File.OpenRead(fullFilePath))
                    {
                        byte[] byteHash = sha256.ComputeHash(stream);
                        return System.BitConverter.ToString(byteHash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch (System.Exception e)
            {
                d.LogError($"[FileService][Hash] Failed to compute hash for file at {fullFilePath}: {e}");
                return null;
            }
        }

        protected override bool OnDeleteFile(string fullFilePath)
        {
            try
            {
                if (!File.Exists(fullFilePath))
                {
                    d.Log($"[FileService][Delete] File at {fullFilePath} does not exist, nothing to delete.");
                    return true; // file doesn't exist, consider it deleted
                }
                File.Delete(fullFilePath);
                d.Log($"[FileService][Delete] File at {fullFilePath} deleted successfully.");
                return true;

            }
            catch (System.Exception e)
            {
                d.LogError($"[FileService][Delete] Failed to delete file at {fullFilePath}: {e}");
                return false;
            }
        }

        protected override bool OnRenameFile(string fullFilePath, string newFullFilePath)
        {
            try
            {
                if (!File.Exists(fullFilePath))
                    throw new System.Exception($"File at {fullFilePath} does not exist, cannot rename.");
                if (File.Exists(newFullFilePath))
                    throw new System.Exception($"Target file at {newFullFilePath} already exists, cannot rename to an existing file.");

                string newDirectory = Path.GetDirectoryName(newFullFilePath);
                if (!Directory.Exists(newDirectory))
                    Directory.CreateDirectory(newDirectory);
                File.Move(fullFilePath, newFullFilePath);

                d.Log($"[FileService][Rename] File renamed from {fullFilePath} to {newFullFilePath} successfully.");
                return true;
            }
            catch (System.Exception e)
            {
                d.LogError($"[FileService][Rename] Failed to rename file from {fullFilePath} to {newFullFilePath}: {e}");
                return false;
            }
        }

        protected override bool OnGetFileExists(string fullFilePath)
        {
            return File.Exists(fullFilePath);
        }

        protected override string[] OnGetFiles(string fullDirectory, string extension)
        {
            if (!Directory.Exists(fullDirectory))
                return new string[0];

            string[] files;
            if (extension.IsNullOrEmpty())
                files = Directory.GetFiles(fullDirectory);
            else
                files = Directory.GetFiles(fullDirectory, $"*.{extension}");

            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);

            return files;
        }
    }
}
