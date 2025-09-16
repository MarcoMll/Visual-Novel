using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Minigames.Combat
{
    /// <summary>
    /// Represents the real-time state of a fighter during the combat minigame.
    /// Keeps track of current health and action points per round
    /// independent from the initialization data.
    /// </summary>
    public class FighterRuntime
    {
        public FighterBaseStats BaseStats { get; }
        public int CurrentHealth { get; private set; }
        public int ActionPointsPerRound { get; private set; }
        public int BaseDamage => BaseStats.baseDamage + GetModifierValue(CombatModifierType.BaseDamage);
        public IReadOnlyList<ItemSO> Items => _items;
        public IReadOnlyCollection<BaseSkill> ActiveSkills => _activeSkills;

        public event Action<int> OnHealthChanged;
        public event Action<CombatModifierType> OnCombatModifierChanged;

        private readonly List<ItemSO> _items = new();
        private readonly Dictionary<CombatModifierType, int> _modifierTotals = new();
        private readonly HashSet<BaseSkill> _activeSkills = new();

        public FighterRuntime(FighterBaseStats baseStats)
        {
            BaseStats = baseStats ?? new FighterBaseStats();
            CurrentHealth = BaseStats.baseHealthPoints;
            ActionPointsPerRound = BaseStats.baseActionPoints;
            SetItems(BaseStats.startingItems);
        }

        public bool IsAlive => CurrentHealth > 0;

        public void TakeDamage(int amount)
        {
            var previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            var delta = CurrentHealth - previousHealth;
            OnHealthChanged?.Invoke(delta);
        }

        public void ApplyRest(int restPoints)
        {
            ActionPointsPerRound += restPoints;
        }

        public void SetItems(IEnumerable<ItemSO> items)
        {
            _items.Clear();
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item != null)
                {
                    _items.Add(item);
                }
            }
        }

        public bool AddItem(ItemSO item)
        {
            if (item == null || _items.Contains(item))
            {
                return false;
            }

            _items.Add(item);
            return true;
        }

        public bool RemoveItem(ItemSO item)
        {
            return item != null && _items.Remove(item);
        }

        public bool ApplySkill(BaseSkill skill)
        {
            if (skill == null || _activeSkills.Add(skill) == false)
            {
                return false;
            }

            foreach (var modifier in CombatSkillModifierUtility.EnumerateModifiers(skill))
            {
                ModifyModifierValue(modifier.Type, modifier.Value);
            }

            return true;
        }

        public bool RemoveSkill(BaseSkill skill)
        {
            if (skill == null || _activeSkills.Remove(skill) == false)
            {
                return false;
            }

            foreach (var modifier in CombatSkillModifierUtility.EnumerateModifiers(skill))
            {
                ModifyModifierValue(modifier.Type, -modifier.Value);
            }

            return true;
        }

        public int GetModifierValue(CombatModifierType type)
        {
            return _modifierTotals.TryGetValue(type, out var value) ? value : 0;
        }

        private void ModifyModifierValue(CombatModifierType type, int delta)
        {
            if (delta == 0)
            {
                return;
            }

            _modifierTotals.TryGetValue(type, out var current);
            var newValue = current + delta;

            if (newValue == 0)
            {
                _modifierTotals.Remove(type);
            }
            else
            {
                _modifierTotals[type] = newValue;
            }

            OnCombatModifierChanged?.Invoke(type);
        }
    }
}
