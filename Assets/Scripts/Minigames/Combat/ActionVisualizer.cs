using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace VisualNovel.Minigames.Combat.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ActionVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Responsible for visualizing an action in the UI and handling its movement and visibility (fade/position).
        /// </summary>

        [Header("UI References")] [SerializeField]
        private Image actionIcon;

        [SerializeField] private TMP_Text actionNumberTextField;

        [Header("Positions (Anchored)")] [SerializeField]
        private Vector2 startPosition;

        [SerializeField] private Vector2 appearanceOffset;
        [SerializeField] private Vector2 endPosition;

        [Header("Fade Control")] [SerializeField]
        private CanvasGroup canvasGroup;

        private RectTransform _rectTransform;

        // Keep references so we can safely kill/replace tweens
        private Sequence _showSequence;
        private Tweener _moveTween;
        private Tweener _fadeTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            EnsureCanvasGroup();

            // Start hidden and at the offset position (so Show() can animate into startPosition)
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            _rectTransform.anchoredPosition = startPosition + appearanceOffset;
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void KillAllTweens()
        {
            _showSequence?.Kill();
            _moveTween?.Kill();
            _fadeTween?.Kill();

            // Defensive: kill any stray tweens targeting our components
            if (_rectTransform != null) _rectTransform.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }
#endif

        // ---------------------------
        // Public API
        // ---------------------------

        /// <summary>
        /// Sets up the visual content (sprite & number) and resets the initial hidden state.
        /// </summary>
        public void Setup(Sprite actionSprite, int number)
        {
            if (actionIcon != null)
            {
                actionIcon.sprite = actionSprite;
                actionIcon.enabled = actionSprite != null;
            }

            if (actionNumberTextField != null)
            {
                actionNumberTextField.text = number.ToString();
            }

            KillAllTweens();
            EnsureCanvasGroup();

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            _rectTransform.anchoredPosition = startPosition + appearanceOffset;
        }

        /// <summary>
        /// Plays the "appearance" effect: move from (start + offset) to start, while fading in.
        /// </summary>
        public void Show(float duration)
        {
            duration = Mathf.Max(0f, duration);
            KillAllTweens();

            // Ensure starting state
            _rectTransform.anchoredPosition = startPosition + appearanceOffset;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            _showSequence = DOTween.Sequence()
                .Join(_rectTransform
                    .DOAnchorPos(startPosition, duration)
                    .SetEase(Ease.OutCubic))
                .Join(canvasGroup
                    .DOFade(1f, duration)
                    .SetEase(Ease.OutCubic));
        }

        /// <summary>
        /// Smoothly moves the action to the configured endPosition.
        /// </summary>
        public void MoveToTarget(float duration)
        {
            duration = Mathf.Max(0f, duration);

            // Only replace position tween; keep fade state
            _rectTransform.DOKill();
            _moveTween = _rectTransform
                .DOAnchorPos(endPosition, duration)
                .SetEase(Ease.InOutQuad);
        }

        /// <summary>
        /// Smoothly fades the action out (keeps current position).
        /// </summary>
        public void Hide(float duration)
        {
            duration = Mathf.Max(0f, duration);

            canvasGroup.DOKill();
            _fadeTween = canvasGroup
                .DOFade(0f, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;
                });
        }

        /// <summary>
        /// Updates only the number displayed for the action without
        /// affecting position or alpha.
        /// </summary>
        public void SetNumber(int number)
        {
            if (actionNumberTextField != null)
            {
                actionNumberTextField.text = number.ToString();
            }
        }

        /// <summary>
        /// Immediately resets to the start state: position at start, alpha 0.
        /// </summary>
        public void Reset()
        {
            KillAllTweens();
            EnsureCanvasGroup();

            _rectTransform.anchoredPosition = startPosition;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
