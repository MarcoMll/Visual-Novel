using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VisualNovel.UI
{
    /// <summary>
    /// Adds left- and right-click UnityEvents to a standard UI Button while preserving
    /// all default visual transitions (color tint, sprite swap, animation).
    /// Attach to the same GameObject as your Button.
    /// </summary>

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class ExtendedButton: MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, ISubmitHandler
    {
        [Serializable]
        public class ButtonClickEvent : UnityEvent { }

        [Header("Target")]
        [Tooltip("Button whose visual transitions and base behavior are preserved.")]
        [SerializeField] private Button targetButton;

        [Header("Extended Click Events")]
        [Tooltip("Invoked on left mouse click (in addition to the Button.onClick).")]
        [SerializeField] private ButtonClickEvent onLeftClick = new ButtonClickEvent();

        [Tooltip("Invoked on right mouse click. Visual 'pressed' transition is simulated.")]
        [SerializeField] private ButtonClickEvent onRightClick = new ButtonClickEvent();

        /// <summary>Subscribe in code if needed.</summary>
        public UnityEvent OnLeftClick => onLeftClick;
        /// <summary>Subscribe in code if needed.</summary>
        public UnityEvent OnRightClick => onRightClick;

        // Track if we simulated a left-press for right-click visuals
        private bool _simulatingRightPress;

        private void Reset()
        {
            targetButton = GetComponent<Button>();
        }

        private void Awake()
        {
            if (targetButton == null)
                targetButton = GetComponent<Button>();
        }

        private void OnDisable()
        {
            // Safety: if object is disabled mid-press, clear simulated state
            _simulatingRightPress = false;
        }

        // ------------------------------------------------------------
        // Pointer Flow
        // ------------------------------------------------------------

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsUsable())
                return;

            // Let the Button handle all LEFT flows naturally via its own handlers.
            // For RIGHT press, simulate a left-press so the transition shows "Pressed".
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                SimulateButtonPointerDown(eventData);
                _simulatingRightPress = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsUsable())
                return;

            // Mirror the simulated flow: release the "Pressed" visual on right button up.
            if (eventData.button == PointerEventData.InputButton.Right && _simulatingRightPress)
            {
                SimulateButtonPointerUp(eventData);
                _simulatingRightPress = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsUsable())
                return;

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    // Do NOT call targetButton.OnPointerClick here: the base Button already receives
                    // the same event from the EventSystem and will run its normal logic/transitions.
                    onLeftClick?.Invoke();
                    break;

                case PointerEventData.InputButton.Right:
                    // Keep base behavior intact (which ignores right-click), but expose our event.
                    onRightClick?.Invoke();
                    break;

                default:
                    break;
            }
        }

        // Treat keyboard/gamepad Submit as a left-click extension.
        public void OnSubmit(BaseEventData eventData)
        {
            if (!IsUsable())
                return;

            // Base Button will also receive Submit and run its transitions/onClick;
            // we only add our extra left-click event.
            onLeftClick?.Invoke();
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------

        private bool IsUsable()
        {
            return targetButton != null &&
                   targetButton.interactable &&
                   targetButton.gameObject.activeInHierarchy;
        }

        // Simulate left-button transitions on the target Button to keep visuals consistent
        // for right-clicks. We do NOT simulate the actual Click to avoid invoking Button.onClick.
        private void SimulateButtonPointerDown(PointerEventData source)
        {
            if (EventSystem.current == null) return;

            var fake = CreateSyntheticEvent(source, PointerEventData.InputButton.Left);
            targetButton.OnPointerDown(fake);
        }

        private void SimulateButtonPointerUp(PointerEventData source)
        {
            if (EventSystem.current == null) return;

            var fake = CreateSyntheticEvent(source, PointerEventData.InputButton.Left);
            targetButton.OnPointerUp(fake);
        }

        private static PointerEventData CreateSyntheticEvent(PointerEventData src, PointerEventData.InputButton asButton)
        {
            var e = new PointerEventData(EventSystem.current)
            {
                button = asButton,
                position = src.position,
                delta = src.delta,
                pressPosition = src.pressPosition,
                pointerId = src.pointerId,
                pointerEnter = src.pointerEnter,
                pointerPressRaycast = src.pointerPressRaycast,
                clickCount = src.clickCount,
                eligibleForClick = true,
                useDragThreshold = src.useDragThreshold
            };
            return e;
        }
    }
}
