using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeltainsTools.UI
{
    public class UILineRenderer : MaskableGraphic
    {
        [SerializeField]
        Vector2[] m_Points = new Vector2[0];
        [SerializeField]
        float m_Thickness = 0.5f;
        [SerializeField, Tooltip("Number of segments to use per 90 degrees when drawing corners, more segments = smoother corners")]
        int m_CornerDetail = 8;
        [SerializeField]
        bool m_IsLoop = false;


        ConnectingSegment[] m_ConnectingSegments;
        CornerSegment[] m_CornerSegments;
        EndCapSegment[] m_EndCapSegments = new EndCapSegment[2]; // 2 end-caps, one for each end of the line
        

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

            public void Draw(VertexHelper vh, Color color)
            {
                if (IsStub)
                    return; // too small!! No espace for mother in law!

                vh.DrawQuad(StartL, EndL, EndR, StartR, color);
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
            public int Detail;

            public CornerSegment(ref ConnectingSegment startSegment, ref ConnectingSegment endSegment, Vector2 cornerOrigin, float cornerRadius, int detail)
            {
                LeftLine = new CornerLine(ref startSegment, ref endSegment, left: true, cornerOrigin, cornerRadius);
                RightLine = new CornerLine(ref startSegment, ref endSegment, left: false, cornerOrigin, cornerRadius);

                float angle = 360 - Vector2.Angle(-startSegment.Direction, endSegment.Direction);
                Detail = Mathf.Max(1, Mathf.FloorToInt(angle / 90f * detail)); // number of segments to draw for this corner segment, based on the angle between the two lines
            }


            static Vector2[] cornerSegmentDrawVerts = new Vector2[4]; // max 4 for quads
            public void Draw(VertexHelper vh, Color color)
            {
                float cornerStep = 1f / Detail;

                // draw the quads that make up the corner segmenets,
                // by quadratically interpolating between the start and end points of the left and right lines
                for (int i = 0; i < Detail; i++)
                {
                    float startT = i * cornerStep;
                    float endT = (i + 1) * cornerStep;

                    if (LeftLine.IsStub)
                    {
                        cornerSegmentDrawVerts[0] = MathB.QuadraticLerp(RightLine.StartPt, RightLine.PolePt, RightLine.EndPt, endT);
                        cornerSegmentDrawVerts[1] = MathB.QuadraticLerp(RightLine.StartPt, RightLine.PolePt, RightLine.EndPt, startT);
                        cornerSegmentDrawVerts[2] = LeftLine.PolePt; // right line is a stub, so just use the pole point
                        vh.DrawTri(cornerSegmentDrawVerts[0], cornerSegmentDrawVerts[1], cornerSegmentDrawVerts[2], color);
                    }
                    else if (RightLine.IsStub)
                    {
                        cornerSegmentDrawVerts[0] = MathB.QuadraticLerp(LeftLine.StartPt, LeftLine.PolePt, LeftLine.EndPt, startT);
                        cornerSegmentDrawVerts[1] = MathB.QuadraticLerp(LeftLine.StartPt, LeftLine.PolePt, LeftLine.EndPt, endT);
                        cornerSegmentDrawVerts[2] = RightLine.PolePt; // right line is a stub, so just use the pole point
                        vh.DrawTri(cornerSegmentDrawVerts[0], cornerSegmentDrawVerts[1], cornerSegmentDrawVerts[2], color);
                    }
                    else
                    {
                        cornerSegmentDrawVerts[0] = MathB.QuadraticLerp(LeftLine.StartPt, LeftLine.PolePt, LeftLine.EndPt, startT);
                        cornerSegmentDrawVerts[1] = MathB.QuadraticLerp(LeftLine.StartPt, LeftLine.PolePt, LeftLine.EndPt, endT);
                        cornerSegmentDrawVerts[2] = MathB.QuadraticLerp(RightLine.StartPt, RightLine.PolePt, RightLine.EndPt, endT);
                        cornerSegmentDrawVerts[3] = MathB.QuadraticLerp(RightLine.StartPt, RightLine.PolePt, RightLine.EndPt, startT);
                        vh.DrawQuad(cornerSegmentDrawVerts[0], cornerSegmentDrawVerts[1], cornerSegmentDrawVerts[2], cornerSegmentDrawVerts[3], color);
                    }
                }
            }
        }

        /// <summary>Structure containing the definition of the end-cap segment of the line, which is a segment that extends from the end of a <see cref="ConnectingSegment"/> towards the origin point of the line</summary>
        struct EndCapSegment // TODO maybe add rounding in the future?
        {
            public Vector2 StartL;
            public Vector2 StartR;
            public Vector2 EndL;
            public Vector2 EndR;

            public EndCapSegment(ConnectingSegment connectingSegment, Vector2 pointOrigin, float nodeRadius)
            {
                bool isStartOfSegment = Vector2.Distance(connectingSegment.StartL, pointOrigin) < Vector2.Distance(connectingSegment.EndL, pointOrigin);
                if (isStartOfSegment)
                {
                    EndL = connectingSegment.StartL;
                    EndR = connectingSegment.StartR;
                    StartL = EndL - connectingSegment.Direction * nodeRadius;
                    StartR = EndR - connectingSegment.Direction * nodeRadius;
                }
                else
                {
                    EndL = connectingSegment.EndL;
                    EndR = connectingSegment.EndR;
                    StartL = EndL + connectingSegment.Direction * nodeRadius;
                    StartR = EndR + connectingSegment.Direction * nodeRadius;
                }
            }

            public void Draw(VertexHelper vh, Color color)
            {
                vh.DrawQuad(StartL, EndL, EndR, StartR, color);
            }
        }


        public void SetPoints(Vector2[] points)
        {
            Array.Resize(ref m_Points, points.Length);
            for (int i = 0; i < points.Length; i++)
                m_Points[i] = points[i];
            RecalculateSegmentsData();
        }

        public void SetPoint(int pointIndex, Vector2 point)
        {
            m_Points[pointIndex] = point;
            RecalculateSegmentsData();
        }

        public Vector2[] GetPoints()
        {
            return m_Points;
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (Application.isEditor && !Application.isPlaying)
                RecalculateSegmentsData();

            if (!IsValidLine)
                return;

            // draw connecting segments, no end-caps or corners to begin with
            foreach (ConnectingSegment segment in m_ConnectingSegments)
                segment.Draw(vh, color);

            // draw corners segments
            foreach (CornerSegment segment in m_CornerSegments)
                segment.Draw(vh, color);

            // draw end-cap segments
            foreach (EndCapSegment segment in m_EndCapSegments)
                segment.Draw(vh, color);
        }

        /// <summary>Calculate and cache the line segment data for the current <see cref="m_Points"/> for more efficient updates</summary>
        void RecalculateSegmentsData()
        {   
            int pointCount = m_Points.Length;
            bool isLoop = m_IsLoop && pointCount > 2;

            int connectingCount = Mathf.Max(0, isLoop ? pointCount : pointCount - 1);
            int cornerCount = Mathf.Max(0, connectingCount - 1 + (isLoop ? 1 : 0));
            int endCapCount = isLoop ? 0 : 2;

            m_ConnectingSegments = new ConnectingSegment[connectingCount];
            m_CornerSegments = new CornerSegment[cornerCount];
            m_EndCapSegments = new EndCapSegment[2]; // always allocate 2 for consistency

            if (!IsValidLine)
                return;

            float nodeRadius = m_Thickness * 0.5f;

            // generate connecting segments data
            for (int i = 0; i < connectingCount; i++)
            {
                int startIdx = i;
                int endIdx = (i + 1) % pointCount;
                m_ConnectingSegments[i] = new ConnectingSegment(m_Points[startIdx], m_Points[endIdx], nodeRadius);
            }

            // generate corner segments data
            for (int i = 0; i < cornerCount; i++)
            {
                int segA = i;
                int segB = (i + 1) % connectingCount;
                int cornerIdx = (i + 1) % pointCount;
                m_CornerSegments[i] = new CornerSegment(ref m_ConnectingSegments[segA], ref m_ConnectingSegments[segB], m_Points[cornerIdx], nodeRadius, m_CornerDetail);
            }

            // generate end-cap segments data (only if not loop)
            if (!isLoop)
            {
                m_EndCapSegments[0] = new EndCapSegment(m_ConnectingSegments[0], m_Points[0], nodeRadius);
                m_EndCapSegments[1] = new EndCapSegment(m_ConnectingSegments[connectingCount - 1], m_Points[pointCount - 1], nodeRadius);
            }
        }


        protected override void Awake()
        {
            RecalculateSegmentsData();
        }
    }
}
