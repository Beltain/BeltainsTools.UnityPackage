using System.IO;

namespace BeltainsTools.Serialization
{
    /// <summary>Service responsible for writing/reading strings to/from a location</summary>
    public abstract class FileService
    {
        protected const uint k_MagicNumber = 0xF15EF15E; // Shared magic number for file validation,
                                                            // used to ensure that the file was written by this service and not some other process

        public readonly string RootPath;
        public readonly string Extension;

        public FileService(string rootPath, string extension) 
        {
            RootPath = rootPath;
            Extension = extension.StartsWith(".") ? extension : "." + extension;
        }

        /// <returns>The path relative to the <see cref="RootPath"/> defined for this <see cref="FileService"/>'s files.</returns>
        private string GetFullFilePath(string relativeFilePath) 
            => GetFullPath(relativeFilePath) + Extension;
        /// <returns>The path relative to the <see cref="RootPath"/> defined for this <see cref="FileService"/>.</returns>
        private string GetFullPath(string relativePath)
            => relativePath.IsNullOrEmpty() ? 
                RootPath : 
                RootPath.IsNullOrEmpty() ? 
                    relativePath : 
                    Path.Combine(RootPath, relativePath);


        /// <summary>Performs a file write to the <paramref name="fullFilePath"/> with the individual <paramref name="dataPayloads"/></summary>
        protected abstract bool OnWriteToFile(string fullFilePath, string[] dataPayloads);
        /// <summary>Attempt to write serialized data payloads to the <paramref name="relativeFilePath"/> to the <see cref="RootPath"/></summary>
        public bool WriteToFile(string relativeFilePath, params string[] dataPayloads)
        {
            return OnWriteToFile(GetFullFilePath(relativeFilePath), dataPayloads); // pass the full path to the abstract method for implementation
        }

        /// <summary>Performs a file read from the <paramref name="fullFilePath"/>, and outputs the individual <paramref name="dataPayloads"/></summary>
        protected abstract bool OnReadFromFile(string fullFilePath, out string[] dataPayloads);
        /// <summary>Attempt to read serialized data payloads from the <paramref name="relativeFilePath"/> to the <see cref="RootPath"/></summary>
        public bool ReadFromFile(string relativeFilePath, out string[] dataPayloads)
        {
            return OnReadFromFile(GetFullFilePath(relativeFilePath), out dataPayloads); // pass the full path to the abstract method for implementation
        }

        /// <returns>The computed hash of the file at the given <paramref name="fullFilePath"/></returns>
        protected abstract string OnReadFileHash(string fullFilePath);
        /// <summary>Get the hash for the file at the <paramref name="relativeFilePath"/> to the <see cref="RootPath"/></summary>
        /// <remarks>use to perform diff checks</remarks>
        public string ReadFileHash(string relativeFilePath)
        {
            return OnReadFileHash(GetFullFilePath(relativeFilePath));
        }

        /// <summary>Handles the deletion of a file at the given <paramref name="fullFilePath"/></summary>
        /// <returns>True if the file was successfully deleted, otherwise false</returns>
        protected abstract bool OnDeleteFile(string fullFilePath);
        /// <summary>Attempt to delete the file at the <paramref name="relativeFilePath"/> to the <see cref="RootPath"/></summary>
        /// <returns>True if the file was successfully deleted, otherwise false</returns>
        public bool DeleteFile(string relativeFilePath)
        {
            return OnDeleteFile(GetFullFilePath(relativeFilePath));
        }

        /// <summary>Handles the renaming of a file from <paramref name="fullFilePath"/> to <paramref name="newFullFilePath"/></summary>
        /// <returns>True if the file was successfully renamed, otherwise false</returns>
        protected abstract bool OnRenameFile(string fullFilePath, string newFullFilePath);
        /// <summary>Attempt to rename the file at the <paramref name="relativeFilePath"/> to the <paramref name="newRelativeFilePath"/> to the <see cref="RootPath"/></summary>
        /// <returns>True if the file was successfully renamed, otherwise false</returns>
        public bool RenameFile(string relativeFilePath, string newRelativeFilePath)
        {
            return OnRenameFile(GetFullFilePath(relativeFilePath), GetFullFilePath(newRelativeFilePath));
        }

        /// <summary>Handles the check for a file at the given <paramref name="fullFilePath"/></summary>
        /// <returns>True if the file exists, otherwise false</returns>
        protected abstract bool OnGetFileExists(string fullFilePath);
        /// <summary>Check if a file exists at the given <paramref name="relativeFilePath"/> to the <see cref="RootPath"/></summary>
        /// <returns>True if the file exists, otherwise false</returns>
        public bool GetFileExists(string relativeFilePath)
        {
            return OnGetFileExists(GetFullFilePath(relativeFilePath));
        }

        /// <summary>Handles the retrieval of all files in the given <paramref name="fullDirectory"/> with the given <paramref name="extension"/></summary>
        protected abstract string[] OnGetFiles(string fullDirectory, string extension);
        /// <summary>Get all managed files in the provided <paramref name="relativeDirectory"/> with the configured <see cref="Extension"/></summary>
        /// <returns>All managed files in the provided <paramref name="relativeDirectory"/> with the configured <see cref="Extension"/></returns>
        public string[] GetFiles(string relativeDirectory = null)
        {
            return OnGetFiles(GetFullPath(relativeDirectory), Extension);
        }
    }
}
