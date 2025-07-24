using UnityEngine;

namespace BeltainsTools.Debugging
{
    public static class Tracking
    {
        static void Foo()
        {
            int value = 42;
            Track(value);
        }

        public static void Track(int value) => Track((TrackableInt)value);
        public static void Track(Trackable value)
        {
            // continue here
        }
    }

    public abstract class Trackable 
    {
        public abstract string GetReadout();
    }

    public abstract class Trackable<T> : Trackable where T : struct
    {
        protected T m_Value;
        public Trackable(T value) { m_Value = value; }
        public override string GetReadout() => m_Value.ToString();
    }

    public class TrackableInt : Trackable<int>
    {
        public TrackableInt(int value) : base(value) { }
        public static implicit operator TrackableInt(int value) => new(value);
    }
}
