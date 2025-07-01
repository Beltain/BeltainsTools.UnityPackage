using System;
using UnityEngine;

namespace BeltainsTools
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EnumArrayAttribute : PropertyAttribute
    {
        public readonly Type m_EnumType;
        public readonly int m_FirstValue;
        public readonly int m_LastValueExclusive;

        public EnumArrayAttribute(Type enumType)
        {
            m_EnumType = enumType;
            m_FirstValue = int.MinValue;
            m_LastValueExclusive = int.MaxValue;
        }

        public EnumArrayAttribute(Type enumType, int firstValue, int lastValueInclusive = int.MaxValue)
        {
            Debug.Assert(lastValueInclusive > firstValue, "Invalid range specified!");

            m_EnumType = enumType;
            m_FirstValue = 0;
            m_LastValueExclusive = lastValueInclusive + 1;
        }
    }
}