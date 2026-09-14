using Newtonsoft.Json;
using UnityEngine;

namespace BeltainsTools.Serialization
{
    /// <summary>Base class for versioned data, allowing for migration between versions and static type lookups for serialisation purposes.</summary>
    /// <remarks>Use this on the base class for your versioned data. (ie. GameData has versions => GameData_V1, GameData_V2, etc.)</remarks>
    public abstract class VersionedSaveData : SaveData
    {
        /// <summary>Essentially acts as a static lookup for our various inheriting classes</summary>
        /// <remarks>
        /// ie. If you have a base class "GameData" that inherits <see cref="VersionedSaveData"/>, this should return "GameData_V1" at index 0, "GameData_V2" at index 1, etc.<br/>
        /// NB: This list should never shrink in size, and the order should never change, as it is used to determine the version number of a given save data instance.
        /// </remarks>
        [JsonIgnore]
        public abstract System.Type[] VersionLookup { get; }


        public int Version => GetVersionFromType(this.GetType());

        public int GetVersionFromType(System.Type type)
        {
            for (int i = 0; i < VersionLookup.Length; i++)
                if (VersionLookup[i] == type)
                    return i + 1;
            return -1;
        }

        public System.Type GetTypeFromVersion(int versionNumber)
        {
            if (versionNumber < 1 || versionNumber > VersionLookup.Length)
                return null;
            return VersionLookup[versionNumber - 1];
        }

        public bool TryMigrateTo<T>(out T migratedData) where T : VersionedSaveData
        {
            try
            {
                if (TryMigrateToVersion(GetVersionFromType(typeof(T)), out VersionedSaveData data))
                {
                    migratedData = data as T;
                    return true;
                }
            }
            catch (System.Exception e)
            {
                d.LogErrorFormat("Failed trying to migrate {0} of type '{1}' to type '{2}', exception: {3}", nameof(VersionedSaveData), this.GetType(), typeof(T), e);
            }
            migratedData = null;
            return false;
        }

        public bool TryMigrateToLatest(out VersionedSaveData migratedData)
        {
            return TryMigrateToVersion(VersionLookup.Length, out migratedData);
        }

        public bool TryMigrateToVersion(int targetVersion, out VersionedSaveData migratedData)
        {
            migratedData = this;

            if (targetVersion == Version)
                return true;

            if (targetVersion < Version)
            {
                d.LogErrorFormat("Failed trying to migrate {0} of type '{1}' from version {2} to version {3}, we cannot downgrade!", nameof(VersionedSaveData), this.GetType(), Version, targetVersion);
                return false;
            }

            try
            {
                int startingVersion = Version;
                while (migratedData.Version < targetVersion && migratedData.GetIsMigratable())
                    migratedData = migratedData.MigrateOnce();
                d.LogFormat("Migrated {0} of type '{1}' from version {2} to version {3}", nameof(VersionedSaveData), this.GetType(), startingVersion, migratedData.Version);
                return true;
            }
            catch (System.Exception e)
            {
                d.LogErrorFormat("Failed migrating {0} of type '{1}' from version {2} to version {3}, exception: {4}", nameof(VersionedSaveData), this.GetType(), Version, targetVersion, e);
                return false;
            }
        }

        public abstract bool GetIsMigratable();

        /// <returns>A new VersionedSaveData that has been uplifted to the next version</returns>
        public abstract VersionedSaveData MigrateOnce();
    }
}
