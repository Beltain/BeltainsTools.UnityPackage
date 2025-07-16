using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class LineUtilities
    {
        public static bool GetLinesIntersection(Vector2 lineAStart, Vector2 lineAEnd, Vector2 lineBStart, Vector2 lineBEnd, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            float A1 = lineAEnd.y - lineAStart.y;
            float B1 = lineAStart.x - lineAEnd.x;
            float C1 = A1 * lineAStart.x + B1 * lineAStart.y;

            float A2 = lineBEnd.y - lineBStart.y;
            float B2 = lineBStart.x - lineBEnd.x;
            float C2 = A2 * lineBStart.x + B2 * lineBStart.y;

            float denominator = A1 * B2 - A2 * B1;

            if (denominator.Approximately(0f))
            {
                // Lines are parallel (or coincident)
                return false;
            }

            float x = (B2 * C1 - B1 * C2) / denominator;
            float y = (A1 * C2 - A2 * C1) / denominator;

            intersection = new Vector2(x, y);
            return true;
        }

        public static bool GetAreDirectionsParallel(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB)
        {
            float denominator = directionA.x * directionB.y - directionA.y * directionB.x;
            return denominator.Approximately(0f); // If the denominator is approximately zero, the directions are parallel
        }

        public static bool GetDirectionsIntersection(Vector2 originA, Vector2 directionA, Vector2 originB, Vector2 directionB, out Vector2 intersection, bool ensureCrossesInDirection = false)
        {
            intersection = Vector2.zero;

            if (GetAreDirectionsParallel(originA, directionA, originB, directionB))
                return false;

            float denominator = directionA.x * directionB.y - directionA.y * directionB.x;
            Vector2 diff = originB - originA;
            float t = (diff.x * directionB.y - diff.y * directionB.x) / denominator;

            intersection = originA + t * directionA;

            if (ensureCrossesInDirection)
            {
                // make sure our intersection is actually in the directions provided
                Vector2 aToI = (intersection - originA).normalized;
                Vector2 bToI = (intersection - originB).normalized;
                return Vector2.Dot(aToI, directionA) >= 0f && Vector2.Dot(bToI, directionB) >= 0f;
            }
            else
            {
                return true;
            }
        }

        public static float GetLineLength(IEnumerable<Vector3> lineVertices)
        {
            float lineLength = 0f;
            for (int i = 1; i < lineVertices.Count(); i++)
                lineLength += Vector3.Distance(lineVertices.ElementAt(i - 1), lineVertices.ElementAt(i));
            return lineLength;
        }

        /// <summary>Sample a point on a line</summary>
        public static Vector3 SampleLinePointAtDistance(IEnumerable<Vector3> lineVertices, float distanceAlongLine)
        {
            SampleLineAtDistance(lineVertices, distanceAlongLine, out Vector3 result, out Vector3 _);
            return result;
        }

        /// <summary>Sample a direction (along the line from the start vertex upwards) of the line segment at a distance along the line</summary>
        public static Vector3 SampleLineDirectionAtDistance(IEnumerable<Vector3> lineVertices, float distanceAlongLine)
        {
            SampleLineAtDistance(lineVertices, distanceAlongLine, out Vector3 _, out Vector3 result);
            return result;
        }

        /// <summary>Sample a point on a line and the direction (along the line from the start vertex upwards) of the line segment at that point</summary>
        public static void SampleLineAtDistance(IEnumerable<Vector3> lineVertices, float distanceAlongLine, out Vector3 sampledPoint, out Vector3 sampledDirection)
        {
            float distanceWalked = 0f;
            Vector3 lastVert = Vector3.negativeInfinity;
            Vector3 lastDirection = Vector3.negativeInfinity;
            foreach (Vector3 curVert in lineVertices)
            {
                if (!lastVert.IsNegativeInfinity())
                {
                    lastDirection = curVert - lastVert;
                    float distanceBetweenVerts = Vector3.Distance(lastVert, curVert);
                    if (distanceWalked + distanceBetweenVerts >= distanceAlongLine)
                    {
                        // our sample point lies within this line segment, so return it
                        sampledPoint = Vector3.Lerp(lastVert, curVert, (distanceAlongLine - distanceWalked) / distanceBetweenVerts);
                        sampledDirection = lastDirection;
                        return;
                    }
                    distanceWalked += distanceBetweenVerts;
                }
                lastVert = curVert;
            }

            // the sample distance exceeds the length of the line
            d.Assert(!lastVert.IsNegativeInfinity(), "Trying to sample point on line that contains no vertices! No!");
            sampledPoint = lastVert;
            sampledDirection = lastDirection;
            return;
        }


        public static void TrimLineFromEnd(IEnumerable<Vector3> lineVertices, float chopLengthFromEnd, out Vector3[] resultingLine)
            => SplitLine(lineVertices, GetLineLength(lineVertices) - chopLengthFromEnd, out resultingLine, out Vector3[] _);
        public static void TrimLineFromStart(IEnumerable<Vector3> lineVertices, float chopLengthFromStart, out Vector3[] resultingLine)
            => SplitLine(lineVertices, chopLengthFromStart, out Vector3[] _, out resultingLine);
        /// <summary>Split a line (collection of line verts) into 2 at the <paramref name="chopLength"/>, adding vertices at the split point</summary>
        public static void SplitLine(IEnumerable<Vector3> lineVertices, float chopLength, out Vector3[] splitLineA, out Vector3[] splitLineB)
            => SplitLine(lineVertices, chopLength, insertPreciseCutVerts: true, out splitLineA, out splitLineB);
        /// <summary>Split a line (collection of line verts) into 2 at the <paramref name="chopLength"/>, adding vertices at the split point if <paramref name="insertPreciseCutVerts"/> is true</summary>
        public static void SplitLine(IEnumerable<Vector3> lineVertices, float chopLength, bool insertPreciseCutVerts, out Vector3[] splitLineA, out Vector3[] splitLineB)
        {
            d.Assert(chopLength != 0f, "Trying to split a line at 0f along it, this operation is not allowed! You need to chop it somewhere along the line...");

            int originalLineVertCount = lineVertices.Count();

            float walkedLength = 0f;
            int walkedLineVertsCount = 0;
            Vector3 cutVert = Vector3.negativeInfinity;
            bool hasCutVert = false;

            //go through all verts and cut when we've walked far enough
            Vector3 lastVert = Vector3.negativeInfinity;
            foreach (Vector3 currentVert in lineVertices)
            {

                if (!lastVert.IsNegativeInfinity())
                {
                    // == per edge calculations ==
                    float lineEdgeLength = Vector3.Distance(lastVert, currentVert);

                    if (walkedLength + lineEdgeLength >= chopLength)
                    {
                        // = Do chop =
                        if (insertPreciseCutVerts)
                        {
                            hasCutVert = true;
                            cutVert = Vector3.Lerp(lastVert, currentVert, (chopLength - walkedLength) / lineEdgeLength);
                        }
                        break;
                    }

                    walkedLength += lineEdgeLength;
                }

                walkedLineVertsCount++;
                lastVert = currentVert;
            }

            // assign verts to the resulting split arrays
            splitLineA = new Vector3[walkedLineVertsCount + (hasCutVert ? 1 : 0)];
            splitLineB = new Vector3[originalLineVertCount - walkedLineVertsCount + (hasCutVert ? 1 : 0)];

            for (int i = 0; i < walkedLineVertsCount; i++)
            {
                splitLineA[i] = lineVertices.ElementAt(i);
            }
            if (hasCutVert)
            {
                splitLineA[splitLineA.Length - 1] = cutVert;
                splitLineB[0] = cutVert;
            }
            for (int i = (hasCutVert ? 1 : 0); i < splitLineB.Length; i++)
            {
                splitLineB[i] = lineVertices.ElementAt(walkedLineVertsCount + i - (hasCutVert ? 1 : 0));
            }
        }
    }
}
