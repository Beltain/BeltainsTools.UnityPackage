using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeltainsTools.Debugging
{
    public class UI_TextOutput_Group : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_TitleText;
        [SerializeField] private RectTransform m_EntriesRect;

        private RectTransform m_RectTransform;

        private HashSet<UI_TextOutput_Element> m_Entries = new HashSet<UI_TextOutput_Element>();

        public HashSet<UI_TextOutput_Element> Entries => m_Entries;

        public void SetTitle(string title)
        {
            m_TitleText.text = title;
        }

        public void AddEntry(UI_TextOutput_Element entry)
        {
            m_Entries.Add(entry);
            entry.RecycledEvent.Subscribe(OnEntryRecycled);
            entry.transform.SetParent(m_EntriesRect, false);
        }

        public void RemoveEntry(UI_TextOutput_Element entry)
        {
            m_Entries.Remove(entry);
            entry.transform.SetParent(null, false);
            entry.RecycledEvent.Unsubscribe(OnEntryRecycled);
        }

        public void ForceRebuildLayoutImmediate()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_EntriesRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_RectTransform);
        }

        private void OnEntryRecycled(UI_TextOutput_Element entry)
        {
            RemoveEntry(entry);
        }

        private void OnPool()
        {
            m_RectTransform = transform as RectTransform;
        }
    }
}
