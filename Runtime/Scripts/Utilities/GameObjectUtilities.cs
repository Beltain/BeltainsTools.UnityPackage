using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class GameObjectUtilities
    {
        public static void SetLayer(GameObject gameObject, int layerIndex, bool includeChildren = false)
        {
            gameObject.layer = layerIndex;
            if (includeChildren)
            {
                foreach (Transform child in gameObject.transform)
                {
                    SetLayer(child.gameObject, layerIndex, true);
                }
            }
        }
    }
}
