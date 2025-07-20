using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class ComponentUtilities
    {
        /// <summary>Get the renderer bounds for a given target and all its children</summary>
        public static UnityEngine.Bounds GetRendererBoundsAcrossChildren<T>(T origin) where T : Component
        {
            Renderer[] targetRenderers = origin.GetComponentsInChildren<Renderer>();
            UnityEngine.Bounds resultBounds = default;
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                if (i == 0)
                {
                    resultBounds = targetRenderers[i].bounds;
                    continue;
                }

                resultBounds.Encapsulate(targetRenderers[i].bounds);
            }
            return resultBounds;
        }

        public static T GetComponentInParents<T>(Component origin, int maxIterations = 100)
        {
            T result;
            Transform currentObject = origin.transform;
            for (int i = 0; i < maxIterations; i++)
            {
                bool hasComponent = currentObject.TryGetComponent<T>(out result);
                if (hasComponent)
                    return result;
                if (currentObject.parent == null)
                    return default;
                currentObject = currentObject.parent;
            }
            return default;
        }
    }
}
