using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Minigames.Combat
{
    [Serializable]
    public class FighterBaseStats
    {
        public CharacterSO characterReference;
        public CharacterSO.CharacterEmotion characterEmotion;
        public int baseHealthPoints = 100;
        public int baseActionPoints = 5;
        public int baseDamage = 5;
        public Vector2 characterScale = Vector2.one;
        public int layer;
        public List<ItemSO> startingItems = new();
    }
}