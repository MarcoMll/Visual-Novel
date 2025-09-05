using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace VisualNovelEngine.Core
{
    using Elements;

    /// <summary>
    /// Centralised place for defining rules about which nodes can connect to which.
    /// Rules are defined per node type and are easy to extend.
    /// </summary>
    public static class NodeConnectionRules
    {
        // Map from node type (output node) to rule delegate.
        private static readonly Dictionary<Type, Func<Port, Port, bool>> OutputRules = new()
        {
            { typeof(TextNode), TextNodeRule },
            { typeof(ChoiceNode), ChoiceNodeRule },
            { typeof(ConditionCheckNode), ConditionNodeRule }
        };

        /// <summary>
        /// Checks whether the output port can connect to the input port.
        /// </summary>
        public static bool CanConnect(Port output, Port input)
        {
            var outputType = output.node.GetType();
            if (OutputRules.TryGetValue(outputType, out var rule))
            {
                return rule(output, input);
            }
            return true; // No rule -> allow by default
        }

        private static bool TextNodeRule(Port output, Port input)
        {
            var targetType = input.node.GetType();

            bool connectedToText = output.connections
                .Any(e => GetOtherPort(e, output).node is TextNode);
            bool connectedToChoice = output.connections
                .Any(e => GetOtherPort(e, output).node is ChoiceNode);

            if (targetType == typeof(TextNode))
            {
                // Allow only one text node connection and do not mix with choice nodes
                if (connectedToText || connectedToChoice)
                    return false;
            }
            else if (targetType == typeof(ChoiceNode))
            {
                // Can't connect to choice if already connected to text
                if (connectedToText)
                    return false;
            }

            return true;
        }

        private static bool ChoiceNodeRule(Port output, Port input)
        {
            // A choice node cannot connect to another choice node.
            return input.node is not ChoiceNode;
        }

        private static bool ConditionNodeRule(Port output, Port input)
        {
            // Condition node outputs may only connect to text or choice nodes.
            return input.node is TextNode || input.node is ChoiceNode;
        }

        private static Port GetOtherPort(Edge edge, Port port)
            => edge.input == port ? edge.output : edge.input;
    }
}
