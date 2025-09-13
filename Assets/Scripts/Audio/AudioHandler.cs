using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace VisualNovel.Audio
{
    using GameFlow;
    
    public class AudioHandler : MonoBehaviour, IInitializeOnAwake
    {
        [SerializeField] private AudioData[] audioDatas;
        public static AudioHandler Instance { get; private set; }

        private readonly Dictionary<AudioClip, AudioSource> _ambienceSources = new();
        private AudioMixerGroup _ambienceMixerGroup;

        [Serializable]
        public class AudioData
        {
            [HideInInspector] public string name;
            
            public AudioSource audioSource;
            public AudioMixerGroup audioMixerGroup;

            public void UpdateNameField()
            {
                if (audioSource == null) return;
                name = audioSource.name;
            }
            
            public void InitializeAudioSource()
            {
                if (audioSource == null || audioMixerGroup == null)
                {
                    Debug.LogError("AudioSource or AudioMixerGroup is not defined.");
                    return;
                }

                audioSource.outputAudioMixerGroup = audioMixerGroup;
            }

            public void PlayClip(AudioClip audioClip, bool loop = false, bool playOneShot = false)
            {
                audioSource.loop = loop;

                if (playOneShot == false)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
                else audioSource.PlayOneShot(audioClip);
            }
        }

        private void OnValidate()
        {
            if (audioDatas == null)
            {
                Debug.LogWarning("AudioHandler: audioDatas array is null in OnValidate");
                return;
            }

            foreach (var audioData in audioDatas)
            {
                if (audioData == null)
                {
                    Debug.LogWarning("AudioHandler: audioDatas contains null entry in OnValidate");
                    continue;
                }
                audioData.UpdateNameField();
            }
        }
        
        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);      // discard the new duplicate
                return;
            }
            Instance = this;

            if (audioDatas == null)
            {
                Debug.LogWarning("AudioHandler: audioDatas array is null during Initialize");
                return;
            }

            foreach (var audioData in audioDatas)
            {
                if (audioData == null)
                {
                    Debug.LogWarning("AudioHandler: audioDatas contains null entry during Initialize");
                    continue;
                }

                audioData.InitializeAudioSource();

                if (audioData.name == "Ambience Player")
                {
                    _ambienceMixerGroup = audioData.audioMixerGroup;
                }
            }
        }

        private void SetAudioClip(AudioClip audioClip, string audioDataName, bool loop = false, bool playOneShot = false)
        {
            if (audioClip == null)
            {
                Debug.LogWarning($"AudioHandler: Provided audio clip for '{audioDataName}' is null");
                return;
            }

            if (audioDatas == null)
            {
                Debug.LogWarning("AudioHandler: audioDatas array is null in SetAudioClip");
                return;
            }

            foreach (var audioData in audioDatas)
            {
                if (audioData == null)
                {
                    Debug.LogWarning("AudioHandler: audioDatas contains null entry in SetAudioClip");
                    continue;
                }

                if (audioDataName == audioData.name)
                {
                    audioData.PlayClip(audioClip, loop, playOneShot);
                    return;
                }
            }

            Debug.LogWarning($"AudioHandler: AudioData with name '{audioDataName}' was not found");
        }
        
        public void SetMusicClip(AudioClip musicClip, bool loop = true)
        {
            SetAudioClip(musicClip, "Music Player", loop);
        }

        public void SetAmbienceClip(AudioClip ambienceClip, bool loop = true)
        {
            SetAmbienceClips(loop, ambienceClip);
        }

        public void SetAmbienceClips(bool loop = true, params AudioClip[] ambienceClips)
        {
            foreach (var clip in ambienceClips)
            {
                PlayAmbienceClip(clip, loop);
            }
        }

        private void PlayAmbienceClip(AudioClip ambienceClip, bool loop)
        {
            if (ambienceClip == null)
            {
                Debug.LogWarning("AudioHandler: ambience clip is null");
                return;
            }

            if (_ambienceMixerGroup == null)
            {
                Debug.LogWarning("Ambience mixer group is not defined!");
                return;
            }

            if (_ambienceSources.TryGetValue(ambienceClip, out var existingSource))
            {
                StopAmbience(ambienceClip);
            }

            var go = new GameObject($"Ambience: {ambienceClip.name}");
            go.transform.SetParent(transform);

            var source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = _ambienceMixerGroup;
            source.loop = loop;
            source.clip = ambienceClip;
            source.Play();

            _ambienceSources[ambienceClip] = source;

            if (!loop)
            {
                StartCoroutine(RemoveWhenFinished(ambienceClip, source));
            }
        }

        private IEnumerator RemoveWhenFinished(AudioClip clip, AudioSource source)
        {
            yield return new WaitWhile(() => source != null && source.isPlaying);
            StopAmbience(clip);
        }

        public void StopAmbience(AudioClip ambienceClip)
        {
            if (_ambienceSources.TryGetValue(ambienceClip, out var source))
            {
                source.Stop();
                Destroy(source.gameObject);
                _ambienceSources.Remove(ambienceClip);
            }
        }

        public void StopAllAmbience()
        {
            foreach (var kvp in new Dictionary<AudioClip, AudioSource>(_ambienceSources))
            {
                StopAmbience(kvp.Key);
            }
        }

        public void PlaySfx(AudioClip soundEffectClip)
        {
            SetAudioClip(soundEffectClip, "SFX Player", playOneShot: true);
        }

        public AudioClip PickRandom(AudioClip[] sounds)
        {
            if (sounds == null)
            {
                Debug.LogWarning("AudioHandler: PickRandom received null array");
                return null;
            }

            switch (sounds.Length)
            {
                case 0:
                    return null;
                case 1:
                    return sounds[0];
                default:
                    var randomIndex = Random.Range(0, sounds.Length);
                    return sounds[randomIndex];
            }
        }
    }
}