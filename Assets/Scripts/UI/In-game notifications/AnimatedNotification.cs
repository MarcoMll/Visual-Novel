using System;
using System.Collections;
using Coffee.UIExtensions;
using UnityEngine;

namespace VisualNovel.UI.Notifications
{
    using Animations;
    using Audio;

    public abstract class AnimatedNotification : GlobalNotification
    {
        [Header("Animators")]
        [SerializeField] private AnimatorSequence[] animatorSequences;

        [Serializable]
        public class AnimatorSequence
        {
            public UIAnimator animator;
            public UIParticle uiParticle;
            public AudioClip sfx;
            public float delayInSec;
        }

        private Coroutine showNotificationRoutine;

        public override void Show()
        {
            if (showNotificationRoutine != null)
            {
                StopCoroutine(showNotificationRoutine);
                showNotificationRoutine = null;
            }
            showNotificationRoutine = StartCoroutine(ShowNotificationRoutine());
        }

        public override void Hide()
        {
            if (showNotificationRoutine != null)
            {
                StopCoroutine(showNotificationRoutine);
                showNotificationRoutine = null;
            }

            if (animatorSequences == null) return;
            foreach (var seq in animatorSequences)
            {
                if (seq?.animator != null)
                    seq.animator.Play("Hide");
            }
        }

        private IEnumerator ShowNotificationRoutine()
        {
            if (animatorSequences == null || animatorSequences.Length == 0)
                yield break;

            // If ALL delays are zero -> play all at once.
            bool allZeroDelay = true;
            foreach (var seq in animatorSequences)
            {
                if (seq != null && seq.delayInSec > 0f)
                {
                    allZeroDelay = false;
                    break;
                }
            }

            if (allZeroDelay)
            {
                foreach (var seq in animatorSequences)
                {
                    if (seq?.animator != null)
                    {
                        seq.animator.Play("Appear");
                        if (seq.uiParticle != null)
                        {
                            seq.uiParticle.Play();
                        }
                        if (seq.sfx != null)
                        {
                            AudioHandler.Instance.PlaySfx(seq.sfx);
                        }
                    }
                }
                yield break;
            }

            // Otherwise: play one-by-one, respecting each item's delay (relative to the previous trigger).
            foreach (var seq in animatorSequences)
            {
                if (seq == null || seq.animator == null)
                    continue;

                if (seq.delayInSec > 0f)
                    yield return new WaitForSeconds(seq.delayInSec);

                seq.animator.Play("Appear");

                if (seq.uiParticle != null)
                {
                    seq.uiParticle.Play();
                }

                if (seq.sfx != null)
                {
                    AudioHandler.Instance.PlaySfx(seq.sfx);
                }
            }
        }
    }
}
