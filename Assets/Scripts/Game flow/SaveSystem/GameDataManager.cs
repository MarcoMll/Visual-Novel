using System.Collections.Generic;
using UnityEngine;

namespace VisualNovel.Data
{
    /// <summary>
    /// Manages saving and loading of the different game data sections.
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        [SerializeReference] private List<BaseGameData> dataSections = new();

        /// <summary>
        /// Saves all registered data sections.
        /// </summary>
        public void SaveGame()
        {
            foreach (var section in dataSections)
            {
                section.Save();
            }
        }

        /// <summary>
        /// Loads all registered data sections.
        /// </summary>
        public void LoadGame()
        {
            foreach (var section in dataSections)
            {
                section.Load();
            }
        }
    }
}
