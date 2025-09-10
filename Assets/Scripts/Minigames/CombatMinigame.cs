using System.Collections.Generic;
using UnityEngine;
using VisualNovel.UI.Dynamic;

namespace VisualNovel.Minigames.Combat
{
    /// <summary>
    /// Example implementation of a minigame. This is only a placeholder
    /// to demonstrate how to derive from <see cref="MinigameBase"/>.
    /// </summary>
    public class CombatMinigame : MinigameBase
    {
        [SerializeField] private UIHealthBar enemyHealthBar;
        [SerializeField] private Transform enemySpriteRoot;

        public List<FighterData> Fighters { get; private set; } = new();

        private FighterData _player;
        private FighterData _enemy;

        public void Initialize(List<FighterData> fighters)
        {
            Fighters = fighters ?? new List<FighterData>();
        }

        protected override void OnStart()
        {
            _player = new FighterData();

            if (Fighters != null && Fighters.Count > 0)
            {
                _enemy = Fighters[0];
                SpawnEnemy(_enemy);
            }

            Debug.Log("Combat minigame launched");
        }

        private void SpawnEnemy(FighterData data)
        {
            if (data == null) return;

            var sprite = data.characterEmotion?.sprite;
            if (sprite == null && data.characterReference != null && !string.IsNullOrEmpty(data.characterEmotion?.spriteName))
                sprite = data.characterReference.GetEmotionSpriteByName(data.characterEmotion.spriteName);

            if (sprite != null && enemySpriteRoot != null)
            {
                var go = new GameObject("Enemy");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                go.transform.SetParent(enemySpriteRoot, false);
            }

            if (enemyHealthBar != null)
                enemyHealthBar.Initialize(data.baseHealthPoints, data.baseHealthPoints);
        }
    }
}
