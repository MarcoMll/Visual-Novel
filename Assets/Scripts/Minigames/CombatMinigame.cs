using System.Collections.Generic;
using UnityEngine;
using VisualNovel.Environment;
using VisualNovel.Minigames.Combat.UI;
using VisualNovel.UI;

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
        
        public List<FighterBaseStats> Fighters { get; private set; } = new();

        private FighterRuntime _player;
        private FighterRuntime _enemy;
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

            if (Fighters != null && Fighters.Count > 0)
            {
                var enemyBase = Fighters[0];
                _enemy = new FighterRuntime(enemyBase);
                SpawnEnemy(enemyBase);
            }

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
    }
}