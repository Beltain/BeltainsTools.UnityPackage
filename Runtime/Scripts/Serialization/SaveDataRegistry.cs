using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Serialization
{
    /// <summary>Object that allows for storing and restoring save data for objects that implement <see cref="IIDDataSaver{T}"/>.</summary>
    public class SaveDataRegistry<T> where T : SaveData
    {
        [JsonProperty]
        private Dictionary<string, T> m_Objects = new Dictionary<string, T>();


        public void Restore(IIDDataSaver<T> saver)
        {
            if (m_Objects.TryGetValue(saver.GetID(), out T obj))
                saver.Deserialize(obj);
        }

        public void Remove(IIDDataSaver<T> saver)
        {
            if (m_Objects.ContainsKey(saver.GetID()))
                m_Objects.Remove(saver.GetID());
        }

        public void Store(IIDDataSaver<T> saver)
        {
            bool saved = saver.Serialize(out T saveData);
            if (!saved)
                return;
            m_Objects[saver.GetID()] = saveData;
        }
    }
}
