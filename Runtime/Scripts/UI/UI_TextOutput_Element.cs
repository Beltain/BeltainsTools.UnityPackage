using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BeltainsTools.Debugging
{
    public class UI_TextOutput_Element : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_TextMP;
        [SerializeField] UnityEngine.UI.Image m_BackgroundImage;

        Color m_DefaultColor = Color.magenta;


        public void SetText(string text)
        {
            m_TextMP.text = text;
        }

        public void ResetBackgroundColor() => SetBackgroundColor(m_DefaultColor);
        public void SetBackgroundColor(Color color)
        {
            m_BackgroundImage.color = color;
        }


        private void OnPool()
        {
            m_DefaultColor = m_BackgroundImage.color;
        }

        void OnSpawn()
        {
            ResetBackgroundColor();
        }
    }
}
