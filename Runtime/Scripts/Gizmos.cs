using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Editor
{
    public static class Bizmos
    {
        static Stack<Color> ColorCache = new Stack<Color>();

        static void CacheAndSetColor(Color color)
        {
            ColorCache.Push(Gizmos.color);
            
            if(color != default)
                Gizmos.color = color;
        }

        static void RestoreColor()
        {
            Gizmos.color = ColorCache.Pop();
        }



        public static void DrawLine(Vector3[] connectingVertices, Color color = default)
        {
            for (int i = 0; i < connectingVertices.Length - 1; i++)
                DrawLine(connectingVertices[i], connectingVertices[i + 1], color);
        }
        public static void DrawLine(List<Vector3> connectingVertices, Color color = default)
        {
            for (int i = 0; i < connectingVertices.Count - 1; i++)
                DrawLine(connectingVertices[i], connectingVertices[i + 1], color);
        }

        public static void DrawLine(Vector3 startPos, Vector3 endPos, Color color = default)
        {
            CacheAndSetColor(color);
            Gizmos.DrawLine(startPos, endPos);
            RestoreColor();
        }


        public static void DrawWireCylinder(float radius, float length, Vector3 center, Vector3 normal, Color color = default, int segmentsCount = 16)
        {
            CacheAndSetColor(color);

            Quaternion normalRotation = Quaternion.LookRotation(normal);

            Vector3[] farVertices = new Vector3[segmentsCount];
            Vector3[] nearVertices = new Vector3[segmentsCount];
            float segmentAngle = 360f / segmentsCount;
            float halfLength = length / 2f;
            for (int i = 0; i < farVertices.Length; i++)
            {
                float angle = ((i * segmentAngle) * Mathf.PI) / 180f;
                farVertices[i] = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0f);
                nearVertices[i] = farVertices[i];
                farVertices[i] = center + (normalRotation * (farVertices[i] + (Vector3.forward * halfLength)));
                nearVertices[i] = center + (normalRotation * (nearVertices[i] - (Vector3.forward * halfLength)));
            }

            for (int i = 0; i < farVertices.Length; i++)
            {
                if (i == 0)
                {
                    Gizmos.DrawLine(farVertices[0], farVertices[farVertices.Length - 1]);
                    Gizmos.DrawLine(nearVertices[0], nearVertices[nearVertices.Length - 1]);
                }
                else
                {
                    Gizmos.DrawLine(farVertices[i], farVertices[i - 1]);
                    Gizmos.DrawLine(nearVertices[i], nearVertices[i - 1]);
                }

                Gizmos.DrawLine(farVertices[i], nearVertices[i]);
            }

            RestoreColor();
        }

        public static void DrawWireConeFromPoint(float length, float angleDegrees, Vector3 pointOrigin, Vector3 direction, Color color = default, int segmentsCount = 16)
        {
            float angleRadians = Mathf.PI * angleDegrees / 180f;
            float radius = length * Mathf.Sin(angleRadians / 2.0f);
            Vector3 center = pointOrigin + (direction * length * 0.5f);
            DrawWireCone(radius, length, center, direction, color, segmentsCount);
        }
        public static void DrawWireCone(float radius, float length, Vector3 center, Vector3 normal, Color color = default, int segmentsCount = 16)
        {
            CacheAndSetColor(color);

            Quaternion normalRotation = Quaternion.LookRotation(-normal);

            float segmentAngle = 360f / segmentsCount;
            float halfLength = length / 2f;
            Vector3[] farVertices = new Vector3[segmentsCount];
            Vector3 nearVertex = center + (normalRotation * Vector3.back * halfLength);
            for (int i = 0; i < farVertices.Length; i++)
            {
                float angle = ((i * segmentAngle) * Mathf.PI) / 180f;
                farVertices[i] = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0f);
                farVertices[i] = center + (normalRotation * (farVertices[i] + (Vector3.forward * halfLength)));
            }

            for (int i = 0; i < farVertices.Length; i++)
            {
                if (i == 0)
                {
                    Gizmos.DrawLine(farVertices[0], farVertices[farVertices.Length - 1]);
                }
                else
                {
                    Gizmos.DrawLine(farVertices[i], farVertices[i - 1]);
                }

                Gizmos.DrawLine(farVertices[i], nearVertex);
            }

            RestoreColor();
        }


        /// <summary>Draws the world bounds of the given target monobehaviour and its child renderers</summary>
        public static void DrawObjectRendererBounds<T>(this T target, Color color = default) where T : Component
        {
            UnityEngine.Bounds target3DBounds = target.GetRendererBoundsAcrossChildren();
            DrawBounds(target3DBounds, color);
        }

        public static void DrawBounds(this Bounds bounds, Color color = default)
            => DrawWireCube(bounds.size, bounds.center, Quaternion.identity, color);

        public static void DrawWireCube(Vector3 size, Vector3 center, Quaternion rotation, Color color = default)
        {
            CacheAndSetColor(color);

            Vector3 halfDims = size / 2f;

            Vector2 faceSize = new Vector2(size.x, size.z);
            DrawWireSquare(faceSize, center + (rotation * (Vector3.up * halfDims.y)), rotation * Vector3.up, rotation * Vector3.forward);
            DrawWireSquare(faceSize, center + (rotation * (Vector3.down * halfDims.y)), rotation * Vector3.down, rotation * Vector3.forward);

            faceSize = new Vector2(size.x, size.y);
            DrawWireSquare(faceSize, center + (rotation * (Vector3.forward * halfDims.z)), rotation * Vector3.forward, rotation * Vector3.up);
            DrawWireSquare(faceSize, center + (rotation * (Vector3.back * halfDims.z)), rotation * Vector3.back, rotation * Vector3.up);

            faceSize = new Vector2(size.y, size.z);
            DrawWireSquare(faceSize, center + (rotation * (Vector3.right * halfDims.x)), rotation * Vector3.right, rotation * Vector3.forward);
            DrawWireSquare(faceSize, center + (rotation * (Vector3.left * halfDims.x)), rotation * Vector3.left, rotation * Vector3.forward);

            RestoreColor();
        }

        public static void DrawWireCircle(float radius, Vector3 center, Vector3 forward, Color color = default, int segmentsCount = 16) => DrawWireCircle(radius, center, Quaternion.LookRotation(forward), color, segmentsCount);
        public static void DrawWireCircle(float radius, Vector3 center, Vector3 normal, Vector3 forward, Color color = default, int segmentsCount = 16) => DrawWireCircle(radius, center, Quaternion.LookRotation(forward, normal), color, segmentsCount);
        public static void DrawWireCircle(float radius, Vector3 center, Quaternion normalRotation, Color color = default, int segmentsCount = 16) => DrawWireCircleSegment(360f, radius, center, normalRotation, color, segmentsCount);
        public static void DrawWireCircleSegment(float angle, float radius, Vector3 center, Vector3 normal, Vector3 forward, Color color = default, int segmentsCount = 16) => DrawWireCircleSegment(angle, radius, center, Quaternion.LookRotation(forward, normal), color, segmentsCount);
        public static void DrawWireCircleSegment(float angle, float radius, Vector3 center, Vector3 forward, Color color = default, int segmentsCount = 16) => DrawWireCircleSegment(angle, radius, center, Quaternion.LookRotation(forward), color, segmentsCount);
        public static void DrawWireCircleSegment(float angle, float radius, Vector3 center, Quaternion forwardRotation, Color color = default, int segmentsCount = 16)
        {
            CacheAndSetColor(color);

            angle = Mathf.Min(angle, 360f);
            float angleRemainder = 360f - angle; //what we're not drawing of a full circle

            Vector3[] vertices = new Vector3[segmentsCount];
            float segmentAngle = angle / (segmentsCount - 1);
            for (int i = 0; i < vertices.Length; i++)
            {
                float vertexAngle = (((i * segmentAngle) + (angleRemainder / 2f) - 90 /*Used to correct us to have the a forward direction*/) * Mathf.PI) / 180f;
                vertices[i] = new Vector3(radius * Mathf.Cos(vertexAngle), 0f, radius * Mathf.Sin(vertexAngle));
                vertices[i] = center + (forwardRotation * vertices[i]);
            }

            for (int i = 1; i < vertices.Length; i++)
            {   
                //connect angle vertices
                Gizmos.DrawLine(vertices[i], vertices[i - 1]);
            }

            if(angleRemainder != 0) //we're not a perfectly closed circle, so draw lines
            {
                Gizmos.DrawLine(vertices[0], center);
                Gizmos.DrawLine(vertices[vertices.Length - 1], center);
            }

            RestoreColor();
        }


        /// <summary>Draws the screen space bounds of the given target component and its child renderers' bounds. Warning cheaper than <see cref="DrawObjectMeshScreenSpaceBounds"/> but less accurate</summary>
        public static void DrawObjectRendererBoundsScreenSpaceBounds<T>(this T target, Color color = default) where T : Component
        {
            d.Assert(Camera.main != null && Camera.main.orthographic, "Trying to draw object renderer bounds but there is no valid camera!");
            Bounds targetBounds = Camera.main.GetScreenBoundsForTargetWorldBounds(target);
            DrawScreenWireSquare(targetBounds.min, targetBounds.max, color);
        }

        /// <summary>Draws the screen space bounds of the given target component and its child meshes. Warning heavier than <see cref="DrawObjectRendererBoundsScreenSpaceBounds"/> but more accurate</summary>
        public static void DrawObjectMeshScreenSpaceBounds<T>(this T target, Color color = default) where T : Component
        {
            d.Assert(Camera.main != null && Camera.main.orthographic, "Trying to draw object renderer bounds but there is no valid camera!");
            Bounds targetBounds = Camera.main.GetScreenBoundsForTargetMeshes(target);
            DrawScreenWireSquare(targetBounds.min, targetBounds.max, color);
        }

        /// <summary>Draw a debug square in screen coordinates</summary>
        public static void DrawScreenWireSquare(Vector2 bottomLeft, Vector2 topRight, Color color = default)
        {
            Vector3 cameraForward = Camera.main.transform.forward * (Camera.main.nearClipPlane * 1.1f);
            Vector3[] vertices = new Vector3[4];
            vertices[0] = Camera.main.ScreenToWorldPoint(bottomLeft) + cameraForward;
            vertices[1] = Camera.main.ScreenToWorldPoint(bottomLeft.SetY(topRight.y)) + cameraForward;
            vertices[2] = Camera.main.ScreenToWorldPoint(topRight) + cameraForward;
            vertices[3] = Camera.main.ScreenToWorldPoint(topRight.SetY(bottomLeft.y)) + cameraForward;

            DrawWireSquare(vertices, color);
        }

        public static void DrawWireSquare(Vector2 size, Vector3 center, Vector3 normal, Vector3 tangent, Color color = default)
        {
            DrawWireSquare(size, center, Quaternion.LookRotation(normal, tangent), color);
        }

        public static void DrawWireSquare(Vector2 size, Vector3 center, Quaternion rotation, Color color = default)
        {
            Vector2 halfDims = size / 2f;
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(halfDims.x, halfDims.y),
                new Vector3(halfDims.x, -halfDims.y),
                new Vector3(-halfDims.x, -halfDims.y),
                new Vector3(-halfDims.x, halfDims.y),
            };

            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = center + (rotation * vertices[i]);

            DrawWireSquare(vertices, color);
        }

        public static void DrawWireSquare(Vector3[] vertices, Color color = default)
        {
            CacheAndSetColor(color);

            Gizmos.DrawLine(vertices[0], vertices[1]);
            Gizmos.DrawLine(vertices[1], vertices[2]);
            Gizmos.DrawLine(vertices[2], vertices[3]);
            Gizmos.DrawLine(vertices[3], vertices[0]);

            RestoreColor();
        }
    }
}