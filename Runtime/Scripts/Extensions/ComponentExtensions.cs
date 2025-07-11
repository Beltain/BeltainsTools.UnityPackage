using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class ComponentExtensions
    {
        /// <inheritdoc cref="ComponentUtilities.GetRendererBoundsAcrossChildren{T}(T)"/>
        public static UnityEngine.Bounds GetRendererBoundsAcrossChildren<T>(this T origin) where T : Component
            => ComponentUtilities.GetRendererBoundsAcrossChildren(origin);

        /// <inheritdoc cref="ComponentUtilities.GetComponentInParents{T}(Component, int)"/>
        public static T GetComponentInParents<T>(this Component origin, int maxIterations = 100)
            => ComponentUtilities.GetComponentInParents<T>(origin, maxIterations);
    }
}
