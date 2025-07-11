using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class CameraUtilities
    {
        public static void SetAspectRatio(Camera camera, float targetAspect)
        {
            float windowAspect = (float)Screen.width / (float)Screen.height;
            float scaleHeight = windowAspect / targetAspect;
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            if (scaleHeight < 1.0f)
            {
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0f;
                rect.y = (1.0f - scaleHeight) / 2.0f;
            }
            else
            {
                rect.height = 1.0f;
                rect.width = scaleWidth;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0f;
            }

            camera.rect = rect;
        }

        public static void SetResolution(Camera camera, float width, float height)
        {
            Rect rect = camera.rect;

            rect.height = height / Screen.height;
            rect.width = width / Screen.width;
            rect.x = (1.0f - rect.width) / 2f;
            rect.y = (1.0f - rect.height) / 2f;

            camera.rect = rect;
        }

        public static void CaptureScreenshot(Camera cam, System.Action<Sprite> onCapturedCallback)
        {
            ScreenshotCapturer.CaptureScreenshotWithCamera(cam, onCapturedCallback);
        }

        public static void CaptureScreenshot(Camera cam, System.Action<Texture2D> onCapturedCallback)
        {
            ScreenshotCapturer.CaptureScreenshotWithCamera(cam, onCapturedCallback);
        }

        public static float GetDistanceFromCameraPlane(Camera cam, Transform transform) => GetDistanceFromCameraPlane(cam, transform.position);
        public static float GetDistanceFromCameraPlane(Camera cam, Vector3 worldPos)
        {
            Plane cameraPlane = new Plane();
            cameraPlane.SetNormalAndPosition(cam.transform.forward, cam.transform.position);
            return cameraPlane.GetDistanceToPoint(worldPos);
        }

        /// <summary>Get the screen bounds for a given target and all its children using each renderer object's bounds. This is a cheaper but less accurate version of <see cref="GetScreenBoundsForTargetMeshes"/></summary>
        public static UnityEngine.Bounds GetScreenBoundsForTargetWorldBounds<T>(Camera cam, T target) where T : Component
        {
            UnityEngine.Bounds target3DBounds = target.GetRendererBoundsAcrossChildren();

            if (target3DBounds == default)
                return default;

            UnityEngine.Bounds resultBounds = new UnityEngine.Bounds();
            GetScreenBoundsForWorldBounds(cam, target3DBounds, ref resultBounds);

            return resultBounds;
        }

        public static UnityEngine.Bounds GetScreenBoundsForWorldBounds(Camera cam, UnityEngine.Bounds worldObjectBounds)
        {
            UnityEngine.Bounds outBounds = new UnityEngine.Bounds();
            GetScreenBoundsForWorldBounds(cam, worldObjectBounds, ref outBounds);
            return outBounds;
        }

        static Vector3[] _s_WorldBoundsCorners = new Vector3[8];
        public static void GetScreenBoundsForWorldBounds(Camera cam, UnityEngine.Bounds worldObjectBounds, ref UnityEngine.Bounds outBounds)
        {
            Vector3 c = worldObjectBounds.center;
            Vector3 e = worldObjectBounds.extents;

            _s_WorldBoundsCorners[0] = new Vector3(c.x + e.x, c.y + e.y, c.z + e.z);
            _s_WorldBoundsCorners[1] = new Vector3(c.x + e.x, c.y + e.y, c.z - e.z);
            _s_WorldBoundsCorners[2] = new Vector3(c.x + e.x, c.y - e.y, c.z + e.z);
            _s_WorldBoundsCorners[3] = new Vector3(c.x + e.x, c.y - e.y, c.z - e.z);
            _s_WorldBoundsCorners[4] = new Vector3(c.x - e.x, c.y + e.y, c.z + e.z);
            _s_WorldBoundsCorners[5] = new Vector3(c.x - e.x, c.y + e.y, c.z - e.z);
            _s_WorldBoundsCorners[6] = new Vector3(c.x - e.x, c.y - e.y, c.z + e.z);
            _s_WorldBoundsCorners[7] = new Vector3(c.x - e.x, c.y - e.y, c.z - e.z);

            IEnumerable<Vector3> screenCorners = _s_WorldBoundsCorners.Select(corner => cam.WorldToScreenPoint(corner));
            outBounds.max = new Vector2(screenCorners.Max(corner => corner.x), screenCorners.Max(corner => corner.y));
            outBounds.min = new Vector2(screenCorners.Min(corner => corner.x), screenCorners.Min(corner => corner.y));
            //Debug.LogError($"Bounds: MIN MAX = {outBounds.min} | {outBounds.max} ___ CENTER = {outBounds.center} ___ EXTENTS = {outBounds.extents}");
        }

        /// <summary>Get the screen bounds for a given target and all its children using each object's meshes. This is a costlier but more accurate version of <see cref="GetScreenBoundsForTargetWorldBounds"/></summary>
        public static UnityEngine.Bounds GetScreenBoundsForTargetMeshes<T>(Camera cam, T target) where T : Component
        {
            MeshFilter[] targetMeshfilters = target.GetComponentsInChildren<MeshFilter>();

            Bounds iBounds = default;
            Bounds resultScreenBounds = default;
            foreach (MeshFilter meshFilter in targetMeshfilters)
            {
                GetScreenBoundsForMeshFilter(cam, meshFilter, ref iBounds);
                if (resultScreenBounds == default)
                {
                    resultScreenBounds = iBounds;
                    continue;
                }
                resultScreenBounds.Encapsulate(iBounds);
            }
            return resultScreenBounds;
        }

        public static UnityEngine.Bounds GetScreenBoundsForMeshFilter(Camera cam, MeshFilter meshFilter)
        {
            UnityEngine.Bounds outBounds = new UnityEngine.Bounds();
            GetScreenBoundsForMeshFilter(cam, meshFilter, ref outBounds);
            return outBounds;
        }

        public static void GetScreenBoundsForMeshFilter(Camera cam, MeshFilter meshFilter, ref UnityEngine.Bounds outBounds)
        {
            outBounds = default;
            d.Assert(meshFilter != null && meshFilter.sharedMesh != null, "Trying to get screen bounds from an invalid mesh filter! Please cleanse your data before trying to use GetScreenBoundsForMeshFilter!");

            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Vector2 minScreenPoint = Vector2.positiveInfinity;
            Vector2 maxScreenPoint = Vector2.negativeInfinity;

            foreach (Vector3 vertex in vertices)
            {
                Vector3 worldPosVertex = meshFilter.transform.TransformPoint(vertex);
                Vector3 worldVertOnScreen = cam.WorldToScreenPoint(worldPosVertex);
                minScreenPoint = Vector2.Min(minScreenPoint, worldVertOnScreen.ToVector2XY());
                maxScreenPoint = Vector2.Max(maxScreenPoint, worldVertOnScreen.ToVector2XY());
            }

            outBounds.max = maxScreenPoint;
            outBounds.min = minScreenPoint;
        }
    }
}
