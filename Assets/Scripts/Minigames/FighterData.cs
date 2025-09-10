using System;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Minigames.Combat
{
    [Serializable]
    public class FighterData
    {
        public CharacterSO characterReference;
        public CharacterSO.CharacterEmotion characterEmotion;
        public int baseHealthPoints = 100;
        public int baseActionPoints = 5;
        public int baseDamage = 5;
    }
}