using System;
using UnityEngine;

namespace BeltainsTools
{
    /// <summary>Persistent GUID (Globally Unique Identifier) that can be serialized in Unity and to files and for maintaining references in save data</summary>
    /// <remarks><i>
    /// Largely based on work done by: <a href="https://github.com/adammyhre">adammyhre</a>
    /// (<a href="https://www.youtube.com/@git-amend">git-amend</a> on youtube) for his 
    /// <a href="https://github.com/adammyhre/Unity-Inventory-System/blob/master/Assets/_Project/Scripts/Inventory/Helpers/SerializableGuid.cs">
    /// Unity-Inventory-System</a> repository
    /// </i></remarks>
    [System.Serializable]
    public struct SerialisableGuid : IEquatable<SerialisableGuid>
    {
        [SerializeField, HideInInspector] public uint Part1;
        [SerializeField, HideInInspector] public uint Part2;
        [SerializeField, HideInInspector] public uint Part3;
        [SerializeField, HideInInspector] public uint Part4;

        public static SerialisableGuid Empty => new(0, 0, 0, 0);

        public static implicit operator Guid(SerialisableGuid serializableGuid) => serializableGuid.ToSystemGuid();
        public static implicit operator SerialisableGuid(Guid systemGUID) => new SerialisableGuid(systemGUID);

        public static bool operator ==(SerialisableGuid left, SerialisableGuid right) => left.Equals(right);
        public static bool operator !=(SerialisableGuid left, SerialisableGuid right) => !(left == right);

        public SerialisableGuid(uint val1, uint val2, uint val3, uint val4)
        {
            Part1 = val1;
            Part2 = val2;
            Part3 = val3;
            Part4 = val4;
        }

        public SerialisableGuid(Guid systemGUID)
        {
            byte[] bytes = systemGUID.ToByteArray();
            Part1 = BitConverter.ToUInt32(bytes, 0);
            Part2 = BitConverter.ToUInt32(bytes, 4);
            Part3 = BitConverter.ToUInt32(bytes, 8);
            Part4 = BitConverter.ToUInt32(bytes, 12);
        }

        public static SerialisableGuid NewGuid() => new (Guid.NewGuid());

        public static SerialisableGuid FromHexString(string hexString)
        {
            if (hexString.Length != 32)
            {
                return Empty;
            }

            return new SerialisableGuid
            (
                Convert.ToUInt32(hexString.Substring(0, 8), 16),
                Convert.ToUInt32(hexString.Substring(8, 8), 16),
                Convert.ToUInt32(hexString.Substring(16, 8), 16),
                Convert.ToUInt32(hexString.Substring(24, 8), 16)
            );
        }

        public string ToHexString()
        {
            return $"{Part1:X8}{Part2:X8}{Part3:X8}{Part4:X8}";
        }

        public Guid ToSystemGuid()
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(Part1).CopyTo(bytes, 0);
            BitConverter.GetBytes(Part2).CopyTo(bytes, 4);
            BitConverter.GetBytes(Part3).CopyTo(bytes, 8);
            BitConverter.GetBytes(Part4).CopyTo(bytes, 12);
            return new Guid(bytes);
        }

        public override bool Equals(object obj)
        {
            return obj is SerialisableGuid guid && this.Equals(guid);
        }

        public bool Equals(SerialisableGuid other)
        {
            return 
                Part1 == other.Part1 && 
                Part2 == other.Part2 && 
                Part3 == other.Part3 && 
                Part4 == other.Part4;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Part1, Part2, Part3, Part4);
        }
    }
}
