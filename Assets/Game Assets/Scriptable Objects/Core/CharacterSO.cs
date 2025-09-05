using System;
using UnityEngine;

namespace GameAssets.ScriptableObjects.Core
{
    [CreateAssetMenu(menuName = "VNE/New character")]
    public class CharacterSO : ScriptableObject
    {
        [Serializable]
        public class CharacterEmotionSprite
        {
            public string spriteName;
            public Sprite sprite;
        }
        
        public string characterName;
        public CharacterEmotionSprite[] characterEmotionSpriteSheet;
    }
}