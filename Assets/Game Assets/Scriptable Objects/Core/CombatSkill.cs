using UnityEngine;
using CustomInspector;

namespace GameAssets.ScriptableObjects.Core
{
    [CreateAssetMenu(menuName = "Visual Novel/Skills/New Combat skill")]
    public class CombatSkill : BaseSkill
    {
        [Header("Effects")]
        public int baseDamageBonus;
    }
}
