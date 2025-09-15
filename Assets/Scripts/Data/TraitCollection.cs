using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Data
{
    /// <summary>
    /// Tracks the player's acquired traits.
    /// </summary>
    [Serializable]
    public class TraitCollection : BaseGameData
    {
        [SerializeField] private List<string> traitGuids = new();
        private readonly List<TraitSO> traits = new();

        protected override string SaveKey => "TRAIT_DATA";

        public IReadOnlyList<TraitSO> Traits => traits;

        public void AddTrait(TraitSO trait)
        {
            if (trait == null || traits.Contains(trait)) return;
            traits.Add(trait);
            if (trait.skills != null)
            {
                foreach (var skill in trait.skills)
                {
                    GameDataManager.Instance.playerSkillCollection.AddSkill(skill);
                }
            }
        }

        public void RemoveTrait(TraitSO trait)
        {
            if (trait == null) return;
            traits.Remove(trait);
        }

        public bool HasTrait(TraitSO trait) => traits.Contains(trait);

        public override void Save()
        {
            traitGuids.Clear();
            foreach (var trait in traits)
            {
                traitGuids.Add(trait.Guid);
            }
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            traits.Clear();
            foreach (var guid in traitGuids)
            {
                var trait = BaseSO.GetByGuid<TraitSO>(guid);
                if (trait != null)
                {
                    traits.Add(trait);
                }
            }
        }
    }
}

