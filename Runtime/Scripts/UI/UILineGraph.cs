using TMPro;
using UnityEngine;

namespace BeltainsTools.UI
{
    public class UILineGraph : MonoBehaviour
    {
        [Header("Axes")]
        [SerializeField]
        private GraphAxis m_VerticalAxis;
        [SerializeField]
        private GraphAxis m_HorizontalAxis;

        [Header("Refs")]
        [SerializeField]
        private UILineRenderer m_GraphLineRenderer;
        [SerializeField]
        private UIGridRenderer m_GraphGridRenderer;

        [System.Serializable]
        public class GraphAxis
        {
            public AxisTypes AxisType;
            public int DivisionCount;
            public Vector2 ValueRange;
            public RectTransform LabelContainer;
            public TextMeshProUGUI LabelPrefab;
            public string LabelFormat = "{0} -";
            public int LabelMaxDecimalPrecision = 2;
            public int LabelMaxCharacters = 6;

            public enum AxisTypes
            {
                Vertical,
                Horizontal
            }

            public void DrawLabels()
            {
                if (LabelContainer == null || LabelPrefab == null)
                    return;

                int numLabels = DivisionCount + 1;
                float labelSpacing = (AxisType switch
                {
                    AxisTypes.Vertical => LabelContainer.rect.height,
                    AxisTypes.Horizontal => LabelContainer.rect.width,
                    _ => 1f
                }) / DivisionCount;
                Vector2 labelDirection = AxisType switch
                {
                    AxisTypes.Vertical => Vector2.up,
                    AxisTypes.Horizontal => Vector2.right,
                    _ => Vector2.zero
                };

                int labelDecimalPlaces = Mathf.Min(LabelMaxDecimalPrecision, GetDecimalPlaces((ValueRange.x - ValueRange.y) / DivisionCount));

                bool labelIsFormattable = !string.IsNullOrEmpty(LabelFormat) && LabelFormat.Contains("{0}");
                for (int i = 0; i < numLabels; i++)
                {
                    TextMeshProUGUI label = (TextMeshProUGUI)UnityEditor.PrefabUtility.InstantiatePrefab(LabelPrefab, LabelContainer);
                    label.rectTransform.anchoredPosition = labelDirection * (labelSpacing * i);
                    
                    if (labelIsFormattable)
                        label.text = string.Format(LabelFormat, FormatFloatAbbreviated(ValueRange.Lerp(i / (float)DivisionCount), labelDecimalPlaces));
                }
            }

            public void ClearLabels()
            {
                if (LabelContainer == null)
                    return;

                for (int i = LabelContainer.childCount - 1; i >= 0; i--)
                {
                    if (Application.isPlaying)
                        Destroy(LabelContainer.GetChild(i).gameObject);
                    else
                        DestroyImmediate(LabelContainer.GetChild(i).gameObject, true);
                }
            }

            string FormatFloatAbbreviated(float value, int decimalPlaces)
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return value.ToString();

                if (LabelMaxCharacters < 1) return "";

                string[] suffixes = { "", "k", "m", "b", "t" };
                float absValue = Mathf.Abs(value);
                int suffixIndex = 0;

                while (absValue >= 1000 && suffixIndex < suffixes.Length - 1)
                {
                    absValue /= 1000;
                    suffixIndex++;
                }

                // Try formatting with decreasing decimal places until it fits
                for (int decimals = decimalPlaces; decimals >= 0; decimals--)
                {
                    string formatString = "F" + decimals;
                    string candidate = (Mathf.Sign(value) * absValue).ToString(formatString) + suffixes[suffixIndex];
                    if (candidate.Length <= LabelMaxCharacters)
                        return candidate;
                }

                // If even integer format doesn't fit, use scientific notation or hashes
                if (LabelMaxCharacters >= 4)
                    return value.ToString("0.#E+0").PadLeft(LabelMaxCharacters).Substring(0, LabelMaxCharacters);

                return new string('#', LabelMaxCharacters);
            }

            int GetDecimalPlaces(float value)
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return 0;

                // Convert to string with high precision, trimming trailing zeroes
                string str = value.ToString("0.################");

                int dotIndex = str.IndexOf('.');
                if (dotIndex == -1)
                    return 0; // No decimal point = no decimal places

                return str.Length - dotIndex - 1;
            }

        }

        public void ReDrawGraphStructure()
        {
            m_VerticalAxis.ClearLabels();
            m_HorizontalAxis.ClearLabels();

            m_GraphGridRenderer?.SetDimensions(m_HorizontalAxis.DivisionCount, m_VerticalAxis.DivisionCount);

            m_VerticalAxis.DrawLabels();
            m_HorizontalAxis.DrawLabels();
        }


        private void Reset()
        {
            ReDrawGraphStructure();
        }
    }
}
