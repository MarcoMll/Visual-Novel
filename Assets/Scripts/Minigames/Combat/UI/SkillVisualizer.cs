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
        private Func<BaseSkill, bool> _onSkillSelected;
        private Func<BaseSkill, bool> _onSkillDeselected;

        public BaseSkill Skill => _skill;
        public bool IsSelected => _isSelected;


        /// <summary>
        /// Sets up the visualizer for the provided skill.
        /// </summary>
        public void Initialize(
            BaseSkill skill,
            bool baseSkill,
            bool startSelected,
            bool skillAlreadyActive,
            Func<BaseSkill, bool> onSkillSelected = null,
            Func<BaseSkill, bool> onSkillDeselected = null)
        {
            _skill = skill;
            _isBaseSkill = baseSkill;
            _onSkillSelected = onSkillSelected;
            _onSkillDeselected = onSkillDeselected;

            SetupButtonListeners();

            _isSelected = false;

            if (startSelected)
            {
                if (skillAlreadyActive)
                {
                    _isSelected = true;
                    UpdateSelectionIndicator(true);
                }
                else
                {
                    if (SelectSkill() == false)
                    {
                        UpdateSelectionIndicator(false);
                    }
                }
            }
            else
            {
                UpdateSelectionIndicator(false);
            }

            UpdateSkillImage();
        }

        /// <summary>
        /// Marks the skill as selected in the UI and applies its effects.
        /// </summary>
        public bool SelectSkill(bool invokeCallback = true)
        {
            if (_isSelected)
            {
                return true;
            }

            if (invokeCallback && _skill != null)
            {
                var result = _onSkillSelected?.Invoke(_skill) ?? true;
                if (result == false)
                {
                    return false;
                }
            }

            _isSelected = true;
            UpdateSelectionIndicator(true);
            return true;
        }

        /// <summary>
        /// Marks the skill as deselected in the UI.
        /// </summary>
        public bool DeselectSkill(bool invokeCallback = true)
        {
            if (_isBaseSkill || _isSelected == false)
            {
                return false;
            }

            if (invokeCallback && _skill != null)
            {
                var result = _onSkillDeselected?.Invoke(_skill) ?? true;
                if (result == false)
                {
                    return false;
                }
            }

            _isSelected = false;
            UpdateSelectionIndicator(false);
            return true;
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
