using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VisualNovel.Environment;
using VisualNovel.Minigames.Combat.UI;
using VisualNovel.UI.Dynamic;
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
        
        [Header("UI")]
        [SerializeField] private UIHealthBar playerHealthBar;
        [SerializeField] private UIHealthBar enemyHealthBar;
        [SerializeField] private Button startRoundButton;

        [Header("Action Visualizers")]
        [SerializeField] private ActionVisualizer playerAction;
        [SerializeField] private ActionVisualizer enemyAction;

        [Header("Action Sprites")]
        [SerializeField] private Sprite attackSprite;
        [SerializeField] private Sprite defenceSprite;
        [SerializeField] private Sprite restSprite;
        
        public List<FighterBaseStats> Fighters { get; private set; } = new();

        private FighterRuntime _player;
        private FighterRuntime _enemy;
        private string _parallaxLayer;
        private Vector2 _characterOffset;
        private bool _roundInProgress;
        private GameObject _additionalUiInstance;

        
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
                _additionalUiInstance.GetComponent<PlayerActionPointsVisualizer>().Initialize(playerStatsController); // assuming the game's UI root has PlayerActionPointsVisualizer script attached
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
                if (enemyHealthBar != null)
                    _enemy.OnHealthChanged += enemyHealthBar.ModifyCurrentHealthValue;
            }

            if (playerHealthBar != null)
            {
                playerHealthBar.Initialize(_player.BaseStats.baseHealthPoints);
                _player.OnHealthChanged += playerHealthBar.ModifyCurrentHealthValue;
            }

            if (playerStatsController != null)
            {
                playerStatsController.Initialize(_player);
                playerStatsController.onActionPointAssigned += CheckActionPoints;
            }

            if (startRoundButton != null)
            {
                startRoundButton.onClick.AddListener(PlayRound);
                CheckActionPoints();
            }

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

            if (enemyHealthBar != null)
                enemyHealthBar.Initialize(data.baseHealthPoints, data.baseHealthPoints);
        }
        
        private void PlayRound()
        {
            if (_roundInProgress)
                return;

            StartCoroutine(PlayRoundRoutine());
        }

        private IEnumerator PlayRoundRoutine()
        {
            _roundInProgress = true;

            if (startRoundButton != null)
                startRoundButton.interactable = false;

            // get points
            playerStatsController.GetDistributedActionPoints(out var pAttackPoints, out var pDefencePoints, out var pRestPoints);
            MathUtility.SplitIntoThree(_enemy.ActionPointsPerRound, out var eAttackPoints, out var eDefencePoints, out var eRestPoints);

            const float showTime = 0.25f;
            const float moveTime = 0.5f;
            const float hideTime = 0.2f;
            const float waitBeforeMove = 0.1f;
            const float preImpact = 0.05f;

            // player attacks first
            playerAction.Setup(attackSprite, pAttackPoints);
            enemyAction.Setup(defenceSprite, eDefencePoints);
            playerAction.Show(showTime);
            enemyAction.Show(showTime);
            yield return new WaitForSeconds(showTime + waitBeforeMove);
            playerAction.MoveToTarget(moveTime);
            enemyAction.MoveToTarget(moveTime);
            yield return new WaitForSeconds(moveTime - preImpact);

            if (pAttackPoints > eDefencePoints)
            {
                enemyAction.Hide(hideTime);
                yield return new WaitForSeconds(preImpact);
                playerAction.SetNumber(pAttackPoints - eDefencePoints);
                yield return new WaitForSeconds(hideTime);
                var damage = (pAttackPoints - eDefencePoints) * _player.BaseStats.baseDamage;
                _enemy.TakeDamage(damage);
                yield return new WaitForSeconds(waitBeforeMove);
                playerAction.Hide(hideTime);
                yield return new WaitForSeconds(hideTime);
            }
            else
            {
                playerAction.Hide(hideTime);
                yield return new WaitForSeconds(preImpact);
                enemyAction.SetNumber(eDefencePoints - pAttackPoints);
                yield return new WaitForSeconds(hideTime);
                yield return new WaitForSeconds(waitBeforeMove);
                enemyAction.Hide(hideTime);
                yield return new WaitForSeconds(hideTime);
            }

            // enemy attacks
            if (_enemy.IsAlive)
            {
                enemyAction.Setup(attackSprite, eAttackPoints);
                playerAction.Setup(defenceSprite, pDefencePoints);
                enemyAction.Show(showTime);
                playerAction.Show(showTime);
                yield return new WaitForSeconds(showTime + waitBeforeMove);
                enemyAction.MoveToTarget(moveTime);
                playerAction.MoveToTarget(moveTime);
                yield return new WaitForSeconds(moveTime - preImpact);

                if (eAttackPoints > pDefencePoints)
                {
                    playerAction.Hide(hideTime);
                    yield return new WaitForSeconds(preImpact);
                    enemyAction.SetNumber(eAttackPoints - pDefencePoints);
                    yield return new WaitForSeconds(hideTime);
                    var damage = (eAttackPoints - pDefencePoints) * _enemy.BaseStats.baseDamage;
                    _player.TakeDamage(damage);
                    yield return new WaitForSeconds(waitBeforeMove);
                    enemyAction.Hide(hideTime);
                    yield return new WaitForSeconds(hideTime);
                }
                else
                {
                    enemyAction.Hide(hideTime);
                    yield return new WaitForSeconds(preImpact);
                    playerAction.SetNumber(pDefencePoints - eAttackPoints);
                    yield return new WaitForSeconds(hideTime);
                    yield return new WaitForSeconds(waitBeforeMove);
                    playerAction.Hide(hideTime);
                    yield return new WaitForSeconds(hideTime);
                }
            }

            // rest animations
            playerAction.Setup(restSprite, pRestPoints);
            enemyAction.Setup(restSprite, eRestPoints);
            playerAction.Show(showTime);
            enemyAction.Show(showTime);
            yield return new WaitForSeconds(showTime + waitBeforeMove);
            _player.ApplyRest(pRestPoints);
            _enemy.ApplyRest(eRestPoints);
            playerAction.Hide(hideTime);
            enemyAction.Hide(hideTime);
            yield return new WaitForSeconds(hideTime);

            playerStatsController.ResetPoints();
            CheckActionPoints();

            playerAction.Reset();
            enemyAction.Reset();

            _roundInProgress = false;
        }

        private void CheckActionPoints()
        {
            if (startRoundButton == null || playerStatsController == null) return;

            startRoundButton.interactable = playerStatsController.AllActionPointsDistributed();
        }

        private void OnDestroy()
        {
            if (playerStatsController != null)
                playerStatsController.onActionPointAssigned -= CheckActionPoints;
        }
    }
}