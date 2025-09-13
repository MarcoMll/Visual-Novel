using System;
using System.Collections;
using Coffee.UIExtensions;
using GameAssets.ScriptableObjects.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace VisualNovel.UI.Notifications
{
    using Animations;
    
    public class ItemNotification : GlobalNotification
    {
        [Header("UI Setup")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemNameTextField;
        [SerializeField] private TMP_Text itemDescriptionTextField;
        [SerializeField] private ExtendedButton readButton;

        [Header("Animators")] 
        [SerializeField] private AnimatorSequence[] animatorSequences;

        [Serializable]
        public class AnimatorSequence
        {
            public UIAnimator animator;
            public UIParticle uiParticle;
            public float delayInSec;
        }

        private Coroutine showNotificationRoutine;
        
        public void Initialize(ItemSO targetItem)
        {
            itemIcon.sprite = targetItem.itemIcon;
            itemNameTextField.text = targetItem.itemName;
            itemDescriptionTextField.text = targetItem.itemDescription;
        }

        public override ExtendedButton ReadButton => readButton;
        
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
                    {
                        seq.animator.Play("Appear");
                        if (seq.uiParticle != null)
                        {
                            seq.uiParticle.Play();
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
            }
        }
    }
}