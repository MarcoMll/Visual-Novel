using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Data
{
    /// <summary>
    /// Manages saving and loading of the different game data sections.
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        [SerializeReference, HideInInspector] private List<BaseGameData> dataSections = new();
        public static GameDataManager Instance { get; private set; }
        
        // ----- Player Stats -----
        public Inventory playerInventory = new Inventory();
        public CharacterCollection characterCollection = new CharacterCollection();
        public TraitCollection playerTraitCollection = new TraitCollection();
        
        // ----- Game flow Modifiers -----
        public FlagCollection flagCollection = new FlagCollection();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);      // discard the new duplicate
                return;
            }
            Instance = this;
            
            dataSections.Add(playerInventory);
            dataSections.Add(characterCollection);
            dataSections.Add(playerTraitCollection);
            dataSections.Add(flagCollection);
        }

        private void Update()
        {
            // temporary made for testing
            if (Input.GetKeyDown(KeyCode.S))
                SaveGame();
            else if (Input.GetKeyDown(KeyCode.L))
                LoadGame();
        }

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
