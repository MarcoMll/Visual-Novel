using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

namespace VisualNovel.Decisions
{
    /// <summary>
    /// Represents a single choice entry. Handles its own styling and
    /// exposes pointer events so that external systems can react to user
    /// interactions.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class DecisionOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Styling")]
        public Color idleColor = Color.white;
        public Color hoverColor = Color.yellow;
        public Vector2 hoverOffset = new Vector2(0, 10);

        [Header("Smoothing")]
        public float textMoveSpeed = 8f;

        private TMP_Text _text;
        private RectTransform _rt;
        private Vector2 _basePos;
        private Vector2 _targetPos;
        private UnityAction _clickAction;

        /// <summary>Raised when the pointer enters this option.</summary>
        public event Action<DecisionOption> Hovered;
        /// <summary>Raised when the pointer exits this option.</summary>
        public event Action<DecisionOption> Exited;
        /// <summary>Raised when this option is clicked.</summary>
        public event Action<DecisionOption> Clicked;

        void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _rt = GetComponent<RectTransform>();
            _basePos = _rt.anchoredPosition;
            _targetPos = _basePos;
        }

        void Update()
        {
            _rt.anchoredPosition = Vector2.Lerp(
                _rt.anchoredPosition,
                _targetPos,
                textMoveSpeed * Time.deltaTime
            );
        }

        /// <summary>Configure the option with display text and a click action.</summary>
        public void Setup(string text, UnityAction onClick)
        {
            _text.text = text;
            _clickAction = onClick;
            ResetStyle();
            gameObject.SetActive(true);
        }

        /// <summary>Resets styling back to idle state.</summary>
        public void ResetStyle()
        {
            _text.color = idleColor;
            _targetPos = _basePos;
            _rt.anchoredPosition = _basePos;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Hovered?.Invoke(this);
            _text.color = hoverColor;
            _targetPos = _basePos + hoverOffset;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Do not reset styling on exit so that the option remains selected
            // until another option is hovered.
            Exited?.Invoke(this);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
            _clickAction?.Invoke();
        }

        /// <summary>Current anchored position of the option.</summary>
        public Vector2 AnchoredPosition => _rt.anchoredPosition;
    }
}
