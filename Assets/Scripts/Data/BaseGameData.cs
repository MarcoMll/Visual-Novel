using System;
using UnityEngine;

namespace VisualNovel.Data
{
    /// <summary>
    /// Base class for any game data section that can be saved or loaded.
    /// Extend this class to include additional data containers.
    /// </summary>
    [Serializable]
    public abstract class BaseGameData
    {
        /// <summary>
        /// Unique key used to store this data section.
        /// </summary>
        protected abstract string SaveKey { get; }

        /// <summary>
        /// Saves the current state of the data section.
        /// </summary>
        public virtual void Save()
        {
            var json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(SaveKey, json);
        }

        /// <summary>
        /// Loads the saved state of the data section.
        /// </summary>
        public virtual void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;
            var json = PlayerPrefs.GetString(SaveKey);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}
