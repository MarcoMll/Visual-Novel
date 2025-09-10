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

        public List<FighterBaseStats> Fighters { get; private set; } = new();

        private FighterRuntime _player;
        private FighterRuntime _enemy;
        private string _parallaxLayer;
        private Vector2 _characterOffset;

        public void Initialize(List<FighterBaseStats> fighters, string parallaxLayer, Vector2 characterOffset)
        {
            Fighters = fighters ?? new List<FighterBaseStats>();
            _parallaxLayer = parallaxLayer;
            _characterOffset = characterOffset;
        }

        protected override void OnStart()
        {
            var playerBase = new FighterBaseStats();
            _player = new FighterRuntime(playerBase);

            if (Fighters != null && Fighters.Count > 0)
            {
                var enemyBase = Fighters[0];
                _enemy = new FighterRuntime(enemyBase);
                SpawnEnemy(enemyBase);
            }

            if (playerHealthBar != null)
                playerHealthBar.Initialize(_player.BaseStats.baseHealthPoints);

            playerStatsController.Initialize(_player);

            if (startRoundButton != null)
                startRoundButton.onClick.AddListener(PlayRound);

            Debug.Log("Combat minigame launched");
        }

        private void SpawnEnemy(FighterBaseStats data)
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

            MathUtility.SplitIntoThree(_enemy.ActionPointsPerRound, out var e_attackPoints, out var e_defencePoints, out var e_restPoints);

            if (p_attackPoints > e_defencePoints)
            {
                var damage = (p_attackPoints - e_defencePoints) * _player.BaseStats.baseDamage;
                _enemy.TakeDamage(damage);
                enemyHealthBar.ModifyCurrentHealthValue(-damage);
            }

            if (_enemy.IsAlive && e_attackPoints > p_defencePoints)
            {
                var damage = (e_attackPoints - p_defencePoints) * _enemy.BaseStats.baseDamage;
                _player.TakeDamage(damage);
                playerHealthBar.ModifyCurrentHealthValue(-damage);
            }

            _enemy.ApplyRest(e_restPoints);

            playerStatsController.ResetPoints();
        }
    }
}