using BeltainsTools.Utilities;
using UnityEngine;

namespace BeltainsTools
{
    public static class RectTransformExtensions
    {
        /// <inheritdoc cref="RectTransformUtilities.SetRectTransformParent(RectTransform, RectTransform)"/>
        public static void SetRectTransformParent(this RectTransform rectTransform, RectTransform uIParentTransform) 
            => RectTransformUtilities.SetRectTransformParent(rectTransform, uIParentTransform);

        /// <inheritdoc cref="RectTransformUtilities.SetRectTransformParent(RectTransform, RectTransform, bool)"/>
        public static void SetRectTransformParent(this RectTransform rectTransform, RectTransform uIParentTransform, bool worldPositionStays)
            => RectTransformUtilities.SetRectTransformParent(rectTransform, uIParentTransform, worldPositionStays);

        /// <inheritdoc cref="RectTransformUtilities.SetAnchorsStretched(RectTransform, RectTransform)"/>
        public static void SetAnchorsStretched(this RectTransform rectTransform, RectTransform parent = null)
            => RectTransformUtilities.SetAnchorsStretched(rectTransform, parent);

        /// <inheritdoc cref="RectTransformUtilities.GetWorldCenter(RectTransform)"/>
        public static Vector3 GetWorldCenter(this RectTransform rectTransform)
            => RectTransformUtilities.GetWorldCenter(rectTransform);
    }
}
