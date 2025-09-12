using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualNovel.Audio
{
    public class AudioContainer : MonoBehaviour
    {
        [SerializeField] private AudioGroup[] audioGroups;

        // Case-insensitive keys so "Theme" and "theme" don't collide at runtime.
        private Dictionary<string, AudioGroup> audioGroupsMap { get; set; }
            = new Dictionary<string, AudioGroup>(StringComparer.OrdinalIgnoreCase);

        public void Initialize()
        {
            if (audioGroupsMap == null)
                audioGroupsMap = new Dictionary<string, AudioGroup>(StringComparer.OrdinalIgnoreCase);
            else
                audioGroupsMap.Clear();

            if (audioGroups == null || audioGroups.Length == 0)
                return;

            foreach (var audioGroup in audioGroups)
            {
                if (audioGroup == null)
                {
                    Debug.LogError("[AudioContainer] Null AudioGroup in array.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(audioGroup.name))
                {
                    Debug.LogError("[AudioContainer] AudioGroup has empty name.");
                    continue;
                }

                if (audioGroupsMap.ContainsKey(audioGroup.name))
                {
                    Debug.LogError($"[AudioContainer] Group already registered: {audioGroup.name}");
                    continue;
                }

                audioGroup.InitializeGroup();
                audioGroupsMap.Add(audioGroup.name, audioGroup);
            }
        }

        /// <summary>
        /// Returns the first clip matching <paramref name="clipName"/>.
        /// Checks element.name first, then falls back to AudioClip.name.
        /// </summary>
        public AudioClip FindClipByName(string clipName)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                Debug.LogError("[AudioContainer] FindClipByName: clipName is null/empty.");
                return null;
            }

            // Fast path: match by element key (element.name).
            foreach (var group in audioGroupsMap.Values)
            {
                if (group.groupElementsMap == null) continue;

                if (group.groupElementsMap.TryGetValue(clipName, out var element) &&
                    element != null && element.audioClip != null)
                {
                    return element.audioClip;
                }
            }

            // Fallback: match by actual AudioClip.name.
            foreach (var group in audioGroupsMap.Values)
            {
                if (group.groupElementsMap == null) continue;

                foreach (var element in group.groupElementsMap.Values)
                {
                    var clip = element?.audioClip;
                    if (clip != null &&
                        string.Equals(clip.name, clipName, StringComparison.OrdinalIgnoreCase))
                    {
                        return clip;
                    }
                }
            }

            Debug.LogWarning($"[AudioContainer] No AudioClip named '{clipName}' was found in any group.");
            return null;
        }

        public AudioClip[] GetAllAudioClipsFromGroup(string groupName)
        {
            if (!audioGroupsMap.TryGetValue(groupName, out var audioGroup) ||
                audioGroup.groupElementsMap == null)
            {
                Debug.LogError($"[AudioContainer] No group with the following name was found: {groupName}");
                return Array.Empty<AudioClip>();
            }

            var values = audioGroup.groupElementsMap.Values;
            var list = new List<AudioClip>(values.Count);

            foreach (var element in values)
            {
                if (element?.audioClip != null)
                    list.Add(element.audioClip);
            }

            return list.ToArray();
        }
    }
}
