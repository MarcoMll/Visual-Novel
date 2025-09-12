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

        [System.Serializable]
        private struct TimingSettings
        {
            public float showTime;       // duration to display action icons
            public float moveTime;       // duration for icons to travel to target
            public float hideTime;       // duration to hide action icons
            public float waitBeforeMove; // pause after showing before movement
            public float preImpact;      // lead-up time before impact occurs
        }

        [Header("Timing Settings")]
        [SerializeField] private TimingSettings timings = new TimingSettings
        {
            showTime = 0.25f,
            moveTime = 0.5f,
            hideTime = 0.2f,
            waitBeforeMove = 0.1f,
            preImpact = 0.05f
        };
        
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


            // player attacks first
            var enemyDefeated = false;
            yield return HandleAttackSequence(
                pAttackPoints,
                eDefencePoints,
                _player,
                _enemy,
                playerAction,
                enemyAction,
                attackSprite,
                defenceSprite,
                result => enemyDefeated = result);

            if (enemyDefeated)
            {
                _playerStats.ResetPoints();
                CheckActionPoints();
                playerAction.Reset();
                enemyAction.Reset();
                _roundInProgress = false;
                _minigame.HandleEnemyDefeat();
                yield break;
            }

            // enemy attacks
            var playerDefeated = false;
            yield return HandleAttackSequence(
                eAttackPoints,
                pDefencePoints,
                _enemy,
                _player,
                enemyAction,
                playerAction,
                attackSprite,
                defenceSprite,
                result => playerDefeated = result);

            if (playerDefeated)
            {
                _playerStats.ResetPoints();
                CheckActionPoints();
                playerAction.Reset();
                enemyAction.Reset();
                _roundInProgress = false;
                _minigame.HandlePlayerDefeat();
                yield break;
            }

            // rest animations
            playerAction.Setup(restSprite, pRestPoints);
            enemyAction.Setup(restSprite, eRestPoints);
            playerAction.Show(timings.showTime);
            enemyAction.Show(timings.showTime);
            yield return new WaitForSeconds(timings.showTime + timings.waitBeforeMove);
            _player.ApplyRest(pRestPoints);
            _enemy.ApplyRest(eRestPoints);
            playerAction.Hide(timings.hideTime);
            enemyAction.Hide(timings.hideTime);
            yield return new WaitForSeconds(timings.hideTime);

            _playerStats.ResetPoints();
            _playerStats.onActionPointAssigned?.Invoke();

            playerAction.Reset();
            enemyAction.Reset();

            _round++;
            UpdateRoundCountTextField();

            _roundInProgress = false;
        }

        private IEnumerator HandleAttackSequence(
            int attackPoints,
            int defencePoints,
            FighterRuntime attacker,
            FighterRuntime defender,
            ActionVisualizer attackerVisualizer,
            ActionVisualizer defenderVisualizer,
            Sprite attackSprite,
            Sprite defenceSprite,
            System.Action<bool> onComplete)
        {
            attackerVisualizer.Setup(attackSprite, attackPoints);
            defenderVisualizer.Setup(defenceSprite, defencePoints);
            attackerVisualizer.Show(timings.showTime);
            defenderVisualizer.Show(timings.showTime);
            yield return new WaitForSeconds(timings.showTime + timings.waitBeforeMove);
            attackerVisualizer.MoveToTarget(timings.moveTime);
            defenderVisualizer.MoveToTarget(timings.moveTime);
            yield return new WaitForSeconds(timings.moveTime - timings.preImpact);

            if (attackPoints > defencePoints)
            {
                defenderVisualizer.Hide(timings.hideTime);
                yield return new WaitForSeconds(timings.preImpact);
                attackerVisualizer.SetNumber(attackPoints - defencePoints);
                yield return new WaitForSeconds(timings.hideTime);
                var damage = (attackPoints - defencePoints) * attacker.BaseStats.baseDamage;
                defender.TakeDamage(damage);

                if (!defender.IsAlive)
                {
                    yield return new WaitForSeconds(timings.waitBeforeMove);
                    attackerVisualizer.Hide(timings.hideTime);
                    yield return new WaitForSeconds(timings.hideTime);
                    onComplete?.Invoke(true);
                    yield break;
                }

                yield return new WaitForSeconds(timings.waitBeforeMove);
                attackerVisualizer.Hide(timings.hideTime);
                yield return new WaitForSeconds(timings.hideTime);
            }
            else
            {
                attackerVisualizer.Hide(timings.hideTime);
                yield return new WaitForSeconds(timings.preImpact);
                defenderVisualizer.SetNumber(defencePoints - attackPoints);
                yield return new WaitForSeconds(timings.hideTime);
                yield return new WaitForSeconds(timings.waitBeforeMove);
                defenderVisualizer.Hide(timings.hideTime);
                yield return new WaitForSeconds(timings.hideTime);
            }

            onComplete?.Invoke(false);
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

