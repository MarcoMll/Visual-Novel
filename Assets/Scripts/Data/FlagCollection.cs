using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualNovel.Data
{
    /// <summary>
    /// Stores boolean flags used throughout the game.
    /// </summary>
    [Serializable]
    public class FlagCollection : BaseGameData
    {
        [SerializeField] private List<string> keys = new();
        [SerializeField] private List<bool> values = new();

        private readonly Dictionary<string, bool> flags = new();

        protected override string SaveKey => "FLAG_DATA";

        public bool GetFlag(string key) => flags.TryGetValue(key, out var value) && value;

        public void SetFlag(string key, bool value)
        {
            flags[key] = value;
        }

        public void RemoveFlag(string key)
        {
            flags.Remove(key);
        }

        public override void Save()
        {
            keys.Clear();
            values.Clear();
            foreach (var pair in flags)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            flags.Clear();
            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
            {
                flags[keys[i]] = values[i];
            }
        }
    }
}

