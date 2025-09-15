using System.Collections.Generic;
using CustomInspector;
using UnityEngine;

namespace GameAssets.ScriptableObjects.Core
{
    [CreateAssetMenu(menuName = "VNE/New item")]
    public class ItemSO : BaseSO
    {
        public string itemName;
        public string itemDescription;
        public Sprite itemIcon;
        public ItemType itemType = ItemType.Trinket;
        public List<BaseSkill> skills = new();
        [ShowIf(nameof(itemType), ItemType.Weapon)]
        public CombatSkill baseWeaponSkill;
    }
}
