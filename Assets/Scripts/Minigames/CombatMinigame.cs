using UnityEngine;

namespace VisualNovel.Minigames
{
    /// <summary>
    /// Example implementation of a minigame. This is only a placeholder
    /// to demonstrate how to derive from <see cref="MinigameBase"/>.
    /// </summary>
    public class CombatMinigame : MinigameBase
    {
        protected override void OnStart()
        {
            // Setup combat-specific logic here. For now we immediately
            // finish with success to demonstrate the flow.
            Debug.Log("Combat minigame launched");
            Finish(true);
        }
    }
}
