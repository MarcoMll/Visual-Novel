using System.Collections.Generic;
using UnityEngine;

namespace VisualNovel.Minigames.Combat
{
    using Environment;
    using UI;
    using VisualNovel.UI;
    
    public class CombatMinigame : MinigameBase
    {
        [SerializeField] private PlayerStatsController playerStatsController;
        
        public List<FighterBaseStats> Fighters { get; private set; } = new();

        private FighterRuntime _player;
        private FighterRuntime _enemy;
        private int _currentEnemyIndex;
        private string _parallaxLayer;
        private Vector2 _characterOffset;
        private GameObject _additionalUiInstance;
        private CombatMinigameUI _uiController;

        
        public void Initialize(List<FighterBaseStats> fighters, string parallaxLayer, Vector2 characterOffset)
        {
            Fighters = fighters ?? new List<FighterBaseStats>();
            _parallaxLayer = parallaxLayer;
            _characterOffset = characterOffset;
        }

        public override void Launch()
        {
            // ----- Instantiating game's UI -----
            if (UserInterfaceManager.Instance != null && uiPrefab != null)
            {
                _additionalUiInstance = UserInterfaceManager.Instance.SpawnAdditionalUI(uiPrefab);
                _uiController = _additionalUiInstance.GetComponent<CombatMinigameUI>();
                if (_uiController != null)
                {
                    _uiController.Initialize(this, playerStatsController);
                }
            }

            OnStart();
        }

        protected override void OnStart()
        {
            var playerBase = new FighterBaseStats();
            _player = new FighterRuntime(playerBase);

            _currentEnemyIndex = -1;
            SetNextEnemy();

            if (playerStatsController != null)
            {
                playerStatsController.Initialize(_player);
            }

            _uiController?.SetupCombatants(_player, _enemy);

            Debug.Log("Combat minigame launched");
        }

        protected override void OnFinish(bool isSuccess)
        {
            if (UserInterfaceManager.Instance != null && uiPrefab != null)
            {
                UserInterfaceManager.Instance.DeleteAdditionalUI(uiPrefab);
            }

            base.OnFinish(isSuccess);
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

        }

        private void SetNextEnemy()
        {
            _currentEnemyIndex++;
            if (Fighters == null)
            {
                Finish(true);
                return;
            }

            while (_currentEnemyIndex < Fighters.Count)
            {
                var enemyBase = Fighters[_currentEnemyIndex];
                var runtime = new FighterRuntime(enemyBase);
                if (runtime.IsAlive)
                {
                    _enemy = runtime;
                    SpawnEnemy(enemyBase);
                    _uiController?.SetupCombatants(_player, _enemy);
                    return;
                }

                _currentEnemyIndex++;
            }

            Finish(true);
        }

        public void HandleEnemyDefeat()
        {
            if (_enemy == null || _enemy.IsAlive)
                return;

            SetNextEnemy();
        }

        public void HandlePlayerDefeat()
        {
            if (_player == null || _player.IsAlive)
                return;

            Finish(false);
        }
    }
}