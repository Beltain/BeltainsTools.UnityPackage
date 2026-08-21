using System.Collections;
using System.Collections.Generic;
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

    public interface IIDDataSaver : IIDDataSaver<BeltainsTools.Serialization.SaveData> { }
    public interface IIDDataSaver<T> : BeltainsTools.Serialization.IDataSaver<T> where T : BeltainsTools.Serialization.SaveData
    {
        string GetID();
    }
}
