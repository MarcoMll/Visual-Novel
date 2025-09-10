using UnityEngine;

namespace VisualNovel.Minigames.Combat
{
    /// <summary>
    /// Represents the real-time state of a fighter during the combat minigame.
    /// Keeps track of current health and action points per round
    /// independent from the initialization data.
    /// </summary>
    public class FighterRuntime
    {
        public FighterBaseStats BaseStats { get; }
        public int CurrentHealth { get; private set; }
        public int ActionPointsPerRound { get; private set; }

        public FighterRuntime(FighterBaseStats baseStats)
        {
            BaseStats = baseStats ?? new FighterBaseStats();
            CurrentHealth = BaseStats.baseHealthPoints;
            ActionPointsPerRound = BaseStats.baseActionPoints;
        }

        public bool IsAlive => CurrentHealth > 0;

        public void TakeDamage(int amount)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        }

        public void ApplyRest(int restPoints)
        {
            ActionPointsPerRound += restPoints;
        }
    }
}
