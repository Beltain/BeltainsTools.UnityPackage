using TMPro;
using UnityEngine;

namespace BeltainsTools.UI
{
    public class UILineGraph : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField]
        private UILineRenderer m_GraphLineRenderer;
        [SerializeField]
        private UIGridRenderer m_GraphGridRenderer;

        [Header("Axes")]
        [SerializeField]
        private GraphAxis m_VerticalAxis;
        [SerializeField]
        private GraphAxis m_HorizontalAxis;

        [Header("Data")]
        [SerializeField, Tooltip("If true: Our Axes value ranges are set based on the min/max data points. If false, use the default axis value range.")]
        private bool m_AxesEncapsulateData = false;
        [SerializeField]
        private Vector2[] m_DataPoints = new Vector2[0];


        [System.Serializable]
        public class GraphAxis
        {
            public enum AxisTypes { Vertical, Horizontal }

            [SerializeField] private AxisTypes m_AxisType;
            [SerializeField] private RectTransform m_LabelContainer;
            [SerializeField] private TextMeshProUGUI m_LabelPrefab;
            [SerializeField] private int m_LabelMaxCharacters = 6;
            [SerializeField] private int m_DivisionCount;

            [SerializeField] private Vector2 m_ValueRange;

            private Vector2 m_OverrideRange = default;

            private int NumLabels => m_DivisionCount + 1;
            public int DivisionCount => m_DivisionCount;
            public Vector2 ValueRange => m_OverrideRange != default ? m_OverrideRange : m_ValueRange;


            const int k_LabelMaxDecimalPrecision = 10;


            public void ResetRange() => SetRange(default);
            public void SetRange(float min, float max) => SetRange(new Vector2(min, max));
            public void SetRange(Vector2 range)
            {
                m_OverrideRange = range;
                RedrawLabels();
            }

            public void RedrawLabels()
            {
                ClearUnusedLabels();
                CreateMissingLabels();

                DrawLabels();
            }

            private void DrawLabels()
            {
                if (m_LabelContainer == null || m_LabelPrefab == null)
                    return;

                float labelSpacing = (m_AxisType switch
                {
                    AxisTypes.Vertical => m_LabelContainer.rect.height,
                    AxisTypes.Horizontal => m_LabelContainer.rect.width,
                    _ => 1f
                }) / m_DivisionCount;
                Vector2 labelDirection = m_AxisType switch
                {
                    AxisTypes.Vertical => Vector2.up,
                    AxisTypes.Horizontal => Vector2.right,
                    _ => Vector2.zero
                };

                string labelFormat = m_LabelPrefab.text;

                bool labelIsFormattable = !string.IsNullOrEmpty(labelFormat) && labelFormat.Contains("{0}");
                for (int i = 0; i < NumLabels; i++)
                {
                    TextMeshProUGUI label = GetLabel(i);
                    label.rectTransform.anchoredPosition = labelDirection * (labelSpacing * i);
                    
                    if (labelIsFormattable)
                        label.text = string.Format(labelFormat, FormatFloatAbbreviated(ValueRange.Lerp(i / (float)m_DivisionCount)));
                }
            }

            /// <summary>Creates labels until we have as many as <see cref="NumLabels"/></summary>
            private void CreateMissingLabels()
            {
                if (m_LabelContainer == null || m_LabelPrefab == null)
                    return;

                for (int i = GetLabelCount(); i < NumLabels; i++)
                    CreateLabel();
            }

            private int GetLabelCount()
            {
                return m_LabelContainer.childCount;
            }

            private TextMeshProUGUI GetLabel(int labelIndex)
            {
                return m_LabelContainer.GetChild(labelIndex).GetComponent<TextMeshProUGUI>();
            }

            private TextMeshProUGUI CreateLabel()
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return (TextMeshProUGUI)UnityEditor.PrefabUtility.InstantiatePrefab(m_LabelPrefab, m_LabelContainer);
#endif
                return Instantiate(m_LabelPrefab, m_LabelContainer);
            }

            private void ClearUnusedLabels() => ClearLabels(NumLabels);
            private void ClearLabels(int newLabelCount = 0)
            {
                if (m_LabelContainer == null)
                    return;

                for (int i = GetLabelCount() - 1; i >= newLabelCount; i--)
                    DestroyLabel(i);
            }

            private void DestroyLabel(int labelIndex)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(m_LabelContainer.GetChild(labelIndex).gameObject, true);
                    return;
                }
