using DG.Tweening;
using UnityEngine;

namespace VisualNovel.UI.Animations
{
    /// <summary>Animates the anchored position of a RectTransform.</summary>
    [System.Serializable]
    public class MoveStep : UIAnimStep
    {
        public Vector2 from;
        public Vector2 to;
        public float duration = 0.5f;
        public Ease ease = Ease.InOutQuad;

        public override Tween Build(RectTransform rect, CanvasGroup canvas)
        {
            rect.anchoredPosition = from;
            return rect.DOAnchorPos(to, duration).SetEase(ease);
        }
    }
}
