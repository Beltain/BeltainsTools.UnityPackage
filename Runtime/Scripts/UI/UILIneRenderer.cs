using UnityEngine;
using UnityEngine.UI;

namespace BeltainsTools.UI
{
    public class UILineRenderer : Graphic
    {
        [SerializeField]
        Vector2[] m_Points = new Vector2[0];
        [SerializeField]
        float m_Thickness = 0.5f;
        [SerializeField, Tooltip("Number of segments to use when drawing corners, more segments = smoother corners")]
        int m_CornerDetail = 8; 


        ConnectingSegment[] m_ConnectingSegments;
        CornerSegment[] m_CornerSegments;

        bool IsValidLine => m_ConnectingSegments != null && m_ConnectingSegments.Length > 0;

        /// <summary>Structure containing the definition of the bit of the line between two points excluding any '<see cref="CornerSegment"/>s' and 'end-caps'</summary>
        struct ConnectingSegment 
        {
            public Vector2 Direction;
            public Vector2 StartL; 
            public Vector2 StartR;
            public Vector2 EndL;
            public Vector2 EndR;

            /// <summary>
            /// If true, the segment is too short and shouldn't be drawn.
            /// Simply a record of the start and end points to allow corners/end-caps to cover the rest
            /// </summary>
            public bool IsStub;

            public ConnectingSegment(Vector2 startPoint, Vector2 endPoint, float nodeRadius)
            {
                Vector2 fullDelta = endPoint - startPoint;
                Vector2 direction = fullDelta.normalized;

                float fullDistance = fullDelta.magnitude;

                float drawRadius = Mathf.Min(nodeRadius, fullDistance / 2f); // clamped radius away from nodes at which to draw segments
                Vector2 start = startPoint + direction * drawRadius;
                Vector2 end = endPoint - direction * drawRadius;

                Vector2 rOffset = direction.Perpendicularise(clockwise: true) * nodeRadius; // offset from the centre of the ends to give us our 4 corners

                Direction = direction;
                StartL = start - rOffset;
                StartR = start + rOffset;
                EndL = end - rOffset;
                EndR = end + rOffset;
                IsStub = fullDistance <= nodeRadius * 2f;
            }
        }

        /// <summary>Structure containing the definition of the bit of the line connecting two <see cref="ConnectingSegment"/>s around one of the <see cref="m_Points"/></summary>
        struct CornerSegment
        {
            /// <summary>Structure defining the line along the outside and inside of a corner segment</summary>
            public struct CornerLine
            {
                public Vector2 StartPt;
                public Vector2 EndPt;
                public Vector2 PolePt;

                /// <summary>If true, this line should be treated as a single point instead.</summary>
                public bool IsStub;

                public CornerLine(ref ConnectingSegment startSegment, ref ConnectingSegment endSegment, bool left, Vector2 cornerOrigin, float cornerRadius)
                {
                    ref Vector2 startSegmentEnd = ref (left ? ref startSegment.EndL : ref startSegment.EndR);
                    ref Vector2 endSegmentStart = ref (left ? ref endSegment.StartL : ref endSegment.StartR);
                    this = new CornerLine(ref startSegmentEnd, startSegment.Direction, ref endSegmentStart, -endSegment.Direction, cornerOrigin, cornerRadius);
                }

                public CornerLine(ref Vector2 startPoint, Vector2 startDirection, ref Vector2 endPoint, Vector2 endDirection, Vector2 cornerOrigin, float cornerRadius)
                {
                    StartPt = startPoint;
                    EndPt = endPoint;

                    // Try find the intersection of the two lines, if they intersect in both directions, then we have a valid corner line pole.
                    // if we find it in the second direction, it means our line is a stub of 0 length and should be treated as a single point.
                    if (MathB.GetDirectionsIntersection(startPoint, startDirection, endPoint, endDirection, out PolePt, ensureCrossesInDirection: true))
                    {
                        IsStub = false;
                    }
                    else if (MathB.GetDirectionsIntersection(startPoint, -startDirection, endPoint, -endDirection, out PolePt, ensureCrossesInDirection: true))
                    {
                        IsStub = true;
                        PolePt = (StartPt + EndPt) * 0.5f; // pole is mid point
                        StartPt = PolePt;   // if we found the intersection in the second direction, then we can treat this line as a single point
                        EndPt = PolePt;     // and set both start and end points to the intersection point
                    }
                    else
                    {
                        // no intersection found in either direction? Just use the mid-point of the segment as the pole
                        IsStub = false;
                        PolePt = (StartPt + EndPt) * 0.5f; // pole is mid point
                    }

                    Vector2 poleOriginOffset = PolePt - cornerOrigin;
                    PolePt = cornerOrigin + poleOriginOffset.normalized * Mathf.Clamp(poleOriginOffset.magnitude, cornerRadius, cornerRadius * 2); // make sure our pole isn't too far away
                }
            }

            public CornerLine LeftLine;
            public CornerLine RightLine;

            public CornerSegment(ref ConnectingSegment startSegment, ref ConnectingSegment endSegment, Vector2 cornerOrigin, float cornerRadius)
            {
                LeftLine = new CornerLine(ref startSegment, ref endSegment, left: true, cornerOrigin, cornerRadius);
                RightLine = new CornerLine(ref startSegment, ref endSegment, left: false, cornerOrigin, cornerRadius);
            }
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            RecalculateSegmentsData(); // TODO: Optimise to not call every mesh population!!

