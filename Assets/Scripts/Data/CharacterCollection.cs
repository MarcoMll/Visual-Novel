using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Data
{
    /// <summary>
    /// Tracks known characters and the player's relationship values with them.
    /// </summary>
    [Serializable]
    public class CharacterCollection : BaseGameData
    {
        [SerializeField] private List<string> characterGuids = new();
        [SerializeField] private List<int> relationshipValues = new();

        private readonly Dictionary<CharacterSO, int> relationships = new();

        protected override string SaveKey => "CHARACTER_DATA";

        public IReadOnlyDictionary<CharacterSO, int> Relationships => relationships;

        public void RegisterCharacter(CharacterSO character)
        {
            if (character == null || relationships.ContainsKey(character)) return;
            relationships[character] = 0;
        }

        public void RemoveCharacter(CharacterSO character)
        {
            if (character == null) return;
            relationships.Remove(character);
        }

        public void SetRelationship(CharacterSO character, int value)
        {
            if (character == null) return;
            relationships[character] = value;
        }

        public int GetRelationship(CharacterSO character)
        {
            return character != null && relationships.TryGetValue(character, out var value) ? value : 0;
        }

        public void ModifyRelationship(CharacterSO character, int delta)
        {
            if (character == null) return;

            if (relationships.ContainsKey(character) == false)
            {
                RegisterCharacter(character);
            }
            
            var current = GetRelationship(character);
            relationships[character] = current + delta;
        }

        public override void Save()
        {
            characterGuids.Clear();
            relationshipValues.Clear();
            foreach (var pair in relationships)
            {
                characterGuids.Add(pair.Key.Guid);
                relationshipValues.Add(pair.Value);
            }
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            relationships.Clear();
            for (int i = 0; i < Math.Min(characterGuids.Count, relationshipValues.Count); i++)
            {
                var character = BaseSO.GetByGuid<CharacterSO>(characterGuids[i]);
                if (character != null)
                {
                    relationships[character] = relationshipValues[i];
                }
            }
        }
    }
}

