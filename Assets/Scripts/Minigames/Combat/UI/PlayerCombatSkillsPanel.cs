using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Minigames.Combat.UI
{
    /// <summary>
    /// Manages the skill sections displayed within the combat player's skills panel.
    /// </summary>
    public class PlayerCombatSkillsPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform playerSkillsPanel;
        [SerializeField] private SkillsSection skillsSectionPrefab;

        private readonly List<SkillsSection> spawnedSections = new();

        /// <summary>
        /// Removes any spawned skill sections from the panel.
        /// </summary>
        public void ClearSections()
        {
            foreach (var section in spawnedSections)
            {
                if (section != null)
                {
                    Destroy(section.gameObject);
                }
            }

            spawnedSections.Clear();
        }

        /// <summary>
        /// Creates a new section inside the panel.
        /// </summary>
        public SkillsSection CreateSection(IList<BaseSkill> skills, string sectionTitle, ISet<BaseSkill> baseSkills = null)
        {
            if (playerSkillsPanel == null || skillsSectionPrefab == null)
            {
                return null;
            }

            var sectionInstance = Instantiate(skillsSectionPrefab, playerSkillsPanel);
            sectionInstance.Initialize(skills, sectionTitle, baseSkills);
            spawnedSections.Add(sectionInstance);
            return sectionInstance;
        }
    }
}
