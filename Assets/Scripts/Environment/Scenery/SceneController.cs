using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VisualNovel.Environment
{
    /// <summary>
    /// Controls visual scene presets: lights, particles, additional objects and colors.
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        [SerializeField] private ScenePreset[] scenePresets;
        [SerializeField] private CharacterPosition[] characterPositions;

        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private SpriteRenderer skyboxRenderer;

        private ScenePreset currentPreset = null;

        public ScenePreset[] ScenePresets => scenePresets;

        #region Nested types

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

            public string PresetName => presetName;
            public LightSource GlobalLight => globalLight;
            public Color SkyColor => skyColor;
            public Color BackgroundColor => backgroundColor;
            public LightSource[] SceneLightSources => sceneLightSources;
            public ParticleSystem[] SceneParticles => sceneParticles;
            public GameObject[] AdditionalSceneObjects => additionalSceneObjects;
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

        #endregion

        #region Character positions

        public bool TryGetCharacterPosition(string positionName, out Vector2 position)
        {
            position = Vector2.zero;
            foreach (var characterPosition in characterPositions)
            {
                if (characterPosition.positionName == positionName)
                {
                    position = characterPosition.position;
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Preset management

        /// <summary>
        /// Switches the active preset to the one with the specified name.
        /// </summary>
        public void ChangePreset(string presetName)
        {
            var newPreset = FindPreset(presetName);
            if (newPreset == null)
            {
                Debug.LogWarning($"[SceneController] No preset found with name \"{presetName}\".");
                return;
            }

            if (currentPreset == newPreset)
                return;

            ApplyPreset(currentPreset, newPreset);
            currentPreset = newPreset;
        }

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

        #endregion

        #region Preset application helpers

        private void ApplyPreset(ScenePreset oldPreset, ScenePreset newPreset)
        {
            ApplyGlobalLight(oldPreset?.GlobalLight, newPreset.GlobalLight);
            ApplyLightSources(oldPreset?.SceneLightSources, newPreset.SceneLightSources);
            ApplyParticles(oldPreset?.SceneParticles, newPreset.SceneParticles);
            ApplyObjects(oldPreset?.AdditionalSceneObjects, newPreset.AdditionalSceneObjects);
            ApplyColors(newPreset);
        }

        private static void ApplyGlobalLight(LightSource oldLight, LightSource newLight)
        {
            if (oldLight != null && oldLight.Light != null && oldLight.Light != newLight.Light)
            {
                oldLight.Light.enabled = false;
            }

            if (newLight.Light != null)
            {
                var light = newLight.Light;
                light.enabled = true;
                light.color = newLight.Color;
                light.intensity = newLight.Intensity;
            }
        }

        private static void ApplyLightSources(LightSource[] oldSources, LightSource[] newSources)
        {
            var newSet = new HashSet<Light2D>();
            if (newSources != null)
            {
                foreach (var ls in newSources)
                {
                    if (ls.Light != null)
                        newSet.Add(ls.Light);
                }
            }

            if (oldSources != null)
            {
                foreach (var ls in oldSources)
                {
                    if (ls.Light != null && !newSet.Contains(ls.Light))
                        ls.Light.enabled = false;
                }
            }

            if (newSources != null)
            {
                foreach (var ls in newSources)
                {
                    if (ls.Light == null)
                        continue;

                    var light = ls.Light;
                    light.enabled = true;
                    light.color = ls.Color;
                    light.intensity = ls.Intensity;
                }
            }
        }

        private static void ApplyParticles(ParticleSystem[] oldParticles, ParticleSystem[] newParticles)
        {
            var newSet = new HashSet<ParticleSystem>(newParticles ?? Array.Empty<ParticleSystem>());

            if (oldParticles != null)
            {
                foreach (var ps in oldParticles)
                {
                    if (ps != null && !newSet.Contains(ps))
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            if (newParticles != null)
            {
                foreach (var ps in newParticles)
                {
                    if (ps != null)
                        ps.Play();
                }
            }
        }

        private static void ApplyObjects(GameObject[] oldObjects, GameObject[] newObjects)
        {
            var newSet = new HashSet<GameObject>(newObjects ?? Array.Empty<GameObject>());

            if (oldObjects != null)
            {
                foreach (var go in oldObjects)
                {
                    if (go != null && !newSet.Contains(go))
                        go.SetActive(false);
                }
            }

            if (newObjects != null)
            {
                foreach (var go in newObjects)
                {
                    if (go != null)
                        go.SetActive(true);
                }
            }
        }

        private void ApplyColors(ScenePreset preset)
        {
            if (skyboxRenderer != null)
                skyboxRenderer.color = preset.SkyColor;

            if (backgroundRenderer != null)
                backgroundRenderer.color = preset.BackgroundColor;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            if (characterPositions == null || characterPositions.Length == 0)
                return;

            foreach (var characterPosition in characterPositions)
            {
                Gizmos.DrawWireCube(characterPosition.position, characterPosition.size);
            }
        }
    }
}

