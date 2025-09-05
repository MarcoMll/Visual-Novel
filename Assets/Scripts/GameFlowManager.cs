using System;
using UnityEngine;
using VisualNovelEngine;
using VisualNovelEngine.Data;
using VisualNovelEngine.Elements;

public class GameFlowManager : MonoBehaviour
{
    [SerializeField] private GraphContainer graph;
    private GraphTracer _graphTracer;
    
    private GraphNodeData _currentNode;

    private void Awake()
    {
        _graphTracer = new GraphTracer(graph);
    }

    public void LaunchGraph()
    {
        _currentNode = _graphTracer.LaunchFirstNode();
        ExecuteNode(_currentNode);
    }

    private void ExecuteNode(GraphNodeData nodeObject)
    {
        var nodeType = _graphTracer.GetNodeType(nodeObject);
        
        switch (nodeType)
        {
            case TextNode textNode:
                break;
            case ChoiceNode choiceNode:
                break;
            case ConditionCheckNode conditionNode:
                break;
            case ModifierNode modifierNode:
                break;
            case SceneControllerNode sceneNode:
                break;
            case ShowCharacterNode characterNode:
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
}