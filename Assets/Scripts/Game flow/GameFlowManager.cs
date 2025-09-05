using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VisualNovelEngine.Data;
using VisualNovelEngine.Elements;

namespace VisualNovel.GameFLow
{
    using UI;
    using Environment;
    
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

        public void LaunchGraph()
        {
            _currentNode = _graphTracer.LaunchFirstNode();
            ExecuteNode(_currentNode);
        }

        public void LaunchNextNodes()
        {
            var linkedNodes = _graphTracer.GetConnectedNodes(_currentNode.GUID);
            foreach (var node in linkedNodes)
            {
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
                    break;
                case ConditionCheckNode conditionNode:
                    break;
                case ModifierNode modifierNode:
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

        private void ExecuteTextNode(TextNode textNode)
        {
            var dialogueTextController = UIDialogueTextController.Instance;
            if (dialogueTextController == null)
            {
                Debug.LogError("UIDialogueTextController is absent on the scene or was not initialized before usage!");
                return;
            }
            
            var text = textNode.Text;
            
            if (textNode.IsDialogue)
            {
                var speakerName = textNode.Speaker.characterName;
                dialogueTextController.PlayText(text, speakerName);
            }
            else
            {
                dialogueTextController.PlayText(textNode.Text);
            }
        }

        private void ExecuteSceneNode(SceneControllerNode sceneNode)
        {
            var sceneDirector = SceneryDirector.Instance;
            if (sceneDirector == null)
            {
                Debug.LogError("SceneControllerNode is absent on the scene or was not initialized before usage!");
                return;
            }

            sceneDirector.ShowScene(sceneNode.ScenePrefab, sceneNode.SelectedPresetName);
        }

        private void ExecuteCharacterNode(ShowCharacterNode characterNode)
        {
            var sceneDirector = SceneryDirector.Instance;
            if (sceneDirector == null)
            {
                Debug.LogError("SceneControllerNode is absent on the scene or was not initialized before usage!");
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
                sceneDirector.TryGetCharacterPosition(characterEntry.SelectedPositionName, out basePosition);
                var finalPosition = basePosition + characterEntry.Offset;

                sceneDirector.ShowCharacter(characterEntry.Character, emotion, characterEntry.SpriteColor,
                    finalPosition, characterEntry.Layer, characterEntry.CharacterScale);
            }
        }
    }
}