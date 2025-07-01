using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace BeltainsTools.UI
{
    public class ScrollViewItemEventPassthrough : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        [SerializeField] bool m_PassDragEvents;
        [SerializeField] bool m_PassScrollEvents;

        //protected ScrollRect m_ScrollRect = null;

        //void RefreshScrollRect() // no good way to refresh this, seeing as this component should ideally be a set and forget kind of thing.
        //{
        //    m_ScrollRect = this.GetComponentInParents<ScrollRect>(10);
        //    d.Assert(m_ScrollRect != null, "Attempted to use a ScrollViewItemEventPassthrough on an element that is not contained within a scrollrect! No bueno!");
        //}

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!m_PassDragEvents)
                return;
            this.GetComponentInParents<ScrollRect>(10).OnBeginDrag(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!m_PassDragEvents)
                return;
            this.GetComponentInParents<ScrollRect>(10).OnDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!m_PassDragEvents)
                return;
            this.GetComponentInParents<ScrollRect>(10).OnEndDrag(eventData);
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if (!m_PassScrollEvents)
                return;
            this.GetComponentInParents<ScrollRect>(10).OnScroll(eventData);
        }


        //void Start()
        //{
        //    RefreshScrollRect();
        //}
    }
}
