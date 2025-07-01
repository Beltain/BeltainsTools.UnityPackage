using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace BeltainsTools
{
    public static class Extensions
    {
        #region Vector3
        public static Vector3 SetX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);
        public static Vector3 SetY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);
        public static Vector3 SetZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

        public static Vector3 GetX(this Vector3 vector) => new Vector3(vector.x, 0f, 0f);
        public static Vector3 GetY(this Vector3 vector) => new Vector3(0f, vector.y, 0f);
        public static Vector3 GetZ(this Vector3 vector) => new Vector3(0f, 0f, vector.z);

        public static Vector2 ToVector2XY(this Vector3 vector) => new Vector2(vector.x, vector.y);
        public static Vector2 ToVector2XZ(this Vector3 vector) => new Vector2(vector.x, vector.z);
        public static Vector2 ToVector2YZ(this Vector3 vector) => new Vector2(vector.y, vector.z);
        public static Vector2 ToVector2ZY(this Vector3 vector) => new Vector2(vector.z, vector.y);
        public static Vector2 ToVector2ZX(this Vector3 vector) => new Vector2(vector.z, vector.x);
        public static Vector2 ToVector2YX(this Vector3 vector) => new Vector2(vector.y, vector.x);

        public static bool IsNegativeInfinity(this Vector3 vector) => vector.x.IsNegativeInfinity() || vector.y.IsNegativeInfinity() || vector.z.IsNegativeInfinity();
        public static bool IsPositiveInfinity(this Vector3 vector) => vector.x.IsPositiveInfinity() || vector.y.IsPositiveInfinity() || vector.z.IsPositiveInfinity();
        public static bool IsInfinity(this Vector3 vector) => vector.x.IsInfinity() || vector.y.IsInfinity() || vector.z.IsInfinity();

        /// <summary>Translate this vector so that it's relative to the camera forward</summary>
        public static Vector3 RelativeToCameraGroundForward(this Vector3 vector, Camera cam = null)
        {
            if (cam == null)
                cam = Camera.main;
            Quaternion rotationToCameraForward = Quaternion.FromToRotation(Vector3.forward, cam.transform.forward.SetY(0).normalized);
            return rotationToCameraForward * vector;
        }
        #endregion

        #region Vector2
        /// <summary>Cheeky func. A shorthand that interperates the Vector2 as a Range.</summary>
        public static float Random(this Vector2 vector2) => UnityEngine.Random.Range(vector2.x, vector2.y);

        /// <summary>Cheeky func. Determines whether the provided Vector2 'range' contains at least some of the other Vector2 'range'</summary>
        public static bool RangeOverlaps(this Vector2 thisRange, Vector2 otherRange) => thisRange.RangeContains(otherRange.x) || thisRange.RangeContains(otherRange.y);
        /// <summary>Cheeky func. Determines whether the provided Vector2 'range' contains the whole of the other Vector2 'range'</summary>
        public static bool RangeContains(this Vector2 thisRange, Vector2 otherRange) => thisRange.RangeContains(otherRange.x) && thisRange.RangeContains(otherRange.y);
        /// <summary>Cheeky func. Determines whether the provided Vector2 'range' contains the given value</summary>
        public static bool RangeContains(this Vector2 range, float value) => range.x > range.y ? (value <= range.x && value >= range.y) : (value <= range.y && value >= range.x);

        public static float Lerp(this Vector2 vector2, float t) => UnityEngine.Mathf.Lerp(vector2.x, vector2.y, t);
        public static float InverseLerp(this Vector2 vector2, float value) => UnityEngine.Mathf.InverseLerp(vector2.x, vector2.y, value);

        public static Vector2 SetX(this Vector2 vector, float x) => new Vector2(x, vector.y);
        public static Vector2 SetY(this Vector2 vector, float y) => new Vector2(vector.x, y);

        public static Vector3 ToVector3XY(this Vector2 vector) => new Vector3(vector.x, vector.y, 0f);
        public static Vector3 ToVector3XZ(this Vector2 vector) => new Vector3(vector.x, 0f, vector.y);
        public static Vector3 ToVector3YZ(this Vector2 vector) => new Vector3(0f, vector.x, vector.y);
        #endregion

        #region Camera

        public static void SetAspectRatio(this Camera camera, float targetAspect)
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

        public static void SetResolution(this Camera camera, float width, float height)
        {
            Rect rect = camera.rect;

            rect.height = height / Screen.height;
            rect.width = width / Screen.width;
            rect.x = (1.0f - rect.width) / 2f;
            rect.y = (1.0f - rect.height) / 2f;

            camera.rect = rect;
        }

        public static void CaptureScreenshot(this Camera cam, System.Action<Sprite> onCapturedCallback)
        {
            ScreenshotCapturer.CaptureScreenshotWithCamera(cam, onCapturedCallback);
        }

        public static void CaptureScreenshot(this Camera cam, System.Action<Texture2D> onCapturedCallback)
        {
            ScreenshotCapturer.CaptureScreenshotWithCamera(cam, onCapturedCallback);
        }

        public static float DistanceFromCameraPlane(this Camera cam, Transform transform) => DistanceFromCameraPlane(cam, transform.position);
        public static float DistanceFromCameraPlane(this Camera cam, Vector3 worldPos)
        {
            Plane cameraPlane = new Plane();
            cameraPlane.SetNormalAndPosition(cam.transform.forward, cam.transform.position);
            return cameraPlane.GetDistanceToPoint(worldPos);
        }

        /// <summary>Get the screen bounds for a given target and all its children using each renderer object's bounds. This is a cheaper but less accurate version of <see cref="GetScreenBoundsForTargetMeshes"/></summary>
        public static UnityEngine.Bounds GetScreenBoundsForTargetWorldBounds<T>(this Camera cam, T target) where T : Component
        {
            UnityEngine.Bounds target3DBounds = target.GetRendererBoundsAcrossChildren();

            if (target3DBounds == default)
                return default;

            UnityEngine.Bounds resultBounds = new UnityEngine.Bounds();
            GetScreenBoundsForWorldBounds(cam, target3DBounds, ref resultBounds);

            return resultBounds;
        }

        public static UnityEngine.Bounds GetScreenBoundsForWorldBounds(this Camera cam, UnityEngine.Bounds worldObjectBounds)
        {
            UnityEngine.Bounds outBounds = new UnityEngine.Bounds();
            GetScreenBoundsForWorldBounds(cam, worldObjectBounds, ref outBounds);
            return outBounds;
        }

        static Vector3[] _s_WorldBoundsCorners = new Vector3[8];
        public static void GetScreenBoundsForWorldBounds(this Camera cam, UnityEngine.Bounds worldObjectBounds, ref UnityEngine.Bounds outBounds)
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
        public static UnityEngine.Bounds GetScreenBoundsForTargetMeshes<T>(this Camera cam, T target) where T : Component
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

        public static UnityEngine.Bounds GetScreenBoundsForMeshFilter(this Camera cam, MeshFilter meshFilter)
        {
            UnityEngine.Bounds outBounds = new UnityEngine.Bounds();
            GetScreenBoundsForMeshFilter(cam, meshFilter, ref outBounds);
            return outBounds;
        }

        public static void GetScreenBoundsForMeshFilter(this Camera cam, MeshFilter meshFilter, ref UnityEngine.Bounds outBounds)
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

        #endregion

        #region Rect
        public static Rect Encapsulate(this Rect rect, Vector2 point)
        {
            float xMin = Mathf.Min(rect.xMin, point.x);
            float yMin = Mathf.Min(rect.yMin, point.y);
            float xMax = Mathf.Max(rect.xMax, point.x);
            float yMax = Mathf.Max(rect.yMax, point.y);

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }
        #endregion

        #region RectTransform

        public static void SetRectTransformParent(this RectTransform rectTransform, RectTransform uIParentTransform) => SetRectTransformParent(rectTransform, uIParentTransform, false);
        public static void SetRectTransformParent(this RectTransform rectTransform, RectTransform uIParentTransform, bool worldPositionStays)
        {
            rectTransform.position = worldPositionStays ? rectTransform.position : uIParentTransform.position;
            
            // v Was the recommended functionality according to stack overflow, but seems to just break configuration of anchors with no clear purpose?
            //rectTransform.anchorMin = Vector2.zero;
            //rectTransform.anchorMax = Vector2.one;
            //rectTransform.pivot = Vector2.one / 2f;

            //rectTransform.sizeDelta = uIParentTransform.rect.size;
            rectTransform.SetParent(uIParentTransform.transform, worldPositionStays);
        }

        public static void SetAnchorsStretched(this RectTransform rectTransform, RectTransform parent = null)
        {
            if(parent != null)
                rectTransform.SetParent(parent);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }

        public static Vector3 GetWorldCenter(this RectTransform rectTransform)
        {
            Rect rect = rectTransform.rect;
            Vector3 pivotToCenter = (rect.size * 0.5f) - new Vector2(rect.width * rectTransform.pivot.x, rect.height * rectTransform.pivot.y);
            Vector3 worldCenter = rectTransform.position + pivotToCenter;
            return worldCenter;
        }

        #endregion

        #region Transform
        /// <summary>Get the local rotation for the given world rotation in the context of this transform</summary>
        public static Quaternion InverseTransformRotation(this Transform transform, Quaternion worldRotation)
        {
            return Quaternion.Inverse(transform.rotation) * worldRotation;
        }

        /// <summary>Get the world rotation for the given local rotation under the given transform</summary>
        public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
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
        public static Transform CreateChild(this Transform transform, string objectName, params System.Type[] components)
        {
            Transform newChild = new GameObject(objectName, components).transform;
            newChild.SetParent(transform);
            newChild.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            return newChild;
        }
        #endregion

        #region GameObject
        public static void SetLayer(this GameObject gameObject, int layerIndex, bool includeChildren = false)
        {
            gameObject.layer = layerIndex;
            if (includeChildren)
            {
                foreach (Transform child in gameObject.transform)
                {
                    child.gameObject.SetLayer(layerIndex, true);
                }
            }
        }
        #endregion

        #region Component

        /// <summary>Get the renderer bounds for a given target and all its children</summary>
        public static UnityEngine.Bounds GetRendererBoundsAcrossChildren<T>(this T origin) where T : Component
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

        public static T GetComponentInParents<T>(this Component origin, int maxIterations = 100)
        {
            T result;
            Transform currentObject = origin.transform;
            for (int i = 0; i < maxIterations; i++)
            {
                result = currentObject.GetComponent<T>();
                if (result != null)
                    return result;
                if (currentObject.parent == null)
                    return default;
                currentObject = currentObject.parent;
            }
            return default;
        }

        #endregion

        #region Type

        public static string ToTypeIdentifierString(this System.Type type) //used to be a GUID thing, didn't work out
        {
            return $"Type.{type.Namespace}.{type.Name}";
        }

        #endregion

        #region String

        public static bool IsEmpty(this string str)
        {
            return str == string.Empty;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return str == null || str == string.Empty;
        }

        #endregion

        #region TextMeshPro
        public static Vector3 GetCharacterPosition(this TMPro.TMP_InputField inputField, int index) => GetCharacterPosition(inputField.textComponent, index);
        public static Vector3 GetCharacterPosition(this TMPro.TMP_Text textElement, int index)
        {
            TMPro.TMP_TextInfo textInfo = textElement.GetTextInfo(textElement.text);
            return textElement.transform.position + textInfo.characterInfo[index].bottomLeft;
        }
        #endregion

        #region Float
        public static bool IsNegativeInfinity(this float value) => float.IsNegativeInfinity(value);
        public static bool IsPositiveInfinity(this float value) => float.IsPositiveInfinity(value);
        public static bool IsInfinity(this float value) => float.IsInfinity(value);

        public static bool Approximately(this float thisFloat, float value, float epsilon = 0.0001f)
        {
            return Mathf.Abs(thisFloat - value) < epsilon;
        }
        #endregion

        #region NavMeshPath
        public static float GetPathLength(this NavMeshPath path)
        {
            float length = 0f;
            for (int i = 1; i < path.corners.Length; i++)
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            return length;
        }
        #endregion

        #region NavMeshAgent
        public static bool TryGetPathTo(this NavMeshAgent agent, Vector3 targetPos, out NavMeshPath path, float maxDistanceTolerance = -1f, int areaMask = NavMesh.AllAreas)
        {
            path = new NavMeshPath();

            NavMeshQueryFilter filter = new NavMeshQueryFilter()
            {
                agentTypeID = agent.agentTypeID,
                areaMask = areaMask
            };

            if(!NavMesh.CalculatePath(agent.transform.position, targetPos, filter, path))
                return false;

            if (maxDistanceTolerance != -1 && Vector3.Distance(path.corners[path.corners.Length - 1], targetPos) > maxDistanceTolerance)
                return false;

            return true;
        }
        #endregion

        #region Animation
        public static void StopAndReset(this Animation anim, bool sampleFromStart = true)
        {
            foreach (AnimationState animState in anim)
            {
                if (animState.clip == anim.clip)
                {
                    animState.time = sampleFromStart ? 0f : 1f;
                    anim.Play();
                    anim.Sample();
                    break;
                }
            }
            anim.Stop();
        }
        #endregion

        #region EventTrigger 
        public static void AddListener<T>(this EventTrigger trigger, EventTriggerType type, System.Action<T> listener) where T : BaseEventData
        {
            UnityEngine.Events.UnityAction<BaseEventData> callback = (data) => listener.Invoke((T)data);

            // Ensure entry
            EventTrigger.Entry entry = trigger.triggers.FirstOrDefault(r => r.eventID == type);
            if (entry == null)
            {
                entry = new EventTrigger.Entry() { eventID = type };
                trigger.triggers.Add(entry);
            }

            // Add listener
            entry.callback.AddListener(callback);
        }

        public static void RemoveListener<T>(this EventTrigger trigger, EventTriggerType type, System.Action<T> listener) where T : BaseEventData
        {
            UnityEngine.Events.UnityAction<BaseEventData> callback = (data) => listener.Invoke((T)data);

            // Remove from every existing entry with the corresponding type
            foreach (EventTrigger.Entry triggerEntry in trigger.triggers.Where(r => r.eventID == type && r.callback.GetPersistentEventCount() > 0))
            {
                triggerEntry.callback.RemoveListener(callback);
            }
        }
        #endregion
    }

    public static class AnimationCurveExtensions
    {
        public static AnimationCurve EaseIn(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            return new AnimationCurve(
                new Keyframe(timeStart, valueStart, 0f, 0f),
                new Keyframe(timeEnd, valueEnd, 2f, 2f)
            );
        }

        public static AnimationCurve EaseOut(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            return new AnimationCurve(
                new Keyframe(timeStart, valueStart, 2f, 2f),
                new Keyframe(timeEnd, valueEnd, 0f, 0f)
            );
        }
    }
}