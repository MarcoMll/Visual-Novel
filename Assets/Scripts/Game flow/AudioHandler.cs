using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace VisualNovel.GameFlow
{
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
                    Debug.LogError("AudioSource or AudioMixerGroup is are not defined!");
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
            foreach (var audioData in audioDatas)
            {
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

            foreach (var audioData in audioDatas)
            {
                audioData.InitializeAudioSource();

                if (audioData.name == "Ambience Player")
                {
                    _ambienceMixerGroup = audioData.audioMixerGroup;
                }
            }
        }

        private void SetAudioClip(AudioClip audioClip, string audioDataName, bool loop = false, bool playOneShot = false)
        {
            foreach (var audioData in audioDatas)
            {
                if (audioDataName == audioData.name)
                {
                    audioData.PlayClip(audioClip, loop);
                    return;
                }
            }
        }
        
        public void SetMusicClip(AudioClip musicClip, bool loop = true)
        {
            SetAudioClip(musicClip, "Music Player");
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
    }
}