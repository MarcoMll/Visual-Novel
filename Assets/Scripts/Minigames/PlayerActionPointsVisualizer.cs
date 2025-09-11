using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.Minigames.Combat.UI
{
    public class PlayerActionPointsVisualizer : MonoBehaviour
    {
        [Header("Attack")] 
        [SerializeField] private Button attackButton;
        [SerializeField] private TMP_Text attackPointsTextField;
        [SerializeField] private TMP_Text currentDamageTextField;
        [SerializeField] private TMP_Text nextDamageTextField;
        [Header("Defence")]
        [SerializeField] private Button defenceButton;
        [SerializeField] private TMP_Text defencePointsTextField;
        [Header("Rest")]
        [SerializeField] private Button restButton;
        [SerializeField] private TMP_Text restPointsTextField;
        
        private PlayerStatsController _playerStatsController;

        public void Initialize(PlayerStatsController playerStatsController)
        {
            _playerStatsController = playerStatsController;
            
            if (_playerStatsController == null) // moved from OnEnable here to avoid issues
            {
                Debug.LogError("PlayerStatsController is absent!");
                return;
            }
            
            _playerStatsController.onActionPointAssigned += UpdateUi;
            UpdateUi();
            
            attackButton.onClick.AddListener(_playerStatsController.AddAttackActionPoint);
            defenceButton.onClick.AddListener(_playerStatsController.AddDefenceActionPoint);
            restButton.onClick.AddListener(_playerStatsController.AddRestActionPoints);
        }

        private void OnDisable()
        {
            if (_playerStatsController != null)
                _playerStatsController.onActionPointAssigned -= UpdateUi;
        }

        private void UpdateUi()
        {
            if (_playerStatsController == null) return;

            _playerStatsController.GetDistributedActionPoints(out var attack, out var defence, out var rest);
            UpdateAttack(attack);
            UpdateDefence(defence);
            UpdateRest(rest);
        }

        private void UpdateAttack(int attackPoints)
        {
            attackPointsTextField.text = attackPoints.ToString();
            var baseDamage = _playerStatsController.BaseDamage;
            currentDamageTextField.text = (attackPoints * baseDamage).ToString();
            nextDamageTextField.text = ((attackPoints + 1) * baseDamage).ToString();
        }

        private void UpdateDefence(int defencePoints)
        {
            defencePointsTextField.text = defencePoints.ToString();
        }

        private void UpdateRest(int restPoints)
        {
            restPointsTextField.text = restPoints.ToString();
        }
    }
}

