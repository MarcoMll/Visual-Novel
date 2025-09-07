using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VisualNovelEngine.Data;
using VisualNovelEngine.Elements;

namespace VisualNovel.GameFlow
{
    using UI;
    using Environment;
    using Data;
    
    public class GameFlowManager : MonoBehaviour
    {
        [SerializeField] private GraphContainer graph;
        
        private GraphTracer _graphTracer;
        private GraphNodeData _currentNode;

        private void Awake()
        {
            _graphTracer = new GraphTracer(graph);
        }

        private void Start()
        {
            FindAndInitializeAllComponents();
            LaunchGraph();
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                LaunchNextNodes();
            }
        }

        private void FindAndInitializeAllComponents()
        {
            var initializables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IInitializeOnAwake>();
            
            foreach (var initializable in initializables)
            {
                initializable.Initialize();
            }
        }

        private void LaunchGraph()
        {
            var startingNodes = _graphTracer.GetAdjacentNodesFromStart();
            if (startingNodes.Count == 0) return;

            foreach (var node in startingNodes)
            {
                var nodeType = _graphTracer.GetNodeType(node);
                ExecuteNode(node);

                if (nodeType is TextNode)
                    _currentNode = node;
            }
        }

        private void LaunchNextNodes()
        {
            var linkedNodes = _graphTracer.GetConnectedNodes(_currentNode.GUID);
            if (linkedNodes.Count == 0) return;
            
            foreach (var node in linkedNodes)
            {
                //TO BE CHANGED LATER
                var nodeType = _graphTracer.GetNodeType(node);
                if (nodeType is TextNode)
                    _currentNode = node;
                
                ExecuteNode(node);
            }
        }
        
        private void ExecuteNode(GraphNodeData nodeObject)
        {
            var nodeType = _graphTracer.GetNodeType(nodeObject);

            switch (nodeType)
            {
                case TextNode textNode:
                    ExecuteTextNode(textNode);
                    break;
                case ChoiceNode choiceNode:
                    ExecuteChoiceNode(choiceNode);
                    break;
                case ConditionCheckNode conditionNode:
                    break;
                case ModifierNode modifierNode:
                    ExecuteModifierNode(modifierNode);
                    break;
                case SceneControllerNode sceneNode:
                    ExecuteSceneNode(sceneNode);
                    break;
                case ShowCharacterNode characterNode:
                    ExecuteCharacterNode(characterNode);
                    break;
                case AudioNode audioNode:
                    break;
                case DelayNode delayNode:
                    break;
                default:
                    Debug.LogError($"Unsupported node type detected: {nodeObject}");
                    break;
            }
        }

        private bool ConditionNodeRequirementsMet(ConditionCheckNode conditionNode)
        {
            var gameDataManager = GameDataManager.Instance;
            if (gameDataManager == null)
            {
                Debug.LogError("GameDataManager is absent on the scene or was not initialized before usage!");
                return false;
            }
            
            // ----- check item conditions -----
            foreach (var itemData in conditionNode.Items)
            {
                if (gameDataManager.playerInventory.HasItem(itemData.Item) == false) return false;
            }
            
            // ----- check trait conditions -----
            foreach (var traitData in conditionNode.Traits)
            {
                if (gameDataManager.playerTraitCollection.HasTrait(traitData.Trait) == false) return false;
            }
            
            // ----- check flag conditions -----
            foreach (var flagData in conditionNode.Flags)
            {
                if (gameDataManager.flagCollection.GetFlag(flagData.FlagName) != flagData.BoolValue) return false;
                // not implemented for integer flags yet
            }

            return true;
        }
        
        private void ExecuteModifierNode(ModifierNode modifierNode)
        {
            var gameDataManager = GameDataManager.Instance;
            if (gameDataManager == null)
            {
                Debug.LogError("GameDataManager is absent on the scene or was not initialized before usage!");
                return;
            }
            
            // ----- check relationship modifiers -----
            foreach (var relationshipModifier in modifierNode.RelationshipModifiers)
            {
                var character = relationshipModifier.Character;
                var amount = relationshipModifier.Amount;
                
                gameDataManager.characterCollection.ModifyRelationship(character, amount);
            }
            
            // ----- check trait modifiers ------
            foreach (var traitData in modifierNode.TraitsToAdd)
            {
                gameDataManager.playerTraitCollection.AddTrait(traitData.Trait);
            }
            
            // ----- check item modifiers -----
            // items to add
            foreach (var itemData in modifierNode.ItemsToAdd)
            {
                gameDataManager.playerInventory.AddItem(itemData.Item);
            }
            
            // items to remove
            foreach (var itemData in modifierNode.ItemsToRemove)
            {
                gameDataManager.playerInventory.RemoveItem(itemData.Item);
            }
            
            // ----- check flag modifiers -----
            // boolean flags
            foreach (var flagData in modifierNode.FlagsToTrigger)
            {
                gameDataManager.flagCollection.SetFlag(flagData.FlagName, true);
            }
            
            // integer flags
            foreach (var flagData in modifierNode.FlagsToModify)
            {
                // not implemented yet
                //gameDataManager.flagCollection.SetFlag(flagData.FlagName, flagData.IntValue);
            }
        }

