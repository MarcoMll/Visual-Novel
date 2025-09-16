using System;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;
using UnityEngine.UI;
using VisualNovel.UI;

namespace VisualNovel.Minigames.Combat.UI
{
    /// <summary>
    /// Handles the visualization logic for a single skill entry inside the combat UI.
    /// </summary>
    public class SkillVisualizer : MonoBehaviour
    {
        [SerializeField] private Image skillIcon;
        [SerializeField] private ExtendedButton skillButton;

        [Header("Selection Indicator")]
        [SerializeField] private Image selectionIndicatorImage;

        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite idleSprite;

        private BaseSkill _skill;
        private bool _isBaseSkill;
        private bool _isSelected;
        private Action<BaseSkill> _onSkillSelected;
        private Action<BaseSkill> _onSkillDeselected;

        public BaseSkill Skill => _skill;
        public bool IsSelected => _isSelected;


        /// <summary>
        /// Sets up the visualizer for the provided skill.
        /// </summary>
        public void Initialize(
            BaseSkill skill,
            bool baseSkill,
            Action<BaseSkill> onSkillSelected = null,
            Action<BaseSkill> onSkillDeselected = null)
        {
            _skill = skill;
            _isBaseSkill = baseSkill;
            _onSkillSelected = onSkillSelected;
            _onSkillDeselected = onSkillDeselected;

            SetupButtonListeners();

            if (_isBaseSkill)
            {
                SelectSkill();
            }
            else
            {
                _isSelected = false;
                UpdateSelectionIndicator(false);
            }

            UpdateSkillImage();
        }

        /// <summary>
        /// Marks the skill as selected in the UI and applies its effects.
        /// </summary>
        public void SelectSkill(bool invokeCallback = true)
        {
            if (_isSelected)
            {
                return;
            }

            _isSelected = true;
            UpdateSelectionIndicator(true);

            if (invokeCallback && _skill != null)
            {
                _onSkillSelected?.Invoke(_skill);
            }
        }

        /// <summary>
        /// Marks the skill as deselected in the UI.
        /// </summary>
        public void DeselectSkill(bool invokeCallback = true)
        {
            if (_isBaseSkill || _isSelected == false)
            {
                return;
            }

            _isSelected = false;
            UpdateSelectionIndicator(false);

            if (invokeCallback && _skill != null)
            {
                _onSkillDeselected?.Invoke(_skill);
            }
        }

        private void UpdateSelectionIndicator(bool selected)
        {
            if (selectionIndicatorImage == null)
            {
                return;
            }

            selectionIndicatorImage.sprite = selected ? selectedSprite : idleSprite;
        }

        private void UpdateSkillImage()
        {
            if (skillIcon == null)
            {
                return;
            }

            if (_skill != null && _skill.skillSprite != null)
            {
                skillIcon.sprite = _skill.skillSprite;
                skillIcon.enabled = true;
            }
            else
            {
                skillIcon.sprite = null;
                skillIcon.enabled = false;
            }
        }

        private void SetupButtonListeners()
        {
            if (skillButton == null)
            {
                skillButton = GetComponent<ExtendedButton>();
            }

            if (skillButton == null)
            {
                return;
            }

            skillButton.OnLeftClick.RemoveListener(HandleLeftClick);
            skillButton.OnLeftClick.AddListener(HandleLeftClick);
            skillButton.OnRightClick.RemoveListener(HandleRightClick);
            skillButton.OnRightClick.AddListener(HandleRightClick);
        }

        private void HandleLeftClick()
        {
            SelectSkill();
        }

        private void HandleRightClick()
        {
            DeselectSkill();
        }
    }
}
