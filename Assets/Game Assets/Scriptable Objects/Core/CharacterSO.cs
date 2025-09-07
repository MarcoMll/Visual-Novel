using System;
using UnityEngine;

namespace GameAssets.ScriptableObjects.Core
{
    [CreateAssetMenu(menuName = "VNE/New character")]
    public class CharacterSO : BaseSO
    {
        [Serializable]
        public class CharacterEmotion
        {
            public string spriteName;
            public Sprite sprite;
        }
        
        public string characterName;
        public CharacterEmotion[] characterEmotionSpriteSheet;
        
        public Sprite GetEmotionSpriteByName(string emotionName)
        {
            foreach (var emotion in characterEmotionSpriteSheet)
            {
                if (emotion.spriteName == emotionName)
                    return emotion.sprite;
            }

            Debug.LogWarning($"Emotion '{emotionName}' not found!");
            return null;
        }
    }
}