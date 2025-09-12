using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameAssets.ScriptableObjects.Core;

namespace VisualNovel.UI.Notifications
{
    using Animations;
    
    public class TraitNotification : GlobalNotification
    {
        [Header("UI Setup")]
        [SerializeField] private Image traitImage;
        [SerializeField] private TMP_Text traitNameTextField;
        [SerializeField] private TMP_Text traitDescriptionTextField;

        [Header("Animators")] 
        [SerializeField] private AnimatorSequence[] animatorSequences;

        [Serializable]
        public class AnimatorSequence
        {
            public UIAnimator animator;
            public float delayInSec;
        }

        private Coroutine showNotificationRoutine;
        
        public void Initialize(TraitSO targetTrait)
        {
            traitImage.sprite = targetTrait.traitIcon;
            traitNameTextField.text = targetTrait.traitName;
            traitDescriptionTextField.text = targetTrait.traitDescription;
        }
        
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

            // hiding everything at once
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
                        seq.animator.Play("Appear");
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
            }
        }
    }
}