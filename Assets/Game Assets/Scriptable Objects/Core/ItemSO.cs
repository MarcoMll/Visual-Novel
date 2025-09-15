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
        [Header("Item effects")]
        [ShowIfIs(nameof(itemType), ItemType.Weapon)]
        [Indent(-1)] public CombatSkill baseWeaponSkill;
        public List<BaseSkill> skills = new();
    }
}
