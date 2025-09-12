using DG.Tweening;
using UnityEngine;

namespace VisualNovel.UI.Animations
{
    /// <summary>Animates the alpha of a CanvasGroup.</summary>
    [System.Serializable]
    public class FadeStep : UIAnimStep
    {
        public float from = 0f;
        public float to = 1f;
        public float duration = 0.5f;
        public Ease ease = Ease.Linear;

        public override Tween Build(RectTransform rect, CanvasGroup canvas)
        {
            canvas.alpha = from;
            return canvas.DOFade(to, duration).SetEase(ease);
        }
    }
}
