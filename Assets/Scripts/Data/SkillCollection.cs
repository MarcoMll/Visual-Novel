using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Data
{
    /// <summary>
    /// Tracks the player's acquired skills.
    /// </summary>
    [Serializable]
    public class SkillCollection : BaseGameData
    {
        [SerializeField] private List<string> skillGuids = new();
        private readonly List<BaseSkill> skills = new();

        protected override string SaveKey => "SKILL_DATA";

        public IReadOnlyList<BaseSkill> Skills => skills;

        public void AddSkill(BaseSkill skill)
        {
            if (skill == null || skills.Contains(skill)) return;
            skills.Add(skill);
        }

        public void RemoveSkill(BaseSkill skill)
        {
            if (skill == null) return;
            skills.Remove(skill);
        }

        public bool HasSkill(BaseSkill skill) => skills.Contains(skill);

        public override void Save()
        {
            skillGuids.Clear();
            foreach (var skill in skills)
            {
                skillGuids.Add(skill.Guid);
            }
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            skills.Clear();
            foreach (var guid in skillGuids)
            {
                var skill = BaseSO.GetByGuid<BaseSkill>(guid);
                if (skill != null)
                {
                    skills.Add(skill);
                }
            }
        }
    }
}
