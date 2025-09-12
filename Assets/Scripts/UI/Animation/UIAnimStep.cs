using DG.Tweening;
using UnityEngine;

namespace VisualNovel.UI.Animations
{
    /// <summary>
    /// Base class for a single tween step used by <see cref="UIAnimator"/>.
    /// Implementations should modify the supplied RectTransform or CanvasGroup
    /// and return the tween that performs the animation.
    /// </summary>
    [System.Serializable]
    public abstract class UIAnimStep
    {
        /// <summary>Builds the tween for this step.</summary>
        public abstract Tween Build(RectTransform rect, CanvasGroup canvas);

        /// <summary>
        /// Creates a shallow copy of this step. This is used when ensuring
        /// each animation sequence has its own instance of a step so that
        /// editing one sequence's step does not affect others.
        /// </summary>
        public UIAnimStep Clone()
        {
            return (UIAnimStep)MemberwiseClone();
        }
    }
}
