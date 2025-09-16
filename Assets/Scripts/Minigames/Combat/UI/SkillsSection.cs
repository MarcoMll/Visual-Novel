using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.Minigames.Combat.UI
{
    /// <summary>
    /// Represents a group of skills displayed under a common section header.
    /// </summary>
    public class SkillsSection : MonoBehaviour
    {
        [SerializeField] private TMP_Text sectionTitleText;
        [SerializeField] private Transform skillsGrid;
        [SerializeField] private SkillVisualizer skillVisualizerPrefab;
        [SerializeField] private LayoutElementResizer layoutElementResizer;

        private readonly List<SkillVisualizer> spawnedVisualizers = new();

        /// <summary>
        /// Populates the section with skill visualizers.
        /// </summary>
        public void Initialize(IList<BaseSkill> skills, string sectionTitle, ISet<BaseSkill> baseSkills = null)
        {
            if (sectionTitleText != null)
            {
                sectionTitleText.text = sectionTitle;
            }

            ClearVisualizers();

            if (skills != null && skillsGrid != null && skillVisualizerPrefab != null)
            {
                foreach (var skill in skills)
                {
                    if (skill == null)
                    {
                        continue;
                    }

                    var visualizer = Instantiate(skillVisualizerPrefab, skillsGrid);
                    var isBaseSkill = baseSkills != null && baseSkills.Contains(skill);
                    visualizer.Initialize(skill, isBaseSkill);
                    spawnedVisualizers.Add(visualizer);
                }
            }

            layoutElementResizer?.Recalculate();
        }

        private void ClearVisualizers()
        {
            if (skillsGrid != null)
            {
                for (var i = skillsGrid.childCount - 1; i >= 0; i--)
                {
                    var child = skillsGrid.GetChild(i);
                    Destroy(child.gameObject);
                }
            }

            spawnedVisualizers.Clear();
        }
    }
}
