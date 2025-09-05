using System;

namespace VisualNovelEngine.Utilities.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NodeMenuAttribute : Attribute
    {
        /// <summary>
        /// The name of the node shown in menus.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Optional group used to organise nodes in menus/search windows.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Convenience property combining <see cref="Group"/> and
        /// <see cref="Name"/> for Unity's menu path format.
        /// </summary>
        public string MenuPath => string.IsNullOrEmpty(Group) ? Name : $"{Group}/{Name}";

        public NodeMenuAttribute(string name, string group = "")
        {
            Name = name;
            Group = group;
        }
    }
}