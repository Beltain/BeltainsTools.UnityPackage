using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Editor.Debugging.DebugPages
{
    public class DebugPagesSessionData : ScriptableObject
    {
        public List<UnityEngine.Object> TrackedObjects = new List<UnityEngine.Object>();
    }
}
