using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.UI.Animations
{
    [RequireComponent(typeof(Image))]
    public class UISpriteAnimator : MonoBehaviour
    {
        [SerializeField] private float animationDurationInSeconds;
        [SerializeField] private Sprite[] spritesSheet;

        private Image _image = null;
        private int _spriteIndex = 0;
        private float _timer = 0;

        private void Start()
        {
            _image = GetComponent<Image>();
        }
        
        private void Update()
        {
            if ((_timer+=Time.deltaTime) >= (animationDurationInSeconds / spritesSheet.Length))
            {
                _timer = 0;
                _image.sprite = spritesSheet[_spriteIndex];
                _spriteIndex = (_spriteIndex + 1) % spritesSheet.Length;
            }
        }
    }
}
