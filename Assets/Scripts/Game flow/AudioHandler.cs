using System;
using UnityEngine;
using UnityEngine.Audio;

namespace VisualNovel.GameFlow
{
    public class AudioHandler : MonoBehaviour, IInitializeOnAwake
    {
        [SerializeField] private AudioData[] audioDatas;
        public static AudioHandler Instance { get; private set; }

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
            SetAudioClip(ambienceClip, "Ambience Player");
        }

        public void PlaySfx(AudioClip soundEffectClip)
        {
            SetAudioClip(soundEffectClip, "SFX Player", playOneShot: true);
        }
    }
}