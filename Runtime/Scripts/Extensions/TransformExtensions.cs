using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class TransformExtensions
    {
        /// <inheritdoc cref="TransformUtilities.InverseTransformRotation(Transform, Quaternion)"/>
        public static Quaternion InverseTransformRotation(this Transform transform, Quaternion worldRotation)
            => TransformUtilities.InverseTransformRotation(transform, worldRotation);

        /// <inheritdoc cref="TransformUtilities.TransformRotation(Transform, Quaternion)"/>
        public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
            => TransformUtilities.TransformRotation(transform, localRotation);

        /// <inheritdoc cref="TransformUtilities.CreateChild(Transform, string, System.Type[])"/>
        public static Transform CreateChild(this Transform transform, string objectName, params System.Type[] components)
            => TransformUtilities.CreateChild(transform, objectName, components);
    }
}
