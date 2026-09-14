using BeltainsTools.EventHandling;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Serialization
{
    /// <summary>
    /// Handles saving and loading of versioned save data to disk
    /// Includes automatic version migration when loading older versions
    /// </summary>
    public class SaveService
    {
        /// <summary>Placeholder metadata class for when you don't want to use metadata, but the SaveService requires a second generic type parameter</summary>
        public class EmptyMetaData : VersionedSaveData
        {
            public override System.Type[] VersionLookup { get; } = new System.Type[] { typeof(EmptyMetaData) };
            public override bool GetIsMigratable() => false;
            public override VersionedSaveData MigrateOnce() => throw new System.NotImplementedException();
        }

        /// <summary>This implementor inherits a <see cref="FileName"/> property from the <see cref="SaveService"/> when saved or loaded from the disk</summary>
        public interface IFileNameInheritor
        {
            string FileName { get; set; }
        }

        /// <summary>Interface for <see cref="SaveService"/> data and metadata, allowing for callbacks when reading or writing to disk</summary>
        public interface IReadWriteListener
        {
            void OnReadFromDisk(string fileName);
            void OnWritingToDisk(string fileName);
        }

        /// <summary>Interface for <see cref="SaveService"/> data, allowing for callbacks when saving (capturing) or loading (restoring)</summary>
        public interface ISaveLoadListener
        {
            void OnSave();
            void OnLoad();
        }

        /// <summary>Interface for objects that can capture and restore their state to/from a <see cref="SaveService{TSaveData, TSaveMetaData}"/> data object</summary>
        public interface IDataCapturer<TData>
        {
            void Capture(TData data);
            void Restore(in TData data);
        }
    }

    /// <inheritdoc cref="SaveService"/>
    public class SaveService<TSaveData> : SaveService<TSaveData, SaveService.EmptyMetaData>
        where TSaveData : VersionedSaveData, new()
    {
        public SaveService(DataService dataService, FileService fileService) : base(dataService, fileService, null) { }
    }

    /// <inheritdoc cref="SaveService"/>
    public class SaveService<TSaveData, TSaveMetaData> : SaveService
        where TSaveData : VersionedSaveData, new()
        where TSaveMetaData : VersionedSaveData, new()
    {
        public readonly DataService DataService;
        public readonly FileService FileService;

        private readonly TSaveData m_StaticDataHelper; // really rancid workaround for C# generics / abstract statics limitations pre 11.0
        private readonly TSaveMetaData m_StaticMetaDataHelper; // really rancid workaround for C# generics / abstract statics limitations pre 11.0

        private readonly System.Func<TSaveData, TSaveMetaData> m_MetaDataFactory;

        private HashSet<IDataCapturer<TSaveData>> m_Gatherers = new HashSet<IDataCapturer<TSaveData>>();

        private List<MetaDataCacheObject> m_CachedMetadatas = new List<MetaDataCacheObject>();

        public TSaveData Data { get; private set; } = null;
        public TSaveMetaData MetaData { get; private set; } = null;

        /// <summary>Event invoked when the <see cref="Data"/> object is changed. Does not get triggered when the object is mutated</summary>
        public BEvent<TSaveData> DataChangedEvent;
        /// <summary>Event invoked when the <see cref="MetaData"/> object is changed. Does not get triggered when the object is mutated</summary>
        public BEvent<TSaveMetaData> MetaDataChangedEvent;
        /// <summary>Event invoked when the <see cref="Data"/> object is loaded from disk.</summary>
        public BEvent<TSaveData> DataLoadedEvent;
        /// <summary>Event invoked when the <see cref="Data"/> object is saved to disk.</summary>
        public BEvent<TSaveData> DataSavedEvent;
        /// <summary>Event invoked when the Cached metadatas are changed to reflect updates to metadatas on disk</summary>
        public BEvent<IEnumerable<TSaveMetaData>> MetaDatasCacheChangedEvent;

        private class MetaDataCacheObject
        {
            public string FileName;
            public string FileHash;
            public TSaveMetaData MetaData;
        }

        public SaveService(DataService dataService, FileService fileService, System.Func<TSaveData, TSaveMetaData> metaDataFactory)
        {
            DataService = dataService;
            FileService = fileService;
            m_StaticDataHelper = new TSaveData();
            m_StaticMetaDataHelper = new TSaveMetaData();
            m_MetaDataFactory = metaDataFactory;
        }


        /// <summary>Register the <paramref name="gatherer"/> for mass capturing/restoring and restore its state from current data</summary>
        public void RegisterAndRestoreGatherer(IDataCapturer<TSaveData> gatherer)
        {
            if (m_Gatherers.Add(gatherer))
                Restore(gatherer);
        }

        /// <summary>Deregister the <paramref name="gatherer"/> from mass capturing/restoring. Capture its state while deregistering.</summary>
        public void DeregisterAndCaptureGatherer(IDataCapturer<TSaveData> gatherer)
        {
            if (m_Gatherers.Remove(gatherer))
                Capture(gatherer);
        }

        /// <summary>Register the <paramref name="gatherer"/> for mass capturing/restoring</summary>
        public bool RegisterGatherer(IDataCapturer<TSaveData> gatherer)
        {
            return m_Gatherers.Add(gatherer);
        }

        /// <summary>Deregister the <paramref name="gatherer"/> from mass capturing/restoring</summary>
        public bool DeregisterGatherer(IDataCapturer<TSaveData> gatherer)
        {
            return m_Gatherers.Remove(gatherer);
        }

        /// <summary>Restore the state of all registered <see cref="m_Gatherers"/> from <see cref="Data"/></summary>
        public void RestoreGatherers()
        {
            foreach (IDataCapturer<TSaveData> gatherer in m_Gatherers)
                Restore(gatherer);
        }

        /// <summary>Restore the state of the passed <paramref name="capturer"/> from <see cref="Data"/></summary>
        public void Restore(IDataCapturer<TSaveData> capturer)
        {
            capturer.Restore(Data);
        }

        /// <summary>Capture the state of all registered <see cref="m_Gatherers"/> into <see cref="Data"/></summary>
        public void CaptureGatherers()
        {
            foreach (IDataCapturer<TSaveData> gatherer in m_Gatherers)
                Capture(gatherer);
        }

        /// <summary>Capture the state of the passed <paramref name="capturer"/> into <see cref="Data"/></summary>
        public void Capture(IDataCapturer<TSaveData> capturer)
        {
            capturer.Capture(Data);
        }




        /// <returns>An enumerable of <see cref="TSaveMetaData"/> objects representing the metadata of the currently saved files on disk</returns>
        public IEnumerable<TSaveMetaData> GetMetaDatas()
        {
            RefreshMetadatas();
            return m_CachedMetadatas.Select(x => x.MetaData);
        }

        /// <summary>Refresh the cached metadata list to match the current files on disk</summary>
        /// <remarks>Warning: This method may be expensive if there are many files on disk.</remarks>
        private void RefreshMetadatas()
        {
            bool anyChanged = false;
            string[] saveNames = FileService.GetFileNames();

            // remove stale from cached
            for (int i = m_CachedMetadatas.Count - 1; i >= 0; i--)
            {
                if (!saveNames.Contains(m_CachedMetadatas[i].FileName) || // file no longer exists, so metadata is stale
                    string.Compare(m_CachedMetadatas[i].FileHash, FileService.ReadFileHash(m_CachedMetadatas[i].FileName)) != 0) // file hash changed, so metadata is stale
                {
                    m_CachedMetadatas.RemoveAt(i);
                    anyChanged = true;
                }
            }

            // add missing to cache
            foreach (string saveName in saveNames)
            {
                if (m_CachedMetadatas.Any(x => x.FileName == saveName))
                    continue;

                if (!TryReadOrCreateMetaDataFromDisk(saveName, out TSaveMetaData metaData))
                    continue;

                m_CachedMetadatas.Add(new MetaDataCacheObject
                {
                    FileName = saveName,
                    FileHash = FileService.ReadFileHash(saveName),
                    MetaData = metaData
                });
                anyChanged = true;
            }

            if (anyChanged)
            {
                MetaDatasCacheChangedEvent.Invoke(m_CachedMetadatas.Select(x => x.MetaData));
            }
        }



        /// <summary>Create new data object and set it as the current <see cref="Data"/></summary>
        public void CreateNew()
        {
            Set(new TSaveData());
            d.Log($"[SaveService] Created new save data. [{typeof(TSaveData)}]");
        }

        /// <summary>Load existing data from disk or create new if none exists</summary>
        public void LoadOrCreateNew(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !TryLoadFromDisk(fileName))
            {
                CreateNew();
                if (Data is IFileNameInheritor inheritor)
                    inheritor.FileName = fileName;
                d.LogWarning($"[SaveService] Unable to load \"{fileName}\" from disk. Created new save data instead. [{typeof(TSaveData)}]");
            }
        }

        /// <summary>Try read and set data from disk</summary>
        /// <returns>True if read was successful</returns>
        public bool TryLoadFromDisk(string fileName)
        {
            bool dataReadSuccessful = TryReadFromDiskMigrated(fileName, out TSaveData dataFromDisk, out TSaveMetaData metaDataFromDisk);
            if (!dataReadSuccessful)
                return false;

            if (dataFromDisk is ISaveLoadListener listenerData)
                listenerData.OnLoad();

            Set(dataFromDisk);
            d.Log($"[SaveService] Loaded existing save data from disk: {fileName} [{typeof(TSaveData)}]");

            DataLoadedEvent.Invoke(Data);
            return true;
        }

        /// <inheritdoc cref="SaveService{TSaveData, TSaveMetaData}.SaveToDisk(string)"/>
        public void SaveToDisk()
        {
            if (Data == null)
            {
                d.LogError("[SaveService] Cannot write null Data to disk!");
                return;
            }

            if (!(Data is IFileNameInheritor inheritorData))
            {
                d.LogError("[SaveService] Data does not implement ISaveLoadListener! Use SaveToDisk(string fileName) instead.");
                return;
            }

            SaveToDisk(inheritorData.FileName);
        }

        /// <summary>Gather current state into data and write to disk</summary>
        public void SaveToDisk(string fileName)
        {
            if (Data == null)
            {
                d.LogError($"[SaveService] Trying to write [{typeof(TSaveData)}] to disk when we have not yet initialised our Current! Please create first! Aborting...");
                return;
            }

            if (fileName.IsNullOrEmpty())
            {
                d.LogError($"[SaveService] Trying to write [{typeof(TSaveData)}] to disk with null file name! Aborting...");
                return;
            }

            if (Data is ISaveLoadListener listenerData)
                listenerData.OnSave();
            CaptureGatherers(); // capture all registered gatherers into the current data before saving to disk
                                // NB: Not the only way save data can be changed, can be populated throughout the game, so this is just a best effort to capture all known gatherers before saving

            UpdateMetaData();

            WriteToDisk(Data, MetaData, fileName);
            DataSavedEvent.Invoke(Data);
        }

        /// <summary>Try read data from disk, applying any necessary migrations</summary>
        public bool TryReadFromDiskMigrated(string fileName, out TSaveData data, out TSaveMetaData metaData)
        {
            data = null;
            metaData = null;

            bool fileReadSuccessful = FileService.ReadFromFile(fileName, out string[] dataStrings);
            if (!fileReadSuccessful)
                return false;

            try
            {
                data = ParseMigratedDataFromRaw(in dataStrings);
                if (data is IReadWriteListener listenerData)
                    listenerData.OnReadFromDisk(fileName);
                if (data is IFileNameInheritor inheritor)
                    inheritor.FileName = fileName;
                metaData = CreateMetaDataFromData(data); // skip reading, just fucken create it from the data since we don't care about the standalone metadata in this case
                return true;
            }
            catch (System.Exception e)
            {
                d.LogError($"[SaveService] OH NO! Unable to complete data read and migration from file {fileName}... THIS IS BAD:\n{e.Message}");
                return false;
            }
        }

        /// <summary>Try read data from disk, without applying any migrations</summary>
        public bool TryReadFromDisk(string fileName, out VersionedSaveData data, out VersionedSaveData metaData)
        {
            metaData = null;
            data = null;

            bool fileReadSuccessful = FileService.ReadFromFile(fileName, out string[] dataStrings);
            if (!fileReadSuccessful)
                return false;

            try
            {
                metaData = ParseOrCreateMetaDataFromRaw(in dataStrings);
                if (metaData is IReadWriteListener listenerMeta)
                    listenerMeta.OnReadFromDisk(fileName);
                if (metaData is IFileNameInheritor inheritor)
                    inheritor.FileName = fileName;
            }
            catch (System.Exception e)
            {
                d.LogError($"[SaveService] Unable to complete metadata read/creation from file {fileName}:\n{e.Message}");
            }

            try
            {
                data = ParseDataFromRaw(in dataStrings);
                if (data is IReadWriteListener listenerData)
                    listenerData.OnReadFromDisk(fileName);
                if (data is IFileNameInheritor inheritor)
                    inheritor.FileName = fileName;
            }
            catch (System.Exception e)
            {
                d.LogError($"[SaveService] Unable to complete data read from file {fileName}:\n{e.Message}");
                return false;
            }

            return true;
        }

        /// <summary>Try read metadata from disk or create if missing</summary>
        public bool TryReadOrCreateMetaDataFromDisk(string fileName, out TSaveMetaData metaData)
        {
            metaData = null;

            bool fileReadSuccessful = FileService.ReadFromFile(fileName, out string[] dataStrings);
            if (!fileReadSuccessful)
            {
                d.LogError($"[SaveService] Unable to read file {fileName} to get or create metadata. Returning false.");
                return false;
            }

            try
            {
                metaData = ParseOrCreateMetaDataFromRaw(in dataStrings);
                if (metaData is IReadWriteListener listenerMeta)
                    listenerMeta.OnReadFromDisk(fileName);
                if (metaData is IFileNameInheritor inheritor)
                    inheritor.FileName = fileName;
                return true;
            }
            catch (System.Exception e)
            {
                d.LogError($"[SaveService] Unable to get or create metadata from file {fileName}. Returning false:\n{e.Message}");
                return false;
            }
        }

        private TSaveMetaData ParseOrCreateMetaDataFromRaw(in string[] rawDataStrings)
        {
            try
            {
                return ParseMigratedMetaDataFromRaw(rawDataStrings);
            }
            catch (System.Exception e)
            {
                d.LogWarning($"[SaveService] Unable to parse metadata from raw data:\n{e.Message}");
            }

            try
            {
                TSaveData data = ParseMigratedDataFromRaw(in rawDataStrings); // load data to create metadata if it doesn't exist
                d.LogFormat("[SaveService] Creating new metadata from data since metadata was not present in raw data.");
                return CreateMetaDataFromData(data);
            }
            catch (System.Exception e)
            {
                d.LogError($"[SaveService] Unable to parse data from raw data to create metadata:\n{e.Message}");
                return null;
            }
        }

        private TSaveMetaData CreateMetaDataFromData(TSaveData data)
        {
            return m_MetaDataFactory != null && data != null ? m_MetaDataFactory.Invoke(data) : new TSaveMetaData();
        }

        private TSaveData ParseMigratedDataFromRaw(in string[] rawDataStrings)
        {
            VersionedSaveData dataRaw = ParseDataFromRaw(in rawDataStrings);
            if (!dataRaw.TryMigrateToVersion(m_StaticDataHelper.Version, out VersionedSaveData migratedData))
                throw new System.Exception($"[SaveService] Unable to migrate data from raw data to current version {m_StaticDataHelper.Version}. Returning null.");
            return (TSaveData)migratedData;
        }

        private TSaveMetaData ParseMigratedMetaDataFromRaw(in string[] rawDataStrings)
        {
            VersionedSaveData metaDataRaw = ParseMetaDataFromRaw(in rawDataStrings);
            if (!metaDataRaw.TryMigrateToVersion(m_StaticMetaDataHelper.Version, out VersionedSaveData migratedMetaData))
                throw new System.Exception($"[SaveService] Unable to migrate metadata from raw data to current version {m_StaticMetaDataHelper.Version}. Returning null.");
            return (TSaveMetaData)migratedMetaData;
        }

        private VersionedSaveData ParseMetaDataFromRaw(in string[] rawDataStrings)
        {
            return ParseVersionedObjectFromDataString(in rawDataStrings[0], m_StaticMetaDataHelper);
        }

        private VersionedSaveData ParseDataFromRaw(in string[] rawDataStrings)
        {
            return ParseVersionedObjectFromDataString(in rawDataStrings[1], m_StaticDataHelper);
        }

        private VersionedSaveData ParseVersionedObjectFromDataString(in string dataString, VersionedSaveData staticDataTypeHelper)
        {
            bool success = DataService.TryGetVersion(in dataString, out int version);
            if (!success)
                throw new System.Exception($"[SaveService] [ParseVersionedObjectFromDataString] Unable to parse version from data string:\n{dataString}");

            System.Type type = staticDataTypeHelper.GetTypeFromVersion(version);
            if (type == null)
                throw new System.Exception($"[SaveService] [ParseVersionedObjectFromDataString] Unable to get type from version {version} for {typeof(VersionedSaveData)} from data string:\n{dataString}");

            success = DataService.Deserialize(in dataString, type, out object dataObject, out int _);

            if (!success)
                throw new System.Exception($"[ParseVersionedObjectFromDataString] Unable to parse {typeof(VersionedSaveData)} from data string:\n{dataString}");

            return (VersionedSaveData)dataObject;
        }


        /// <summary>Write data and metadata to disk</summary>
        public void WriteToDisk(TSaveData data, TSaveMetaData meta, string fileName)
        {
            if (fileName.IsNullOrEmpty())
            {
                d.LogError($"[SaveService] Trying to write [{typeof(TSaveData)}] to disk with null file name! Aborting...");
                return;
            }

            if (data == null)
            {
                d.LogError($"[SaveService] Trying to write null [{typeof(TSaveData)}] to disk! Aborting...");
                return;
            }

            if (data is IReadWriteListener listenerData)
                listenerData.OnWritingToDisk(fileName);
            if (meta is IReadWriteListener listenerMeta)
                listenerMeta.OnWritingToDisk(fileName);

            if (data is IFileNameInheritor inheritorData)
                inheritorData.FileName = fileName;
            if (meta is IFileNameInheritor inheritorMeta)
                inheritorMeta.FileName = fileName;

            try
            {
                DataService.Serialize(meta, out string metaDataString, meta.Version);
                byte[] metaDataBytes = System.Text.Encoding.UTF8.GetBytes(metaDataString);
                DataService.Serialize(data, out string dataString, data.Version);
                FileService.WriteToFile(fileName, metaDataString, dataString);
            }
            catch (System.Exception e)
            {
                d.LogError($"[SaveService] Unable to write [{typeof(TSaveMetaData)}] and [{typeof(TSaveData)}] to disk at {fileName}:\n{e.Message}");
            }
        }


        private void Set(TSaveData data)
        {
            bool changed = false;

            if (Data != data)
            {
                Data = data;
                changed = true;
            }

            UpdateMetaData();

            RestoreGatherers();

            if (changed)
                DataChangedEvent.Invoke(Data);
        }

        private void UpdateMetaData()
        {
            MetaData = CreateMetaDataFromData(Data);

            if (Data is IFileNameInheritor inheritorData && MetaData is IFileNameInheritor inheritorMeta)
                inheritorMeta.FileName = inheritorData.FileName;

            MetaDataChangedEvent.Invoke(MetaData);
        }
    }
}
