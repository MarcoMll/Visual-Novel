using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VisualNovel.Minigames.Combat;
using VisualNovel.UI.Dynamic;
using VisualNovel.Utilities;

namespace VisualNovel.Minigames.Combat.UI
{
    /// <summary>
    /// Handles all UI interactions for the combat minigame.
    /// This script is intended to be attached to the minigame UI prefab.
    /// </summary>
    public class CombatMinigameUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roundCountTextField;
        [SerializeField] private TMP_Text playerActionPointsTextField;
        [SerializeField] private TMP_Text enemyNameTextField;
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

        private CombatMinigame _minigame;
        private PlayerStatsController _playerStats;
        private FighterRuntime _player;
        private FighterRuntime _enemy;
        private bool _roundInProgress;
        private int _round = 1;
        
        /// <summary>
        /// Initialize the UI and hook up required events.
        /// </summary>
        public void Initialize(CombatMinigame minigame, PlayerStatsController statsController)
        {
            _minigame = minigame;
            _playerStats = statsController;

            var pointsVisualizer = GetComponent<PlayerActionPointsVisualizer>();
            if (pointsVisualizer != null && _playerStats != null)
            {
                pointsVisualizer.Initialize(_playerStats);
            }

            if (_playerStats != null)
            {
                _playerStats.onActionPointAssigned += CheckActionPoints;
                _playerStats.onActionPointAssigned += UpdatePlayerActionPointsTextField;
            }

            if (startRoundButton != null)
            {
                startRoundButton.onClick.AddListener(PlayRound);
            }
        }

        /// <summary>
        /// Provide runtime fighter instances so UI can react to their state.
        /// </summary>
        public void SetupCombatants(FighterRuntime player, FighterRuntime enemy)
        {
            if (_player != null && playerHealthBar != null)
            {
                _player.OnHealthChanged -= playerHealthBar.ModifyCurrentHealthValue;
            }

            _player = player;

            if (playerHealthBar != null)
            {
                playerHealthBar.Initialize(_player.BaseStats.baseHealthPoints);
                _player.OnHealthChanged += playerHealthBar.ModifyCurrentHealthValue;
            }

            if (enemyHealthBar != null)
            {
                if (_enemy != null)
                {
                    _enemy.OnHealthChanged -= enemyHealthBar.ModifyCurrentHealthValue;
                }

                _enemy = enemy;

                if (_enemy != null)
                {
                    enemyHealthBar.Initialize(_enemy.BaseStats.baseHealthPoints, _enemy.BaseStats.baseHealthPoints);
                    _enemy.OnHealthChanged += enemyHealthBar.ModifyCurrentHealthValue;

                    enemyNameTextField.text = _enemy.BaseStats.characterReference.characterName;
                }
            }

            CheckActionPoints();
            UpdatePlayerActionPointsTextField();
        }

        private void UpdatePlayerActionPointsTextField()
        {
            playerActionPointsTextField.text = $"Очков осталось: {_playerStats.totalLeftActionPoints}/{_player.ActionPointsPerRound}";
        }

        private void UpdateRoundCountTextField()
        {
            roundCountTextField.text = $"РАУНД {_round}";
        }
        
        private void PlayRound()
        {
            if (_roundInProgress || _playerStats == null || _enemy == null)
                return;

            StartCoroutine(PlayRoundRoutine());
        }

        private IEnumerator PlayRoundRoutine()
        {
            _roundInProgress = true;

            if (startRoundButton != null)
                startRoundButton.interactable = false;

            // get points
            _playerStats.GetDistributedActionPoints(out var pAttackPoints, out var pDefencePoints, out var pRestPoints);
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

                if (!_enemy.IsAlive)
                {
                    _playerStats.ResetPoints();
                    CheckActionPoints();
                    playerAction.Reset();
                    enemyAction.Reset();
                    _roundInProgress = false;
                    _minigame.HandleEnemyDefeat();
                    yield break;
                }
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
                    if (!_player.IsAlive)
                    {
                        yield return new WaitForSeconds(waitBeforeMove);
                        enemyAction.Hide(hideTime);
                        yield return new WaitForSeconds(hideTime);
                        _playerStats.ResetPoints();
                        CheckActionPoints();
                        playerAction.Reset();
                        enemyAction.Reset();
                        _roundInProgress = false;
                        _minigame.HandlePlayerDefeat();
                        yield break;
                    }

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

            _playerStats.ResetPoints();
            _playerStats.onActionPointAssigned?.Invoke();

            playerAction.Reset();
            enemyAction.Reset();

            _round++;
            UpdateRoundCountTextField();
            
            _roundInProgress = false;
        }

        private void CheckActionPoints()
        {
            if (startRoundButton == null || _playerStats == null) return;

            startRoundButton.interactable = _playerStats.AllActionPointsDistributed();
        }

        private void OnDestroy()
        {
            if (_playerStats != null)
                _playerStats.onActionPointAssigned -= CheckActionPoints;
        }
    }
}

