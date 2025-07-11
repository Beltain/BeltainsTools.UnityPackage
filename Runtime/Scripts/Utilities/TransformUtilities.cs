using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class TransformUtilities
    {
        /// <summary>Get the local rotation for the given world rotation in the context of this transform</summary>
        public static Quaternion InverseTransformRotation(Transform transform, Quaternion worldRotation)
        {
            return Quaternion.Inverse(transform.rotation) * worldRotation;
        }

        /// <summary>Get the world rotation for the given local rotation under the given transform</summary>
        public static Quaternion TransformRotation(Transform transform, Quaternion localRotation)
        {
            return transform.rotation * localRotation;
        }

        /// <summary>
        /// Create a new gameobject that is childed under this parent
        /// </summary>
        /// <param name="transform">Transform under which the child is added</param>
        /// <param name="objectName">Name of the child gameobject</param>
        /// <param name="components">Components of the child gameobject</param>
        /// <returns>A child object created with the given properties that matches its parent's position and rotation exactly</returns>
        public static Transform CreateChild(Transform transform, string objectName, params System.Type[] components)
        {
            Transform newChild = new GameObject(objectName, components).transform;
            newChild.SetParent(transform);
            newChild.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            return newChild;
        }
    }
}
