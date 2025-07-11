using BeltainsTools.Utilities;
using UnityEngine;

namespace BeltainsTools
{
    public static class CameraExtensions
    {
        /// <inheritdoc cref="CameraUtilities.SetAspectRatio(Camera, float)"/>
        public static void SetAspectRatio(this Camera camera, float targetAspect)
            => CameraUtilities.SetAspectRatio(camera, targetAspect);

        /// <inheritdoc cref="CameraUtilities.SetResolution(Camera, float, float)"/>
        public static void SetResolution(Camera camera, float width, float height)
            => CameraUtilities.SetResolution(camera, width, height);

        /// <inheritdoc cref="CameraUtilities.CaptureScreenshot(Camera, System.Action{Sprite})"/>
        public static void CaptureScreenshot(this Camera cam, System.Action<Sprite> onCapturedCallback)
            => CameraUtilities.CaptureScreenshot(cam, onCapturedCallback);

        /// <inheritdoc cref="CameraUtilities.CaptureScreenshot(Camera, System.Action{Texture2D})"/>
        public static void CaptureScreenshot(this Camera cam, System.Action<Texture2D> onCapturedCallback)
            => CameraUtilities.CaptureScreenshot(cam, onCapturedCallback);

        /// <inheritdoc cref="CameraUtilities.GetDistanceFromCameraPlane(Camera, Transform)"/>
        public static float GetDistanceFromCameraPlane(this Camera cam, Transform transform)
            => CameraUtilities.GetDistanceFromCameraPlane(cam, transform.position);

        /// <inheritdoc cref="CameraUtilities.GetDistanceFromCameraPlane(Camera, Vector3)"/>
        public static float GetDistanceFromCameraPlane(this Camera cam, Vector3 worldPos)
            => CameraUtilities.GetDistanceFromCameraPlane(cam, worldPos);

        /// <inheritdoc cref="CameraUtilities.GetScreenBoundsForTargetWorldBounds{T}(Camera, T)"/>
        public static UnityEngine.Bounds GetScreenBoundsForTargetWorldBounds<T>(this Camera cam, T target) where T : Component
            => CameraUtilities.GetScreenBoundsForTargetWorldBounds(cam, target);

        /// <inheritdoc cref="CameraUtilities.GetScreenBoundsForWorldBounds(Camera, Bounds)"/>
        public static UnityEngine.Bounds GetScreenBoundsForWorldBounds(this Camera cam, UnityEngine.Bounds worldObjectBounds)
            => CameraUtilities.GetScreenBoundsForWorldBounds(cam, worldObjectBounds);

        /// <inheritdoc cref="CameraUtilities.GetScreenBoundsForWorldBounds(Camera, Bounds, ref Bounds)"/>
        public static void GetScreenBoundsForWorldBounds(this Camera cam, UnityEngine.Bounds worldObjectBounds, ref UnityEngine.Bounds outBounds)
            => CameraUtilities.GetScreenBoundsForWorldBounds(cam, worldObjectBounds, ref outBounds);

        /// <inheritdoc cref="CameraUtilities.GetScreenBoundsForTargetMeshes{T}(Camera, T)"/>
        public static UnityEngine.Bounds GetScreenBoundsForTargetMeshes<T>(this Camera cam, T target) where T : Component
            => CameraUtilities.GetScreenBoundsForTargetMeshes<T>(cam, target);

        /// <inheritdoc cref="CameraUtilities.GetScreenBoundsForMeshFilter(Camera, MeshFilter)"/>
        public static UnityEngine.Bounds GetScreenBoundsForMeshFilter(this Camera cam, MeshFilter meshFilter)
            => CameraUtilities.GetScreenBoundsForMeshFilter(cam, meshFilter);

        /// <inheritdoc cref="CameraUtilities.GetScreenBoundsForMeshFilter(Camera, MeshFilter, ref Bounds)"/>
        public static void GetScreenBoundsForMeshFilter(this Camera cam, MeshFilter meshFilter, ref UnityEngine.Bounds outBounds)
            => CameraUtilities.GetScreenBoundsForMeshFilter(cam, meshFilter, ref outBounds);
    }
}
