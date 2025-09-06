using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;
using VisualNovel.GameFLow;

namespace VisualNovel.Environment
{
    public class SceneEnvironmentManager : MonoBehaviour, IInitializeOnAwake
    {
        private SceneController _currentScene = null;
        private string _currentScenePreset = string.Empty;

        private List<CharacterSceneData> _charactersOnSceneData;
        
        public static SceneEnvironmentManager Instance { get; private set; }
        
        private class CharacterSceneData
        {
            public SpriteRenderer characterSprite { get; set; }
            public CharacterSO character { get; set; }
            public CharacterSO.CharacterEmotion emotion { get; set; }
            public Color multiplyColor { get; set; }
            public Vector2 scenePosition { get; set; }
            public int sortingLayer { get; set; }
            public Vector2 scale { get; set; }
            public Transform parentLayer { get; set; }
            public string parallaxLayerName { get; set; }
        }

        public void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;
            _charactersOnSceneData = new List<CharacterSceneData>();
        }
        
        public void ShowScene(SceneController newScene, string newPreset)
        {
            if (_currentScene == newScene && _currentScenePreset == newPreset)
            {
                return;
            }

            if (_currentScene == newScene && _currentScenePreset != newPreset)
            {
                _currentScene.ChangePreset(newPreset);
                return;
            }

            if (_currentScene != null)
            {
                Destroy(_currentScene);
                _currentScene = null;
            }
            
            _currentScene = Instantiate(newScene);
            _currentScene.ChangePreset(newPreset);
        }

        public bool TryGetCharacterPosition(string positionName, out Vector2 position)
        {
            position = Vector2.zero;
            if (_currentScene == null)
                return false;
            return _currentScene.TryGetCharacterPosition(positionName, out position);
        }
        
        public void ShowCharacter(CharacterSO character, CharacterSO.CharacterEmotion emotion, Color multiplyColor,
            Vector2 scenePosition, int sortingLayer, Vector2 characterScale, string parallaxLayerName)
        {
            var characterSceneData = FindAssociatedCharacterSceneData(character);
            var showInstantly = false;

            var parentLayer = GetParallaxLayerTransform(parallaxLayerName);

            if (characterSceneData == null)
            {
                characterSceneData = RememberCharacter(character, emotion, multiplyColor, scenePosition, sortingLayer, characterScale, parentLayer, parallaxLayerName);
                showInstantly = true;
            }
            else
            {
                UpdateCharacterOnSceneData(characterSceneData, emotion, multiplyColor, scenePosition, sortingLayer, characterScale, parentLayer, parallaxLayerName);
            }

            DisplayCharacter(characterSceneData, showInstantly);
        }

        private void UpdateCharacterOnSceneData(CharacterSceneData characterSceneData, CharacterSO.CharacterEmotion newEmotion,
            Color newMultiplyColor, Vector2 newScenePosition, int newSortingLayer, Vector2 newScale, Transform newParentLayer, string newParallaxName)
        {
            if (NeedsUpdate(characterSceneData, newEmotion, newMultiplyColor, newScenePosition, newSortingLayer, newScale, newParentLayer) == false) return;

            characterSceneData.emotion = newEmotion;
            characterSceneData.multiplyColor = newMultiplyColor;
            characterSceneData.scenePosition = newScenePosition;
            characterSceneData.sortingLayer = newSortingLayer;
            characterSceneData.scale = newScale;
            characterSceneData.parentLayer = newParentLayer;
            characterSceneData.parallaxLayerName = newParallaxName;
        }

        private void DisplayCharacter(CharacterSceneData characterSceneData, bool showInstantly)
        {
            // implement fade out
            // fade out animation should be applied if and only if the character is currently shown on scene, otherwise instantly display it
            
            var characterSprite = characterSceneData.characterSprite;
            characterSprite.sprite = characterSceneData.emotion.sprite;
            characterSprite.color = characterSceneData.multiplyColor;
            characterSprite.transform.position = characterSceneData.scenePosition;
            characterSprite.sortingOrder = characterSceneData.sortingLayer;
            characterSprite.transform.localScale = characterSceneData.scale;
            var parent = characterSceneData.parentLayer != null ? characterSceneData.parentLayer : _currentScene.transform;
            characterSprite.transform.SetParent(parent, false);

            // implement fade in
        }
        
        private bool NeedsUpdate(CharacterSceneData characterSceneData, CharacterSO.CharacterEmotion newEmotion, Color newMultiplyColor,
            Vector2 newScenePosition, int newSortingLayer, Vector2 newScale, Transform newParentLayer)
        {
            return characterSceneData.emotion != newEmotion || characterSceneData.multiplyColor != newMultiplyColor ||
                   characterSceneData.scenePosition != newScenePosition || characterSceneData.sortingLayer != newSortingLayer ||
                   characterSceneData.scale != newScale || characterSceneData.parentLayer != newParentLayer;
        }
        
        private CharacterSceneData RememberCharacter(CharacterSO character, CharacterSO.CharacterEmotion emotion, Color multiplyColor,
            Vector2 scenePosition, int sceneLayer, Vector2 scale, Transform parentLayer, string parallaxLayerName)
        {
            var characterSceneData = new CharacterSceneData
            {
                character = character,
                emotion = emotion,
                multiplyColor = multiplyColor,
                scenePosition = scenePosition,
                sortingLayer = sceneLayer,
                scale = scale,
                parentLayer = parentLayer,
                parallaxLayerName = parallaxLayerName
            };

            var characterSprite = new GameObject(character.name).AddComponent<SpriteRenderer>();
            characterSceneData.characterSprite = characterSprite;
            var parent = parentLayer != null ? parentLayer : _currentScene.transform;
            characterSprite.transform.SetParent(parent, false);

            _charactersOnSceneData.Add(characterSceneData);
            return characterSceneData;
        }

        private Transform GetParallaxLayerTransform(string layerName)
        {
            if (_currentScene == null)
                return null;

            var parallax = _currentScene.GetComponent<ParallaxController>();
            if (parallax?.layers == null)
                return null;

            foreach (var layer in parallax.layers)
            {
                if (layer != null && layer.name == layerName)
                    return layer;
            }

            return null;
        }
        
        private CharacterSceneData FindAssociatedCharacterSceneData(CharacterSO targetCharacter)
        {
            foreach (var characterOnScene in _charactersOnSceneData)
            {
                if (characterOnScene.character == targetCharacter)
                {
                    return characterOnScene;
                }
            }

            return null;
        }
    }
}