using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VisualNovel.Environment;
using VisualNovel.UI.Dynamic;

namespace VisualNovel.Minigames.Combat
{
    using Utilities;
    
    /// <summary>
    /// Example implementation of a minigame. This is only a placeholder
    /// to demonstrate how to derive from <see cref="MinigameBase"/>.
    /// </summary>
    public class CombatMinigame : MinigameBase
    {
        [SerializeField] private PlayerStatsController playerStatsController;
        [Header("UI")]
        [SerializeField] private UIHealthBar playerHealthBar;
        [SerializeField] private UIHealthBar enemyHealthBar;
        [SerializeField] private Button startRoundButton;

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

            playerHealthBar.Initialize(_player.baseHealthPoints);
            
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
                _characterOffset, data.layer, data.characterScale, _parallaxLayer);

            if (enemyHealthBar != null)
                enemyHealthBar.Initialize(data.baseHealthPoints, data.baseHealthPoints);
        }
        
        private void PlayRound()
        {
            // ----- player have already distributed their points -----
            playerStatsController.GetDistributedActionPoints(out var p_attackPoints, out var p_defencePoints, out var p_restPoints);
            // ----- randomly distribute AI's points -----
            // ----- compare -----
        }
    }
}