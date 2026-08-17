using System;
using UnityEngine;

namespace BeltainsTools
{
    public static class GuidExtensions
    {
        public static SerialisableGuid ToSerialisableGUID(this Guid systemGUID)
        {
            return new SerialisableGuid(systemGUID);
        }
    }
}
