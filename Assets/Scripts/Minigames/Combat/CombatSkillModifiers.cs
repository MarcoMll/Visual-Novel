using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;

namespace VisualNovel.Minigames.Combat
{
    public enum CombatModifierType
    {
        BaseDamage = 0,
    }

    public readonly struct CombatSkillModifier
    {
        public CombatSkillModifier(CombatModifierType type, int value)
        {
            Type = type;
            Value = value;
        }

        public CombatModifierType Type { get; }
        public int Value { get; }
    }

    public static class CombatSkillModifierUtility
    {
        public static IEnumerable<CombatSkillModifier> EnumerateModifiers(BaseSkill skill)
        {
            if (skill is not CombatSkill combatSkill)
            {
                yield break;
            }

            if (combatSkill.baseDamageBonus != 0)
            {
                yield return new CombatSkillModifier(CombatModifierType.BaseDamage, combatSkill.baseDamageBonus);
            }
        }
    }
}