            if (!IsValidLine)
                return;

            // draw connecting segments, no end-caps or corners to begin with
            foreach (ConnectingSegment segment in m_ConnectingSegments)
            {
                if (segment.IsStub)
                    continue; // too small!! No espace for mother in law!

                DrawQuad(segment.StartL, segment.EndL, segment.EndR, segment.StartR, vh);
            }

            // draw corners segments
            float cornerStep = 1f / m_CornerDetail;
            Vector2[] cornerSegmentDrawVerts = new Vector2[4]; // max 4 for quads
            foreach (CornerSegment segment in m_CornerSegments)
            {
                // draw the quads that make up the corner segmenets,
                // by quadratically interpolating between the start and end points of the left and right lines
                for (int i = 0; i < m_CornerDetail; i++)
                {
                    float startT = i * cornerStep;
                    float endT = (i + 1) * cornerStep;

                    if (segment.LeftLine.IsStub)
                    {
                        cornerSegmentDrawVerts[0] = MathB.QuadraticLerp(segment.RightLine.StartPt, segment.RightLine.PolePt, segment.RightLine.EndPt, endT);
                        cornerSegmentDrawVerts[1] = MathB.QuadraticLerp(segment.RightLine.StartPt, segment.RightLine.PolePt, segment.RightLine.EndPt, startT);
                        cornerSegmentDrawVerts[2] = segment.LeftLine.PolePt; // right line is a stub, so just use the pole point
                        DrawTri(cornerSegmentDrawVerts[0], cornerSegmentDrawVerts[1], cornerSegmentDrawVerts[2], vh);
                    }
                    else if (segment.RightLine.IsStub)
                    {
                        cornerSegmentDrawVerts[0] = MathB.QuadraticLerp(segment.LeftLine.StartPt, segment.LeftLine.PolePt, segment.LeftLine.EndPt, startT);
                        cornerSegmentDrawVerts[1] = MathB.QuadraticLerp(segment.LeftLine.StartPt, segment.LeftLine.PolePt, segment.LeftLine.EndPt, endT);
                        cornerSegmentDrawVerts[2] = segment.RightLine.PolePt; // right line is a stub, so just use the pole point
                        DrawTri(cornerSegmentDrawVerts[0], cornerSegmentDrawVerts[1], cornerSegmentDrawVerts[2], vh);
                    }
                    else
                    {
                        cornerSegmentDrawVerts[0] = MathB.QuadraticLerp(segment.LeftLine.StartPt, segment.LeftLine.PolePt, segment.LeftLine.EndPt, startT);
                        cornerSegmentDrawVerts[1] = MathB.QuadraticLerp(segment.LeftLine.StartPt, segment.LeftLine.PolePt, segment.LeftLine.EndPt, endT);
                        cornerSegmentDrawVerts[2] = MathB.QuadraticLerp(segment.RightLine.StartPt, segment.RightLine.PolePt, segment.RightLine.EndPt, endT);
                        cornerSegmentDrawVerts[3] = MathB.QuadraticLerp(segment.RightLine.StartPt, segment.RightLine.PolePt, segment.RightLine.EndPt, startT);
                        DrawQuad(cornerSegmentDrawVerts[0], cornerSegmentDrawVerts[1], cornerSegmentDrawVerts[2], cornerSegmentDrawVerts[3], vh);
                    }
                }
            }

            // draw end-cap segments
            // ...
        }

        /// <summary>Calculate and cache the line segment data for the current <see cref="m_Points"/> for more efficient updates</summary>
        void RecalculateSegmentsData()
        {
            m_ConnectingSegments = new ConnectingSegment[Mathf.Max(0, m_Points.Length - 1)];
            m_CornerSegments = new CornerSegment[Mathf.Max(0, m_ConnectingSegments.Length - 1)];

            if (!IsValidLine)
                return;

            float nodeRadius = m_Thickness * 0.5f;

            // generate connecting segments data
            for (int i = 0; i < m_ConnectingSegments.Length; i++)
                m_ConnectingSegments[i] = new ConnectingSegment(m_Points[i], m_Points[i + 1], nodeRadius);

            // generate corner segments data
            for (int i = 0; i < m_CornerSegments.Length; i++)
                m_CornerSegments[i] = new CornerSegment(ref m_ConnectingSegments[i], ref m_ConnectingSegments[i + 1], m_Points[i + 1], nodeRadius);

            // generate end-cap segments data
            // ...
        }




        void DrawTri(Vector2 p1, Vector2 p2, Vector2 p3, VertexHelper vh)
        {
            int vertIndex = vh.currentVertCount;

            vh.AddVert(p1, color, Vector2.zero);
            vh.AddVert(p2, color, Vector2.zero);
            vh.AddVert(p3, color, Vector2.zero);

            vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
        }

        void DrawQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, VertexHelper vh)
        {
            int vertIndex = vh.currentVertCount;

            vh.AddVert(p1, color, Vector2.zero);
            vh.AddVert(p2, color, Vector2.zero);
            vh.AddVert(p3, color, Vector2.zero);
            vh.AddVert(p4, color, Vector2.zero);

            vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
            vh.AddTriangle(vertIndex + 2, vertIndex + 3, vertIndex);
        }
    }
}
