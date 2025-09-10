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

        private void UpdateAttack()
        {
            // attackPointsTextField -> type the amount of action points currently dedicated to attack
            // currentDamageTextField -> the amount of attack points * base damage
            // nextDamageTextField -> currentDamageTextField + base damage, this field shows what the damage will become on the next "add"
        }

        private void UpdateDefence()
        {
            // defencePointsTextField -> type the amount of action points currently dedicated to defence
        }

        private void UpdateRest()
        {
            // restPointsTextField -> type the amount of action points currently dedicated to resting
        }
    }
}