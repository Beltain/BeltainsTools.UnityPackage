using BeltainsTools.UI;
using TMPro;
using UnityEngine;

namespace BeltainsTools.Debugging
{
    public class UI_LineGraphOutput_Element : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_TitleText;
        [SerializeField] UILineGraph m_LineGraph;


        public void Set(string title, Vector2[] data)
        {
            m_TitleText.text = title;
            m_LineGraph.SetData(data);
        }

        public void UpdateData(Vector2[] data)
        {
            m_LineGraph.SetData(data);
        }

        void OnPool()
        {
            Set(string.Empty, null);
        }

        void OnRecycle()
        {
            Set(string.Empty, null);
        }
    }
}
