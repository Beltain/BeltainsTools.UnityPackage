using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class RectTransformUtilities
    {
        /// <summary>Convert the <paramref name="screenPoint"/> to a point on the <paramref name="rectTransform"/> 
        /// representing the percentage X and Y that the point falls at.</summary>
        /// <returns>Whether we've hit the rect at all or not.</returns>
        /// <param name="camera">From which camera are we sampling? For points on a screenspace overlay canvas, leave null.</param>
        public static bool ScreenPointToRectXY01(RectTransform rectTransform, Vector2 screenPoint, out Vector2 xy01, Camera camera = null)
        {
            bool hit = ScreenPointToRectUV(rectTransform, screenPoint, out xy01, camera);
            xy01 = xy01.SetY(1f - xy01.y); // flip to rectTransfrom XY space
            return hit;
        }

        /// <summary>Convert the <paramref name="screenPoint"/> to a UV point on the <paramref name="rectTransform"/></summary>
        /// <returns>Whether we've hit the rect at all or not.</returns>
        /// <param name="camera">From which camera are we sampling? For points on a screenspace overlay canvas, leave null.</param>
        public static bool ScreenPointToRectUV(RectTransform rectTransform, Vector2 screenPoint, out Vector2 uvPoint, Camera camera = null)
        {
            uvPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                screenPoint, camera, out Vector2 localPointHit))
            {
                // Convert localPoint to normalized UV coordinates
                uvPoint.x = (localPointHit.x + (rectTransform.rect.width * 0.5f)) / rectTransform.rect.width;
                uvPoint.y = 1f - ((localPointHit.y + (rectTransform.rect.height * 0.5f)) / rectTransform.rect.height); // flip to UV space

                return new Rect(Vector2.zero, Vector2.one).Contains(uvPoint); // return whether we're in UV bounds 0 -> 1
            }
            else
            {
                return false;
            }
        }
    }
}
