using UnityEngine;

namespace BeltainsTools.DataStructures
{
    [System.Serializable]
    /// <summary>Data container with some simple editor functionality that allows the value to be flagged for logic gating or any other reason</summary>
    public class FlaggedValue<T> : System.IEquatable<T>, System.IEquatable<FlaggedValue<T>> where T : System.IEquatable<T>
    {
        public bool IsFlagged;
        public T Value;


        public FlaggedValue() : this(default, false) { }
        public FlaggedValue(T value) : this(value, false) { }
        public FlaggedValue(T value, bool flag)
        {
            Value = value;
            IsFlagged = flag;
        }

        public bool TryGetValue(out T value)
        {
            value = Value;
            return IsFlagged;
        }

        bool System.IEquatable<T>.Equals(T other)
        {
            return Value.Equals(other);
        }

        bool System.IEquatable<FlaggedValue<T>>.Equals(FlaggedValue<T> other)
        {
            return Value.Equals(other.Value) && IsFlagged == other.IsFlagged;
        }

        public static implicit operator T(FlaggedValue<T> flaggedValue) => flaggedValue.Value;
        public static implicit operator FlaggedValue<T>(T value) => new FlaggedValue<T>(value);

        public static bool operator ==(FlaggedValue<T> left, FlaggedValue<T> right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(FlaggedValue<T> left, FlaggedValue<T> right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is FlaggedValue<T> other)
                return Equals(other);
            if (obj is T value)
                return Value.Equals(value);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Value != null ? Value.GetHashCode() : 0);
                hash = hash * 23 + IsFlagged.GetHashCode();
                return hash;
            }
        }
    }

    public class FlagLabelAttribute : PropertyAttribute
    {
        public string FlagLabel { get; }

        public FlagLabelAttribute(string flagLabel)
        {
            FlagLabel = flagLabel;
        }
    }
}
