using UnityEngine;

namespace BeltainsTools.PlayerInteraction
{
    public interface IHoverHandler
    {
        void OnHoverEnter(PointerEventData pointerData);
        void OnHoverExit(PointerEventData pointerData);
    }


    public interface IClickHandlerBase { }
    /// <summary>For handling full click events (both down and up completed on the same object)</summary>
    public interface IClickHandler : IClickHandlerBase
    {
        void OnClick(PointerEventData pointerData, PointerEventData.Click clickData);
    }
    public interface IClickDownHandler : IClickHandlerBase
    {
        void OnClickDown(PointerEventData pointerData, PointerEventData.Click clickData);
    }
    public interface IClickUpHandler : IClickHandlerBase
    {
        void OnClickUp(PointerEventData pointerData, PointerEventData.Click clickData);
    }

    public interface IDragHandlerBase { }
    public interface IDragHandler : IDragStartHandler, IDragUpdateHandler, IDragEndHandler { }

    public interface IDragInitialiser : IDragHandlerBase
    {
        /// <returns>Whether or not a drag is valid for the current <see cref="PointerEventData"/> and this implementer</returns>
        bool OnTryInitialiseDrag(PointerEventData pointerData, PointerEventData.Drag dragData);
    }

    public interface IDragStartHandler : IDragHandlerBase
    {
        void OnDragStart(PointerEventData pointerData, PointerEventData.Drag dragData);
    }
    public interface IDragUpdateHandler : IDragHandlerBase
    {
        void OnDrag(PointerEventData pointerData, PointerEventData.Drag dragData);
    }
    public interface IDragEndHandler : IDragHandlerBase
    {
        void OnDragEnd(PointerEventData pointerData, PointerEventData.Drag dragData);
    }
}
