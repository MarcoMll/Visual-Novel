using DG.Tweening;
using UnityEngine;

namespace VisualNovel.UI.Animations
{
    /// <summary>Animates the local scale of a RectTransform.</summary>
    [System.Serializable]
    public class ScaleStep : UIAnimStep
    {
        public Vector3 from = Vector3.one;
        public Vector3 to = Vector3.one;
        public float duration = 0.5f;
        public Ease ease = Ease.InOutQuad;

        public override Tween Build(RectTransform rect, CanvasGroup canvas)
        {
            rect.localScale = from;
            return rect.DOScale(to, duration).SetEase(ease);
        }
    }
}