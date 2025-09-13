using GameAssets.ScriptableObjects.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.UI.Notifications
{
    using UI;

    public class TraitNotification : AnimatedNotification
    {
        [Header("UI Setup")]
        [SerializeField] private Image traitImage;
        [SerializeField] private TMP_Text traitNameTextField;
        [SerializeField] private TMP_Text traitDescriptionTextField;
        [SerializeField] private ExtendedButton readButton;

        public void Initialize(TraitSO targetTrait)
        {
            traitImage.sprite = targetTrait.traitIcon;
            traitNameTextField.text = targetTrait.traitName;
            traitDescriptionTextField.text = targetTrait.traitDescription;
        }

        public override ExtendedButton ReadButton => readButton;
    }
}
