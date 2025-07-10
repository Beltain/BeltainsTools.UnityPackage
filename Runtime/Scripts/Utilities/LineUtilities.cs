using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class LineUtilities
    {
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
