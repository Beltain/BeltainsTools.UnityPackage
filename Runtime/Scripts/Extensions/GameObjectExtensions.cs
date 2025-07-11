using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class GameObjectExtensions
    {
        /// <inheritdoc cref="GameObjectUtilities.SetLayer(GameObject, int, bool)"/>
        public static void SetLayer(this GameObject gameObject, int layerIndex, bool includeChildren = false)
            => GameObjectUtilities.SetLayer(gameObject, layerIndex, includeChildren);
    }
}
