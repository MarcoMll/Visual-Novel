using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualNovel.Audio
{
    [Serializable]
    public class AudioGroup
    {
        public string name;
        [SerializeField] private AudioGroupElement[] elements;

        public Dictionary<string, AudioGroupElement> groupElementsMap;

        public void InitializeGroup()
        {
            if (groupElementsMap == null)
                groupElementsMap = new Dictionary<string, AudioGroupElement>(StringComparer.OrdinalIgnoreCase);
            else
                groupElementsMap.Clear();

            if (elements == null || elements.Length == 0)
                return;

            foreach (var element in elements)
            {
                if (element == null)
                {
                    Debug.LogError($"[AudioGroup:{name}] Null element in array.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(element.name))
                {
                    Debug.LogError($"[AudioGroup:{name}] Element has empty name.");
                    continue;
                }

                if (groupElementsMap.ContainsKey(element.name))
                {
                    Debug.LogError($"[AudioGroup:{name}] Element already registered: {element.name}");
                    continue;
                }

                groupElementsMap.Add(element.name, element);
            }
        }
    }
}