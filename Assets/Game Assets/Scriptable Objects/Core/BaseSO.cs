using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameAssets.ScriptableObjects.Core
{
    /// <summary>
    /// Base class for ScriptableObjects that require a persistent GUID.
    /// Provides runtime lookup by GUID for loading references from saved data.
    /// </summary>
    public abstract class BaseSO : ScriptableObject
    {
        [SerializeField, HideInInspector] private string guid;

        private static readonly Dictionary<string, BaseSO> Registry = new();

        /// <summary>
        /// Globally unique identifier for this asset.
        /// </summary>
        public string Guid => guid;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
            Register();
        }
#endif

        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = System.Guid.NewGuid().ToString();
            }
            Register();
        }

        private void Register()
        {
            Registry[guid] = this;
        }

        /// <summary>
        /// Retrieves an asset instance by its GUID.
        /// </summary>
        public static T GetByGuid<T>(string id) where T : BaseSO
        {
            return Registry.TryGetValue(id, out var so) ? so as T : null;
        }
    }
}

