using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VisualNovel.Environment
{
    public class SceneController : MonoBehaviour
    {
    [SerializeField] private ScenePreset[] scenePresets;
    [SerializeField] private CharacterPosition[] characterPositions;

    // Assign these once in the Inspector to the SpriteRenderers you want to tint.
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer skyboxRenderer;

    // Assign a prefab or configured AudioSource here.
    // This AudioSource will be instantiated (cloned) for each ambience clip in a preset.
    [SerializeField] private AudioSource ambiencePlayerPrefab;

    // Keeps track of which preset is currently active
    private ScenePreset currentPreset = null;

    // Map from AudioClip → its instantiated AudioSource so we can stop/destroy when needed
    private readonly Dictionary<AudioClip, AudioSource> activeAmbienceSources = new Dictionary<AudioClip, AudioSource>();

    public ScenePreset[] ScenePresets => scenePresets;
    
    [Serializable]
    public class LightSource
    {
        [SerializeField] private Light2D light;
        [SerializeField] private Color color;
        [SerializeField] private float intensity;

        public Light2D Light => light;
        public Color Color => color;
        public float Intensity => intensity;
    }

    [Serializable]
    public class AmbienceClip
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        public AudioClip Clip => clip;
        public float Volume => volume;
    }

    [Serializable]
    public class ScenePreset
    {
        [Header("Preset name")]
        [SerializeField] private string presetName;

        [Header("Main setup")]
        [SerializeField] private LightSource globalLight;
        [SerializeField] private Color skyColor;
        [SerializeField] private Color backgroundColor;

        [Header("Other settings")]
        [SerializeField] private LightSource[] sceneLightSources;
        [SerializeField] private ParticleSystem[] sceneParticles;
        [SerializeField] private GameObject[] additionalSceneObjects;

        [Header("Audio ambience")]
        [Tooltip("List of ambience clips (with per-clip volume). If absent, no ambience plays.")]
        [SerializeField] private AmbienceClip[] ambienceClips;

        #region Public getters

        public string PresetName => presetName;
        public LightSource GlobalLight => globalLight;
        public Color SkyColor => skyColor;
        public Color BackgroundColor => backgroundColor;
        public LightSource[] SceneLightSources => sceneLightSources;
        public ParticleSystem[] SceneParticles => sceneParticles;
        public GameObject[] AdditionalSceneObjects => additionalSceneObjects;
        public AmbienceClip[] AmbienceClips => ambienceClips;

        #endregion
    }

    [Serializable]
    public class CharacterPosition
    {
        public string positionName;
        public Vector2 position;
        public int layer;
        public Vector2 size;
        public Vector2 defaultCharacterScale;
    }
    
    /// <summary>
    /// Switches from the current preset (if any) into the one matching "presetName."
    /// </summary>
    public void ChangePreset(string presetName)
    {
        ScenePreset newPreset = FindPreset(presetName);
        if (newPreset == null)
        {
            Debug.LogWarning($"[SceneController] No preset found with name \"{presetName}\".");
            return;
        }

        // If it's already active, do nothing.
        if (currentPreset == newPreset)
            return;

        InitializeScenePreset(currentPreset, newPreset);
        currentPreset = newPreset;
    }
    
    private void InitializeScenePreset(ScenePreset oldPreset, ScenePreset newPreset)
    {
        // 1) GLOBAL LIGHT
        if (oldPreset != null)
        {
            var oldGL = oldPreset.GlobalLight;
            var newGL = newPreset.GlobalLight;

            if (oldGL.Light != newGL.Light)
            {
                if (oldGL.Light != null)
                    oldGL.Light.enabled = false;

                if (newGL.Light != null)
                {
                    newGL.Light.enabled = true;
                    newGL.Light.color = newGL.Color;
                    newGL.Light.intensity = newGL.Intensity;
                }
            }
            else
            {
                if (newGL.Light != null)
                {
                    newGL.Light.enabled = true;
                    newGL.Light.color = newGL.Color;
                    newGL.Light.intensity = newGL.Intensity;
                }
            }
        }
        else
        {
            var newGL = newPreset.GlobalLight;
            if (newGL.Light != null)
            {
                newGL.Light.enabled = true;
                newGL.Light.color = newGL.Color;
                newGL.Light.intensity = newGL.Intensity;
            }
        }

        // 2) OTHER LIGHT SOURCES
        if (oldPreset != null)
        {
            foreach (var oldLS in oldPreset.SceneLightSources)
            {
                bool foundInNew = false;
                foreach (var newLS in newPreset.SceneLightSources)
                {
                    if (oldLS.Light == newLS.Light)
                    {
                        foundInNew = true;
                        break;
                    }
                }

                if (!foundInNew && oldLS.Light != null)
                    oldLS.Light.enabled = false;
            }
        }

        foreach (var newLS in newPreset.SceneLightSources)
        {
            bool foundInOld = false;
            if (oldPreset != null)
            {
                foreach (var oldLS in oldPreset.SceneLightSources)
                {
                    if (newLS.Light == oldLS.Light)
                    {
                        foundInOld = true;
                        break;
                    }
                }
            }

            if (foundInOld)
            {
                if (newLS.Light != null)
                {
                    newLS.Light.enabled = true;
                    newLS.Light.color = newLS.Color;
                    newLS.Light.intensity = newLS.Intensity;
                }
            }
            else
            {
                if (newLS.Light != null)
                {
                    newLS.Light.enabled = true;
                    newLS.Light.color = newLS.Color;
                    newLS.Light.intensity = newLS.Intensity;
                }
            }
        }

        // 3) PARTICLE SYSTEMS
        if (oldPreset != null)
        {
            foreach (var oldPS in oldPreset.SceneParticles)
            {
                bool foundInNew = false;
                foreach (var newPS in newPreset.SceneParticles)
                {
                    if (oldPS == newPS)
                    {
                        foundInNew = true;
                        break;
                    }
                }

                if (!foundInNew && oldPS != null)
                    oldPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        foreach (var newPS in newPreset.SceneParticles)
        {
            bool foundInOld = false;
            if (oldPreset != null)
            {
                foreach (var oldPS in oldPreset.SceneParticles)
                {
                    if (newPS == oldPS)
                    {
                        foundInOld = true;
                        break;
                    }
                }
            }

            if (!foundInOld && newPS != null)
                newPS.Play();
        }

        // 4) ADDITIONAL SCENE OBJECTS
        if (oldPreset != null)
        {
            foreach (var oldGO in oldPreset.AdditionalSceneObjects)
            {
                bool foundInNew = false;
                foreach (var newGO in newPreset.AdditionalSceneObjects)
                {
                    if (oldGO == newGO)
                    {
                        foundInNew = true;
                        break;
                    }
                }

                if (!foundInNew && oldGO != null)
                    oldGO.SetActive(false);
            }
        }

        foreach (var newGO in newPreset.AdditionalSceneObjects)
        {
            bool foundInOld = false;
            if (oldPreset != null)
            {
                foreach (var oldGO in oldPreset.AdditionalSceneObjects)
                {
                    if (newGO == oldGO)
                    {
                        foundInOld = true;
                        break;
                    }
                }
            }

            if (!foundInOld && newGO != null)
                newGO.SetActive(true);
        }

        // 5) SKYBOX & BACKGROUND
        if (oldPreset == null || newPreset.SkyColor != oldPreset.SkyColor)
        {
            if (skyboxRenderer != null)
                skyboxRenderer.color = newPreset.SkyColor;
        }

        if (oldPreset == null || newPreset.BackgroundColor != oldPreset.BackgroundColor)
        {
            if (backgroundRenderer != null)
                backgroundRenderer.color = newPreset.BackgroundColor;
        }

        // 6) AUDIO AMBIENCE
        // --- Stop any old ambience clips not in the new preset
        if (oldPreset != null)
        {
            foreach (var oldAmb in oldPreset.AmbienceClips)
            {
                bool foundInNew = false;
                foreach (var newAmb in newPreset.AmbienceClips)
                {
                    if (oldAmb.Clip == newAmb.Clip)
                    {
                        foundInNew = true;
                        break;
                    }
                }

                if (!foundInNew && oldAmb.Clip != null)
                {
                    if (activeAmbienceSources.TryGetValue(oldAmb.Clip, out var src))
                    {
                        src.Stop();
                        Destroy(src.gameObject);
                        activeAmbienceSources.Remove(oldAmb.Clip);
                    }
                }
            }
        }

        // --- Play or update any new ambience clips
        foreach (var newAmb in newPreset.AmbienceClips)
        {
            if (newAmb.Clip == null)
                continue;

            bool foundInOld = false;
            if (oldPreset != null)
            {
                foreach (var oldAmb in oldPreset.AmbienceClips)
                {
                    if (newAmb.Clip == oldAmb.Clip)
                    {
                        foundInOld = true;
                        break;
                    }
                }
            }

            if (foundInOld)
            {
                // Shared: just update volume if changed
                if (activeAmbienceSources.TryGetValue(newAmb.Clip, out var existingSrc))
                {
                    existingSrc.volume = newAmb.Volume;
                    if (!existingSrc.isPlaying)
                        existingSrc.Play();
                }
            }
            else
            {
                // Brand-new ambience clip: instantiate from the prefab and play
                if (ambiencePlayerPrefab != null)
                {
                    AudioSource srcInstance = Instantiate(ambiencePlayerPrefab, transform);
                    srcInstance.clip = newAmb.Clip;
                    srcInstance.volume = newAmb.Volume;
                    srcInstance.loop = true;
                    srcInstance.Play();
                    activeAmbienceSources.Add(newAmb.Clip, srcInstance);
                }
                else
                {
                    Debug.LogWarning("[SceneController] AmbiencePlayerPrefab is not assigned in the Inspector.");
                }
            }
        }
    }
    
    /// <summary>
    /// Returns the ScenePreset whose PresetName matches, or null if none.
    /// </summary>
    private ScenePreset FindPreset(string presetName)
    {
        if (scenePresets == null)
            return null;

        foreach (var preset in scenePresets)
        {
            if (preset.PresetName == presetName)
                return preset;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (characterPositions.Length == 0) return;

        foreach (var characterPosition in characterPositions)
        {
            Gizmos.DrawWireCube(characterPosition.position, characterPosition.size);
        }
    }
    }
}
