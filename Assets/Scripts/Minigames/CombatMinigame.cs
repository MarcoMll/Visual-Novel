using System.Collections.Generic;
using UnityEngine;
using VisualNovel.Environment;
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

        public List<FighterData> Fighters { get; private set; } = new();

        private FighterData _player;
        private FighterData _enemy;
        private string _parallaxLayer;
        private Vector2 _characterOffset;

        public void Initialize(List<FighterData> fighters, string parallaxLayer, Vector2 characterOffset)
        {
            Fighters = fighters ?? new List<FighterData>();
            _parallaxLayer = parallaxLayer;
            _characterOffset = characterOffset;
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
            if (data?.characterReference == null || data.characterEmotion == null)
                return;

            var sceneManager = SceneEnvironmentManager.Instance;
            if (sceneManager == null)
            {
                Debug.LogError("SceneEnvironmentManager is absent on the scene or was not initialized before usage!");
                return;
            }

            sceneManager.ShowCharacter(data.characterReference, data.characterEmotion, Color.white,
                _characterOffset, 0, Vector2.one, _parallaxLayer);

            if (enemyHealthBar != null)
                enemyHealthBar.Initialize(data.baseHealthPoints, data.baseHealthPoints);
        }
    }
}
