using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class GameObjectExtensions
    {
        /// <inheritdoc cref="GameObjectUtilities.SetLayer(GameObject, int, bool)"/>
        public static void SetLayer(this GameObject gameObject, int layerIndex, bool includeChildren = false)
            => GameObjectUtilities.SetLayer(gameObject, layerIndex, includeChildren);

        /// <inheritdoc cref="ComponentUtilities.GetComponentInParents{T}(Component, int)"/>
        public static T GetComponentInParents<T>(this GameObject gameObject, int maxIterations = 100) where T : Component
            => ComponentUtilities.GetComponentInParents<T>(gameObject.transform, maxIterations);

        /// <inheritdoc cref="TransformUtilities.GetFullHierarchyString(Transform)"/>
        public static string GetFullHierarchyString(this GameObject gameObject)
            => TransformUtilities.GetFullHierarchyString(gameObject.transform);
    }
}
