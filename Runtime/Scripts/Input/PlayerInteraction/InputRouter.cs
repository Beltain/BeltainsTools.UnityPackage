using BeltainsTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BeltainsTools.PlayerInteraction
{
    /// <summary>Class to convert raw player input into player interaction intent and broadcast intent events (OnDragStart, OnClick, OnDrag, OnHover, Etc.)</summary>
    /// <remarks>
    /// Essentially a parallel UnityEngine.EventSystems.EventSystem for custom player interaction handling and decoupling 
    /// <para>(Can block player interaction through this while allowing basic UI events through EventSystem just by disabling input maps (UI/PlayerInteraction) for example.)</para>
    /// </remarks>
    public partial class InputRouter : MonoBehaviour
    {
        [SerializeField, Tooltip("Base layers to whitelist for interaction. If you want to use Raycast Strategies, please note that these act as a default WHITELIST")]
        private LayerMask m_BaseLayerMask = ~0;
        [SerializeField, Range(0f, 1f), Tooltip(
            "The minimum viewport distance traveled in the minor axis needed to initiate a drag." +
            "\nFor reference: 0.1 in 1920:1080 would be 0.1 * 1080 = 108 pixels travelled")]
        private float m_ClickDragThreshold = 0.01f;
        [Space(5)]
        [SerializeField, Tooltip("Reference to a graphic raycaster for UI raycasting. If left unassigned UI raycasting will be skipped.")]
        private GraphicRaycaster m_GraphicRaycaster;
        [SerializeField, Tooltip("The input action asset to listen to, reassign on play with any generated InputActionAssets for best results")]
        private InputActionAsset m_ActionAsset;
        [SerializeField]
        private InputMappingSettings m_InputMapping;

        private static List<RaycastResult> s_RaycastResultsCache = new List<RaycastResult>();
        private static RaycastHit[] s_RaycastHitsCache = new RaycastHit[16];

        private PointerEventData m_Pointer = new PointerEventData();
        private HashSet<IRaycastStrategy> m_ActiveRaycastStrategies = new HashSet<IRaycastStrategy>();

        private Comparer<RaycastHit> m_RaycastHitDistanceComparer = Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));


        public InputActionAsset ActionAsset
        {
            get => m_ActionAsset;
            set
            {
                RemoveInputCallbacks();
                m_ActionAsset = value;
                m_InputMapping.RereferenceAll(m_ActionAsset);
                AddInputCallbacks();
            }
        }

        public GraphicRaycaster GraphicRaycaster
        {
            get => m_GraphicRaycaster;
            set => m_GraphicRaycaster = value;
        }

        [System.Serializable]
        public class InputMappingSettings
        {
            public InputActionProperty PrimaryInteract;
            public InputActionProperty SecondaryInteract;

            private Dictionary<string, InputAction> m_OriginalActionRefs;

            public void AddCallback(ref InputActionProperty property, System.Action<InputAction.CallbackContext> callback)
            {
                if (property.action == null)
                    return;
                property.action.started += callback;
                property.action.canceled += callback;
                property.action.performed += callback;
            }

            public void RemoveCallback(ref InputActionProperty property, System.Action<InputAction.CallbackContext> callback)
            {
                if (property.action == null)
                    return;
                property.action.started -= callback;
                property.action.canceled -= callback;
                property.action.performed -= callback;
            }

            public void RereferenceAll(InputActionAsset asset)
            {
                Rereference(ref PrimaryInteract, asset);
                Rereference(ref SecondaryInteract, asset);
            }

            private void Rereference(ref InputActionProperty property, InputActionAsset asset)
            {
                TrackOriginalRefs(ref property);
                if (property.reference != null && property.reference.action != null)
                {
                    InputActionReference inputActionReference = ScriptableObject.CreateInstance<InputActionReference>();
                    InputAction action = asset != null ? asset.FindAction(property.reference.action.name) : m_OriginalActionRefs[property.reference.action.name];
                    inputActionReference.Set(action);
                    property = new InputActionProperty(inputActionReference);
                }
            }

            private void TrackOriginalRefs(ref InputActionProperty property)
            {
                if (m_OriginalActionRefs == null)
                    m_OriginalActionRefs = new Dictionary<string, InputAction>();

                if (property.reference != null && property.reference.action != null && !m_OriginalActionRefs.ContainsKey(property.reference.action.name))
                    m_OriginalActionRefs[property.reference.action.name] = property.reference.action;
            }
        }


        /// <summary>Assign a <see cref="IRaycastStrategy"/> to be applied to raycasts performed by the InputRouter. This can be used to filter raycast results for interaction logic.</summary>
        public void RegisterRaycastStrategy(IRaycastStrategy strategy)
        {
            if (strategy != null)
                m_ActiveRaycastStrategies.Add(strategy);
        }

        public void DeregisterRaycastStrategy(IRaycastStrategy strategy)
        {
            if (strategy != null)
                m_ActiveRaycastStrategies.Remove(strategy);
        }


        private void AddInputCallbacks()
        {
            m_InputMapping.AddCallback(ref m_InputMapping.PrimaryInteract, OnClick_Primary);
            m_InputMapping.AddCallback(ref m_InputMapping.SecondaryInteract, OnClick_Secondary);
        }

        private void RemoveInputCallbacks()
        {
            m_InputMapping.RemoveCallback(ref m_InputMapping.PrimaryInteract, OnClick_Primary);
            m_InputMapping.RemoveCallback(ref m_InputMapping.SecondaryInteract, OnClick_Secondary);
        }


        private GameObject ResolveRaycast(out bool isOverUI)
        {
            LayerMask mask = m_BaseLayerMask;
            foreach (IRaycastStrategy strategy in m_ActiveRaycastStrategies)
                mask |= strategy.GetLayerMaskWhitelist();
            foreach (IRaycastStrategy strategy in m_ActiveRaycastStrategies)
                mask &= ~strategy.GetLayerMaskBlacklist();

            GameObject gameObject = ResolveRaycastUI(mask, out isOverUI);
            if (gameObject != null)
                return gameObject;

            return isOverUI ? null : ResolveRaycastWorld(mask); // only raycast world if not over UI
        }

        private bool ValidateRaycastGameObject(GameObject gameObject)
        {
            return gameObject != null &&
                !m_ActiveRaycastStrategies.Any(r => r.GetInvalidateTarget(gameObject)) && // filter out blacklisted
                (m_ActiveRaycastStrategies.Count == 0 || m_ActiveRaycastStrategies.Any(r => r.GetValidateTarget(gameObject))); // filter out non-whitelisted
        }

        private GameObject ResolveRaycastUI(LayerMask raycastMask, out bool isOverUI) // defers to unity graphic raycaster but with some additional filtering on top
        {
            if (m_GraphicRaycaster == null)
            {
                isOverUI = false;
                return null;
            }

            GameObject gameObject = null;

            UnityEngine.EventSystems.PointerEventData pointerEventData = new UnityEngine.EventSystems.PointerEventData(EventSystem.current) { position = m_Pointer.ScreenPosition };
            m_GraphicRaycaster.Raycast(pointerEventData, s_RaycastResultsCache);
            isOverUI = s_RaycastResultsCache.Count > 0; // includes non-interactable graphics, which is what we want for blocking interaction with the world behind UI

            gameObject = s_RaycastResultsCache.FirstOrDefault(
                r => r.isValid && 
                (1 << r.gameObject.layer & raycastMask) != 0 && 
                ValidateRaycastGameObject(r.gameObject)).gameObject;
            s_RaycastResultsCache.Clear();
            return gameObject;
        }

        private GameObject ResolveRaycastWorld(LayerMask raycastMask)
        {
            if (Camera.main == null)
                return null;

            Ray ray = Camera.main.ScreenPointToRay(m_Pointer.ScreenPosition);
            int hits = BeltainsTools.Utilities.PhysicsUtilities.RaycastNonAlloc(ray, ref s_RaycastHitsCache, float.PositiveInfinity, raycastMask);
            System.Array.Sort(s_RaycastHitsCache, 0, hits, m_RaycastHitDistanceComparer);

            for (int i = 0; i < hits; i++)
            {
                RaycastHit hit = s_RaycastHitsCache[i];
                if (!ValidateRaycastGameObject(hit.collider.gameObject))
                    continue;
                return hit.collider.gameObject;
            }

            return null;
        }




        
        private void UpdatePointerPosition()
        {
            Vector2 pointerPos = Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
            m_Pointer.ScreenPosition = pointerPos;

            GameObject objectOver = ResolveRaycast(out m_Pointer.IsOverUI);

            if (m_Pointer.GameObjectOver != objectOver)
                m_Pointer.GameObjectOver = objectOver;
        }


        private void HandleHoverEvent()
        {
            if (m_Pointer.LastHoverGameObject == m_Pointer.GameObjectOver)
                return;
            m_Pointer.LastHoverGameObject = m_Pointer.GameObjectOver;

            IHoverHandler[] nextHoverTargets = m_Pointer.GetEventComponents<IHoverHandler>();

            if (m_Pointer.HoverTargets != null)
            {
                foreach (IHoverHandler previousHoverTarget in m_Pointer.HoverTargets)
                {
                    if (previousHoverTarget != null && (nextHoverTargets == null || !nextHoverTargets.Contains(previousHoverTarget)))
                        previousHoverTarget.OnHoverExit(m_Pointer); // no longer hovering over this target, so trigger exit
                }
            }

            if (nextHoverTargets != null)
            {
                foreach (IHoverHandler nextHoverTarget in nextHoverTargets)
                {
                    if (m_Pointer.HoverTargets == null || !m_Pointer.HoverTargets.Contains(nextHoverTarget))
                        nextHoverTarget.OnHoverEnter(m_Pointer); // newly hovering over this target, so trigger enter
                }
            }

            m_Pointer.HoverTargets = nextHoverTargets;
        }



        private void HandleClickDownEvent(PointerEventData.InputTypes inputType)
        {
            if (m_Pointer.Clicks.ContainsKey(inputType))
            {
                d.LogError($"Trying to start click with input type that already exists! Aborting... FIX ME [{inputType}]");
                return;
            }

            UpdatePointerPosition();

            PointerEventData.Click click = new PointerEventData.Click
            {
                InputType = inputType,
                ScreenOrigin = m_Pointer.ScreenPosition,
                Handlers = m_Pointer.GetEventComponents<IClickHandlerBase>()
            };
            m_Pointer.Clicks.Add(inputType, click);

            if (click.Handlers != null)
            {
                foreach (IClickDownHandler clickDownHandler in click.Handlers.Where(r => r is IClickDownHandler).Select(r => r as IClickDownHandler))
                    clickDownHandler.OnClickDown(m_Pointer, click);
            }
        }

        private void UpdateClicks()
        {
            foreach (KeyValuePair<PointerEventData.InputTypes, PointerEventData.Click> clickKVP in m_Pointer.Clicks)
            {
                PointerEventData.Click click = clickKVP.Value;
                Vector2 screenDelta = m_Pointer.ScreenPosition - click.ScreenOrigin;

                if (!click.HasAttemptedDrag)
                {
                    float minorAxisSize = Mathf.Min(Screen.height, Screen.width);
                    if (screenDelta.magnitude / minorAxisSize > m_ClickDragThreshold)
                    {
                        click.HasAttemptedDrag = true;
                        HandleDragStartEvent(click);
                    }
                }

                if (click.Drag != null)
                    HandleDragUpdateEvent(click);
            }
        }

        private void HandleClickUpEvent(PointerEventData.InputTypes inputType)
        {
            if (!m_Pointer.Clicks.TryGetValue(inputType, out PointerEventData.Click click))
            {
                d.LogError($"Trying to end click with input type that does not exist! Aborting... FIX ME [{inputType}]");
                return;
            }

            UpdatePointerPosition();

            if (click.Drag != null)
                HandleDragStopEvent(click);

            if (click.Handlers != null)
            {
                // click up handlers from original click down event
                foreach (IClickUpHandler clickUpHandler in click.Handlers.Where(r => r is IClickUpHandler).Select(r => r as IClickUpHandler))
                    clickUpHandler.OnClickUp(m_Pointer, click);

                // Full click handlers from original click down event
                IClickHandler[] finalClickHandlers = m_Pointer.GetEventComponents<IClickHandler>(); 
                IEnumerable<IClickHandler> originalClickHandlers = click.Handlers.Where(r => r != null && r is IClickHandler).Select(r => r as IClickHandler);
                if (finalClickHandlers != null)
                {
                    foreach (IClickHandler originalClickHandler in originalClickHandlers)
                    {
                        if (originalClickHandler != null && finalClickHandlers.Contains(originalClickHandler))
                            originalClickHandler.OnClick(m_Pointer, click);
                    }
                }
            }

            m_Pointer.Clicks.Remove(inputType);
        }




        private void HandleDragStartEvent(PointerEventData.Click click)
        {
            PointerEventData.Drag drag = new PointerEventData.Drag
            {
                InputType = click.InputType,
                ScreenOrigin = click.ScreenOrigin,
                ScreenPosition = m_Pointer.ScreenPosition,
                Handlers = m_Pointer.GetEventComponents<IDragHandlerBase>()
            };

            if (drag.Handlers == null || 
                drag.Handlers.Any(r => r is IDragInitialiser dragInitialiser && !dragInitialiser.OnTryInitialiseDrag(m_Pointer, drag)))
                return; // if any drag initialiser fails, don't start the drag

            click.Drag = drag;

            foreach (IDragStartHandler dragHandler in drag.Handlers.Select(r => r as IDragStartHandler))
                dragHandler.OnDragStart(m_Pointer, drag);
        }

        private void HandleDragUpdateEvent(PointerEventData.Click click)
        {
            click.Drag.ScreenPosition = m_Pointer.ScreenPosition;

            foreach (IDragUpdateHandler dragHandler in click.Drag.Handlers.Where(r => r != null).Select(r => r as IDragUpdateHandler))
                dragHandler.OnDrag(m_Pointer, click.Drag);
        }

        private void HandleDragStopEvent(PointerEventData.Click click)
        {
            foreach (IDragEndHandler dragHandler in click.Drag.Handlers.Where(r => r != null).Select(r => r as IDragEndHandler))
                dragHandler.OnDragEnd(m_Pointer, click.Drag);

            click.Drag = null;
        }



        private void OnClick_Primary(InputAction.CallbackContext context)
        {
            if (context.started)
                HandleClickDownEvent(PointerEventData.InputTypes.Primary);
            else if (context.canceled)
                HandleClickUpEvent(PointerEventData.InputTypes.Primary);
        }


        private void OnClick_Secondary(InputAction.CallbackContext context)
        {
            if (context.started)
                HandleClickDownEvent(PointerEventData.InputTypes.Secondary);
            else if (context.canceled)
                HandleClickUpEvent(PointerEventData.InputTypes.Secondary);
        }


        private void Awake()
        {
            m_InputMapping.RereferenceAll(m_ActionAsset);
            AddInputCallbacks();
        }

        private void Update()
        {
            UpdatePointerPosition();

            HandleHoverEvent();

            UpdateClicks();
        }

        private void OnDestroy()
        {
            RemoveInputCallbacks();
        }
    }

    public class PointerEventData
    {
        public Vector2 ScreenPosition;
        public GameObject GameObjectOver;
        public bool IsOverUI;

        public GameObject LastHoverGameObject;
        public IHoverHandler[] HoverTargets;

        public Dictionary<InputTypes, Click> Clicks = new Dictionary<InputTypes, Click>();

        [System.Flags]
        public enum InputTypes : byte
        {
            Primary = 1 << 0,
            Secondary = 1 << 1,
        }

        public class Click
        {
            public InputTypes InputType;
            public Vector2 ScreenOrigin;
            public IClickHandlerBase[] Handlers;

            public bool HasAttemptedDrag;
            public Drag Drag;
        }

        public class Drag
        {
            public InputTypes InputType;
            public Vector2 ScreenOrigin;
            public Vector2 ScreenPosition;
            public IDragHandlerBase[] Handlers;

            public Vector2 ScreenDelta => ScreenPosition - ScreenOrigin;
        }

        public T[] GetEventComponents<T>()
        {
            if (GameObjectOver == null)
                return null;
            T firstComponent = GameObjectOver.transform.GetComponentInParents<T>();
            if (firstComponent == null)
                return null;
            return (firstComponent as Component).GetComponents<T>();
        }
    }
}
 