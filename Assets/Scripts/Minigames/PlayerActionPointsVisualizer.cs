using TMPro;
using UnityEngine;

namespace VisualNovel.Minigames.Combat
{
    public class PlayerActionPointsVisualizer : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private TMP_Text attackPointsTextField;
        [SerializeField] private TMP_Text currentDamageTextField;
        [SerializeField] private TMP_Text nextDamageTextField;
        [Header("Defence")]
        [SerializeField] private TMP_Text defencePointsTextField;
        [Header("Rest")]
        [SerializeField] private TMP_Text restPointsTextField;
        [Header("Stats Controller")]
        [SerializeField] private PlayerStatsController playerStatsController;

        private void OnEnable()
        {
            if (playerStatsController != null)
            {
                playerStatsController.onActionPointAssigned += UpdateUi;
                UpdateUi();
            }
        }

        private void OnDisable()
        {
            if (playerStatsController != null)
                playerStatsController.onActionPointAssigned -= UpdateUi;
        }

        private void UpdateUi()
        {
            if (playerStatsController == null) return;

            playerStatsController.GetDistributedActionPoints(out var attack, out var defence, out var rest);
            UpdateAttack(attack);
            UpdateDefence(defence);
            UpdateRest(rest);
        }

        private void UpdateAttack(int attackPoints)
        {
            attackPointsTextField.text = attackPoints.ToString();
            var baseDamage = playerStatsController.BaseDamage;
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

