using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.UI.Animations
{
    [RequireComponent(typeof(Image))]
    public class UISpriteAnimator : MonoBehaviour
    {
        [SerializeField] private float animationDurationInSeconds = 1f;
        [SerializeField] private Sprite[] spritesSheet;
        [SerializeField] private bool loop = true; // <-- new

        private Image _image = null;
        private int _spriteIndex = 0;
        private float _timer = 0f;

        private void Start()
        {
            _image = GetComponent<Image>();

            // Optional: show the first sprite immediately if available
            if (spritesSheet != null && spritesSheet.Length > 0)
            {
                _image.sprite = spritesSheet[0];
            }
        }

        private void Update()
        {
            if (spritesSheet == null || spritesSheet.Length == 0) return;

            float frameTime = (animationDurationInSeconds > 0f)
                ? animationDurationInSeconds / spritesSheet.Length
                : 0f;
            if (frameTime <= 0f) return;

            if ((_timer += Time.deltaTime) >= frameTime)
            {
                _timer = 0f;

                // Display current sprite
                _image.sprite = spritesSheet[_spriteIndex];

                // Advance or stop depending on loop
                if (_spriteIndex >= spritesSheet.Length - 1)
                {
                    if (loop)
                    {
                        _spriteIndex = 0;
                    }
                    else
                    {
                        // Stay on the last sprite and stop updating
                        enabled = false;
                    }
                }
                else
                {
                    _spriteIndex++;
                }
            }
        }
    }
}