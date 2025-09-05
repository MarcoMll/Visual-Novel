using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;
using VisualNovel.GameFLow;

namespace VisualNovel.Environment
{
    public class SceneryDirector : MonoBehaviour, IInitializeOnAwake
    {
        private SceneController _currentScene = null;
        private string _currentScenePreset = string.Empty;

        private List<CharacterSceneData> _charactersOnSceneData;
        
        public static SceneryDirector Instance { get; private set; }
        
        private class CharacterSceneData
        {
            public SpriteRenderer characterSprite { get; set; }
            public CharacterSO character { get; set; }
            public CharacterSO.CharacterEmotion emotion { get; set; }
            public Color multiplyColor { get; set; }
            public Vector2 scenePosition { get; set; }
            public int sortingLayer { get; set; }
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

        public void ShowCharacter(CharacterSO character, CharacterSO.CharacterEmotion emotion, Color multiplyColor, Vector2 scenePosition, int sortingLayer)
        {
            var characterSceneData = FindAssociatedCharacterSceneData(character);
            var showInstantly = false;
            
            if (characterSceneData == null)
            {
                characterSceneData = RememberCharacter(character, emotion, multiplyColor, scenePosition, sortingLayer);
                showInstantly = true;
            }
            else
            {
                UpdateCharacterOnSceneData(characterSceneData, emotion, multiplyColor, scenePosition, sortingLayer);   
            }
            
            DisplayCharacter(characterSceneData, showInstantly);
        }

        private void UpdateCharacterOnSceneData(CharacterSceneData characterSceneData, CharacterSO.CharacterEmotion newEmotion, Color newMultiplyColor, Vector2 newScenePosition, int newSortingLayer)
        {
            if (NeedsUpdate(characterSceneData, newEmotion, newMultiplyColor, newScenePosition, newSortingLayer) == false) return;
            
            characterSceneData.emotion = newEmotion;
            characterSceneData.multiplyColor = newMultiplyColor;
            characterSceneData.scenePosition = newScenePosition;
            characterSceneData.sortingLayer = newSortingLayer;
        }

        private void DisplayCharacter(CharacterSceneData characterSceneData, bool showInstantly)
        {
            // implement fade out
            // fade out animation should be applied if and only if the character is currently shown on scene, otherwise instantly display it
            
            var characterSprite = characterSceneData.characterSprite;
            characterSprite.sprite = characterSceneData.emotion.sprite;
            characterSprite.color = characterSceneData.multiplyColor;
            characterSprite.transform.position = characterSceneData.scenePosition;
            characterSprite.sortingLayerID = characterSceneData.sortingLayer;
            
            // implement fade in
        }
        
        private bool NeedsUpdate(CharacterSceneData characterSceneData, CharacterSO.CharacterEmotion newEmotion, Color newMultiplyColor, Vector2 newScenePosition, int newSortingLayer)
        {
            return characterSceneData.emotion != newEmotion || characterSceneData.multiplyColor != newMultiplyColor ||
                   characterSceneData.scenePosition != newScenePosition || characterSceneData.sortingLayer != newSortingLayer;
        }
        
        private CharacterSceneData RememberCharacter(CharacterSO character, CharacterSO.CharacterEmotion emotion, Color multiplyColor,
            Vector2 scenePosition, int sceneLayer)
        {
            var characterSceneData = new CharacterSceneData
            {
                character = character,
                emotion = emotion,
                multiplyColor = multiplyColor,
                scenePosition = scenePosition,
                sortingLayer = sceneLayer
            };

            var characterGO = new GameObject(character.name);
            var characterSprite = Instantiate(characterGO).AddComponent<SpriteRenderer>();;
            characterSceneData.characterSprite = characterSprite;
            
            _charactersOnSceneData.Add(characterSceneData);
            return characterSceneData;
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