        private void ExecuteChoiceNode(ChoiceNode choiceNode)
        {
            var choiceHandler = ChoiceHandler.Instance;
            if (choiceHandler == null)
            {
                Debug.LogError("ChoiceHandler is absent on the scene or was not initialized before usage!");
                return;
            }
            
            choiceHandler.AddChoice(choiceNode.Text, () =>
            {
                var linkedNodes = _graphTracer.GetConnectedNodes(choiceNode.GUID);
                foreach (var node in linkedNodes)
                {
                    var nodeType = _graphTracer.GetNodeType(node);
                    if (nodeType is TextNode)
                        _currentNode = node;

                    ExecuteNode(node);
                }
            });
        }

        private void ExecuteTextNode(TextNode textNode)
        {
            var dialogueTextController = UIDialogueTextController.Instance;
            if (dialogueTextController == null)
            {
                Debug.LogError("UIDialogueTextController is absent on the scene or was not initialized before usage!");
                return;
            }
            
            if (textNode.IsDialogue)
            {
                var speakerName = textNode.Speaker.characterName;
                dialogueTextController.PlayText(textNode.Text, speakerName);
            }
            else
            {
                dialogueTextController.PlayText(textNode.Text);
            }

            // After displaying the text, immediately execute any connected ChoiceNodes to allow the player to make a choice
            var choiceHandler = ChoiceHandler.Instance;
            if (choiceHandler == null)
            {
                Debug.LogError("ChoiceHandler is absent on the scene or was not initialized before usage!");
                return;
            }

            choiceHandler.ClearChoices();
            
            var linkedNodes = _graphTracer.GetConnectedNodes(textNode.GUID);
            var hasChoices = false;
            
            if (linkedNodes.Count == 0) return;
            
            foreach (var node in linkedNodes)
            {
                var nodeType = _graphTracer.GetNodeType(node);
                if (nodeType is ChoiceNode choiceNode)
                {
                    hasChoices = true;
                    ExecuteChoiceNode(choiceNode);
                }
            }

            if (hasChoices)
                choiceHandler.ShowChoices();
        }

        private void ExecuteSceneNode(SceneControllerNode sceneNode)
        {
            var sceneManager = SceneEnvironmentManager.Instance;
            if (sceneManager == null)
            {
                Debug.LogError("SceneEnvironmentManager is absent on the scene or was not initialized before usage!");
                return;
            }

            sceneManager.ShowScene(sceneNode.ScenePrefab, sceneNode.SelectedPresetName);
        }

        private void ExecuteCharacterNode(ShowCharacterNode characterNode)
        {
            var sceneManager = SceneEnvironmentManager.Instance;
            if (sceneManager == null)
            {
                Debug.LogError("SceneEnvironmentManager is absent on the scene or was not initialized before usage!");
                return;
            }

            foreach (var characterEntry in characterNode.Characters)
            {
                if (characterEntry.Character == null) continue;

                // Find the emotion object by name
                var emotion = characterEntry.Character.characterEmotionSpriteSheet
                    .FirstOrDefault(e => e != null && e.spriteName == characterEntry.SelectedEmotion);
                if (emotion == null) continue;

                // Resolve final position: predefined character position + offset
                Vector2 basePosition;
                sceneManager.TryGetCharacterPosition(characterEntry.SelectedPositionName, out basePosition);
                var finalPosition = basePosition + characterEntry.Offset;

                sceneManager.ShowCharacter(characterEntry.Character, emotion, characterEntry.SpriteColor,
                    finalPosition, characterEntry.Layer, characterEntry.CharacterScale, characterEntry.SelectedParallaxLayer);
            }
        }
    }
}