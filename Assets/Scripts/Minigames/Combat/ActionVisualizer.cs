using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VisualNovel.UI.Animations;

namespace VisualNovel.Minigames.Combat.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimator))]
    public sealed class ActionVisualizer : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        private Image actionIcon;

        [SerializeField] private TMP_Text actionNumberTextField;

        UIAnimator _animator;

        void Awake()
        {
            _animator = GetComponent<UIAnimator>();
        }

        /// <summary>
        /// Sets up the visual content (sprite & number).
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
        }

        public void Show(float _duration) => _animator.Play("Appear");
        public void MoveToTarget(float _duration) => _animator.Play("Move");
        public void Hide(float _duration) => _animator.Play("Hide");

        public void SetNumber(int number)
        {
            if (actionNumberTextField != null)
            {
                actionNumberTextField.text = number.ToString();
            }
        }

        public void Reset()
        {
            _animator.Stop();
        }
    }
}
