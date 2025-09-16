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
        
        [Header("Selection Indicator")]
        [SerializeField] private Image selectionIndicatorImage;
        
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite idleSprite;

        private BaseSkill _skill;
        private bool _isBaseSkill;

        public BaseSkill Skill => _skill;
        

        /// <summary>
        /// Sets up the visualizer for the provided skill.
        /// </summary>
        public void Initialize(BaseSkill skill, bool baseSkill)
        {
            _skill = skill;
            _isBaseSkill = baseSkill;

            if (_isBaseSkill)
                SelectSkill();
            
            UpdateSkillImage();
        }

        /// <summary>
        /// Marks the skill as selected in the UI and applies its effects.
        /// </summary>
        public void SelectSkill()
        {
            UpdateSelectionIndicator(true);
        }

        /// <summary>
        /// Marks the skill as deselected in the UI.
        /// </summary>
        public void DeselectSkill()
        {
            if (_isBaseSkill)
            {
                return;
            }
            
            UpdateSelectionIndicator(false);
        }

        private void UpdateSelectionIndicator(bool selected)
        {
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
    }
}
