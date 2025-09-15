using GameAssets.ScriptableObjects.Core;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.Minigames.Combat.UI
{
    /// <summary>
    /// Handles the visualization logic for a single skill entry inside the combat UI.
    /// </summary>
    public class SkillVisualizer : MonoBehaviour
    {
        [SerializeField] private Image skillIcon;
        [SerializeField] private Toggle toggle;

        private BaseSkill _skill;
        private bool _isBaseSkill;

        public BaseSkill Skill => _skill;
        public bool IsSelected => toggle != null && toggle.isOn;

        private void Awake()
        {
            ConfigureToggle();

            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(HandleToggleValueChanged);
            }
        }

        private void OnDestroy()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
            }
        }

        private void OnValidate()
        {
            ConfigureToggle();
            UpdateIcon();
        }

        /// <summary>
        /// Sets up the visualizer for the provided skill.
        /// </summary>
        public void Initialize(BaseSkill skill, bool baseSkill)
        {
            _skill = skill;
            _isBaseSkill = baseSkill;

            ConfigureToggle();
            UpdateIcon();
            UpdateToggleState(_isBaseSkill);
        }

        /// <summary>
        /// Marks the skill as selected in the UI.
        /// </summary>
        public void Select()
        {
            UpdateToggleState(true);
        }

        /// <summary>
        /// Marks the skill as deselected in the UI.
        /// </summary>
        public void Deselect()
        {
            if (_isBaseSkill)
            {
                UpdateToggleState(true);
                return;
            }

            UpdateToggleState(false);
        }

        private void ConfigureToggle()
        {
            if (toggle == null)
            {
                return;
            }

            toggle.transition = Selectable.Transition.SpriteSwap;
        }

        private void HandleToggleValueChanged(bool isOn)
        {
            if (_isBaseSkill && !isOn)
            {
                UpdateToggleState(true);
                return;
            }

            UpdateToggleState(isOn);
        }

        private void UpdateToggleState(bool isOn)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.SetIsOnWithoutNotify(isOn);
            toggle.interactable = !_isBaseSkill;
            toggle.PlayEffect(true);
        }

        private void UpdateIcon()
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
    }
}
