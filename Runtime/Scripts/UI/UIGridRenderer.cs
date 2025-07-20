using UnityEngine;
using UnityEngine.UI;

namespace BeltainsTools
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIGridRenderer : MaskableGraphic
    {
        [SerializeField] int m_ColumnCount = 10;
        [SerializeField] int m_RowCount = 10;
        [SerializeField] float m_LineThickness = 10f;
        [SerializeField] bool m_DrawBorderLines = true;

        public void SetDimensions(int columnCount, int rowCount)
        {
            m_ColumnCount = columnCount;
            m_RowCount = rowCount;

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect graphicRect = (transform as RectTransform).rect;
            DrawBars(graphicRect.width, graphicRect.height, m_ColumnCount, graphicRect.min, Vector2.right, Vector2.up, numSegments: 1, vh);
            DrawBars(graphicRect.height, graphicRect.width, m_RowCount, graphicRect.min, Vector2.up, Vector2.right, numSegments: m_ColumnCount, vh);
            if (m_DrawBorderLines)
                DrawOutline(graphicRect, vh);
        }

        void DrawBars(float axisLength, float lineHeight, int numLines, 
            Vector2 axisOrigin, Vector2 axisDirection, Vector2 segmentDirection, 
            int numSegments, VertexHelper vh)
        {
            float spacing = axisLength / numLines;
            float splitSpacing = m_LineThickness * 0.5f; // how much space to leave off the top and bottom of each segment when splitting
            float lineSegmentLength = lineHeight / numSegments;
            for (int i = 1; i < numLines; i++)
            {
                for (int segI = 0; segI < numSegments; segI++)
                {
                    Vector2 segOffset = segmentDirection * lineSegmentLength * segI;

                    float startShortening = ((numSegments > 1 && segI > 0) || m_DrawBorderLines) ? splitSpacing : 0f;
                    float endShortening = ((numSegments > 1 && segI + 1 < numSegments) || m_DrawBorderLines) ? splitSpacing : 0f;

                    Vector2 unshortenedOrigin = axisOrigin + axisDirection * spacing * i + segOffset;
                    Vector2 unshortenedTarget = unshortenedOrigin + segmentDirection * lineSegmentLength;

                    Vector2 lineOrigin = unshortenedOrigin + segmentDirection * startShortening;
                    Vector2 lineTarget = unshortenedTarget - segmentDirection * endShortening;

                    vh.DrawLine(lineOrigin, lineTarget, m_LineThickness, color);
                }
            }
        }

        void DrawOutline(Rect aroundRect, VertexHelper vh)
        {
            void DrawLine(Vector2 start, Vector2 end)
            {
                Vector2 direction = (end - start).normalized;
                Vector2 lineOffset = -direction * m_LineThickness * 0.5f;
                vh.DrawLine(start + lineOffset, end + lineOffset, m_LineThickness, color);
            }

            Vector2[] corners = new Vector2[] {
                aroundRect.min,
                new Vector2(aroundRect.min.x, aroundRect.max.y),
                aroundRect.max,
                new Vector2(aroundRect.max.x, aroundRect.min.y),
            };

            DrawLine(corners[0], corners[1]);
            DrawLine(corners[1], corners[2]);
            DrawLine(corners[2], corners[3]);
            DrawLine(corners[3], corners[0]);
        }
    }
}