#endif
                Destroy(m_LabelContainer.GetChild(labelIndex).gameObject);
            }

            private string FormatFloatAbbreviated(float value)
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return value.ToString();

                if (m_LabelMaxCharacters < 1) return "";

                string[] suffixes = { "", "k", "m", "b", "t" };
                float absValue = Mathf.Abs(value);
                int suffixIndex = 0;

                while (absValue >= 1000 && suffixIndex < suffixes.Length - 1)
                {
                    absValue /= 1000;
                    suffixIndex++;
                }

                // get decimal places based on the step between value range divisions
                string stepString = ((ValueRange.x - ValueRange.y) / m_DivisionCount).ToString("0.################");
                int dotIndex = stepString.IndexOf('.');
                int decimalPlaces = Mathf.Min(k_LabelMaxDecimalPrecision, dotIndex == -1 ? 0 : stepString.Length - dotIndex - 1);

                // Try formatting with decreasing decimal places until it fits
                for (int decimals = decimalPlaces; decimals >= 0; decimals--)
                {
                    string formatString = "F" + decimals;
                    string candidate = (Mathf.Sign(value) * absValue).ToString(formatString) + suffixes[suffixIndex];
                    if (candidate.Length <= m_LabelMaxCharacters)
                        return candidate;
                }

                // If even integer format doesn't fit, use scientific notation or hashes
                if (m_LabelMaxCharacters >= 4)
                    return value.ToString("0.#E+0").PadLeft(m_LabelMaxCharacters).Substring(0, m_LabelMaxCharacters);
                return new string('#', m_LabelMaxCharacters);
            }
        }

        public void RedrawGraphStructure()
        {
            m_GraphGridRenderer?.SetDimensions(m_HorizontalAxis.DivisionCount, m_VerticalAxis.DivisionCount);

            m_VerticalAxis.RedrawLabels();
            m_HorizontalAxis.RedrawLabels();
        }

        public void SetData(Vector2[] dataPoints)
        {
            System.Array.Resize(ref m_DataPoints, dataPoints.Length);
            for (int i = 0; i < dataPoints.Length; i++)
                m_DataPoints[i] = dataPoints[i];

            RedrawGraphData();
        }

        public void RedrawGraphData()
        {
            if (m_GraphLineRenderer != null)
            {
                if (m_DataPoints != null && m_DataPoints.Length > 1)
                {
                    System.Array.Sort(m_DataPoints, (a, b) => a.x.CompareTo(b.x));
                    m_GraphLineRenderer.SetPoints(m_DataPoints);
                }
                else
                {
                    m_GraphLineRenderer.SetPoints(null);
                }
            }

            if (m_AxesEncapsulateData && m_DataPoints != null && m_DataPoints.Length > 0)
            {
                Vector2 dataMin = Vector2.positiveInfinity;
                Vector2 dataMax = Vector2.negativeInfinity;
                foreach (Vector2 point in m_DataPoints)
                {
                    dataMin = Vector2.Min(dataMin, point);
                    dataMax = Vector2.Max(dataMax, point);
                }

                m_VerticalAxis.SetRange(dataMin.y, dataMax.y);
                m_HorizontalAxis.SetRange(dataMin.x, dataMax.x);
            }
            else
            {
                m_VerticalAxis.ResetRange();
                m_HorizontalAxis.ResetRange();
            }
        }


        private void Reset()
        {
            RedrawGraphStructure();
            RedrawGraphData();
        }
    }
}
