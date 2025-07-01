using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BeltainsTools.EventHandling;

namespace BeltainsTools.InputUtils
{
    public class CursorActionHandler : System.IDisposable
    {
        public BEvent StartedEvent;
        public BEvent CancelledEvent;
        public BEvent DragStartedEvent;
        public BEvent DragCancelledEvent;

        const float k_DragUnitsThreshold = 0.01f;
        const float k_DragUnitsNormalMaximum = 0.01f;

        public bool StartedThisFrame { get; private set; } = false;
        public bool ReleasedThisFrame { get; private set; } = false;

        public bool IsDragging { get; private set; } = false;
        public bool DragStartedThisFrame { get; private set; } = false;
        public bool DragReleasedThisFrame { get; private set; } = false;

        public Vector2 DragOrigin { get; private set; } = Vector2.negativeInfinity;
        /// <summary>Vector travelled from the <see cref="DragOrigin"/> in screen space.</summary>
        public Vector2 DragVector { get; private set; } = Vector2.zero;
        /// <summary>How many of the screen's smallest axis lengths (between width/height) is the <see cref="DragVector"/></summary>
        public Vector2 DragUnits { get; private set; } = Vector2.zero;
        /// <summary>How many screen widths is the <see cref="DragVector"/></summary>
        public Vector2 DragUnits_ScreenWidth { get; private set; } = Vector2.zero;
        /// <summary>How many screen heights is the <see cref="DragVector"/></summary>
        public Vector2 DragUnits_ScreenHeight { get; private set; } = Vector2.zero;
        /// <summary>Intensity of <see cref="DragVector"/>, capped to the <see cref="k_DragUnitsNormalMaximum"/> as a proportion of the standard drag unit</summary>
        public Vector2 DragNormalIntensity01 { get; private set; } = Vector2.zero;


        InputAction m_Action;
        MonoBehaviour m_CoroutineOwner;


        public CursorActionHandler(InputAction inputAction, MonoBehaviour coroutineOwner)
        {
            m_Action = inputAction;
            m_CoroutineOwner = coroutineOwner;

            m_Action.started += InputAction_started;
            m_Action.canceled += InputAction_canceled;
        }

        ~CursorActionHandler() => Dispose();
        public void Dispose()
        {
            m_Action.started -= InputAction_started;
            m_Action.canceled -= InputAction_canceled;
        }



        void SetActiveClickDown(bool down)
        {
            if (down)
            {
                StartedThisFrame = true;

                CursorDownCo = CursorDownCoroutine();
                m_CoroutineOwner.StartCoroutine(CursorDownCo);
                StartedEvent.Invoke();
            }
            else
            {
                if (StartedThisFrame)
                    StartedThisFrame = false;

                ReleasedThisFrame = true;

                if (IsDragging)
                {
                    IsDragging = false;
                    DragReleasedThisFrame = true;
                    DragCancelledEvent.Invoke();
                }

                m_CoroutineOwner.StopCoroutine(CursorDownCo);
                CancelledEvent.Invoke();

                DragOrigin = Vector2.negativeInfinity;
                SetDragVector(Vector2.zero);

                m_CoroutineOwner.StartCoroutine(CursorUpCoroutine());
            }
        }

        IEnumerator CursorDownCo = null;
        IEnumerator CursorDownCoroutine()
        {
            DragOrigin = Mouse.current.position.value;

            bool wasDragging = false;
            while (true)
            {
                UpdateDragVector();

                if (!IsDragging && Vector2.Distance(DragOrigin, Mouse.current.position.value) > k_DragUnitsThreshold)
                {
                    IsDragging = true;
                    DragStartedEvent.Invoke();
                }

                if (wasDragging != IsDragging)
                {
                    wasDragging = IsDragging;
                    if (IsDragging)
                        DragStartedThisFrame = true;
                }

                yield return new WaitForEndOfFrame();

                DragStartedThisFrame = false;
                StartedThisFrame = false;
            }
        }

        IEnumerator CursorUpCoroutine()
        {
            yield return new WaitForEndOfFrame();

            ReleasedThisFrame = false;
            DragReleasedThisFrame = false;
        }

        void UpdateDragVector() => SetDragVector(Mouse.current.position.value - DragOrigin);
        void SetDragVector(Vector2 dragVec)
        {
            DragVector = dragVec;

            DragUnits_ScreenHeight = DragVector / Screen.height;
            DragUnits_ScreenWidth = DragVector / Screen.width;
            DragUnits = Screen.height > Screen.width ? DragUnits_ScreenWidth : DragUnits_ScreenHeight;

            DragNormalIntensity01 = Vector2.ClampMagnitude(DragUnits / k_DragUnitsNormalMaximum, 1f);
        }


        private void InputAction_canceled(InputAction.CallbackContext obj)
        {
            SetActiveClickDown(false);
        }

        private void InputAction_started(InputAction.CallbackContext obj)
        {
            SetActiveClickDown(true);
        }
    }
}
