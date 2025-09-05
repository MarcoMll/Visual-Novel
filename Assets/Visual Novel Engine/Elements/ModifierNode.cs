using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Core;
    using Data;
    using Utilities;
    using Utilities.Attributes;

    [Serializable]
    [NodeMenu("Modifier Node", "Logic & Flow")]
    public class ModifierNode : GraphNode
    {
        public List<ItemData> ItemsToAdd { get; set; } = new();
        public List<ItemData> ItemsToRemove { get; set; } = new();
        public List<RelationshipModifier> RelationshipModifiers { get; set; } = new();
        public List<TraitData> TraitsToAdd { get; set; } = new();
        public List<FlagData> FlagsToTrigger { get; set; } = new();
        public List<FlagData> FlagsToModify { get; set; } = new();

        private void AddModifierPanel(VisualElement container, string label, Func<Action, VisualElement> createRow)
        {
            var panel = new Box();
            panel.style.flexDirection = FlexDirection.Column;
            panel.style.marginBottom = 4;

            var title = new Label(label);
            panel.Add(title);

            var row = createRow(() => panel.RemoveFromHierarchy());
            panel.Add(row);

            container.Add(panel);
        }
        
        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);
            Choices.Add("Output Link");
            NodeName = "Modifier Node";
        }

        public override void Draw()
        {
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);
            titleContainer.Insert(0, nodeNameTextField);
            titleContainer.Q<TextField>().SetEnabled(false);

            SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.ModifierIcon));

            var inputPort = this.CreatePort("Input Link", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            var foldout = new Foldout { text = "Modifiers" };
            var container = new VisualElement();
            foldout.Add(container);

            var menu = new ToolbarMenu { text = "Add Modifier" };
            menu.menu.AppendSeparator("Item Modifiers");
            menu.menu.AppendAction("Item Modifiers/Add Item", _ => AddModifierPanel(container, "Add Item", onRemove => new ItemRow(this, ItemsToAdd, null, false, () => { onRemove(); RefreshExpandedState(); })));
            menu.menu.AppendAction("Item Modifiers/Remove Item", _ => AddModifierPanel(container, "Remove Item", onRemove => new ItemRow(this, ItemsToRemove, null, false, () => { onRemove(); RefreshExpandedState(); })));
            menu.menu.AppendSeparator("Relationship Modifiers");
            menu.menu.AppendAction("Relationship Modifiers/Improve relationship", _ => AddModifierPanel(container, "Improve relationship", onRemove => new RelationshipModifierRow(this, RelationshipModifiers, new RelationshipModifier { Amount = 1 }, () => { onRemove(); RefreshExpandedState(); })));
            menu.menu.AppendAction("Relationship Modifiers/Worsen relationship", _ => AddModifierPanel(container, "Worsen relationship", onRemove => new RelationshipModifierRow(this, RelationshipModifiers, new RelationshipModifier { Amount = -1 }, () => { onRemove(); RefreshExpandedState(); })));
            menu.menu.AppendSeparator("Trait Modifiers");
            menu.menu.AppendAction("Trait Modifiers/Add trait", _ => AddModifierPanel(container, "Add trait", onRemove => new TraitRow(this, TraitsToAdd, null, false, () => { onRemove(); RefreshExpandedState(); })));
            menu.menu.AppendSeparator("Flag Modifiers");
            menu.menu.AppendAction("Flag Modifiers/Trigger flag (bool)", _ => AddModifierPanel(container, "Trigger flag", onRemove => new FlagRow(this, FlagsToTrigger, null, false, typeof(bool), () => { onRemove(); RefreshExpandedState(); })));
            menu.menu.AppendAction("Flag Modifiers/Modify value (int)", _ => AddModifierPanel(container, "Modify value", onRemove => new FlagRow(this, FlagsToModify, null, true, typeof(int), () => { onRemove(); RefreshExpandedState(); })));

            foldout.Add(menu);
            extensionContainer.Add(foldout);

            schedule.Execute(() =>
            {
                foreach (var item in ItemsToAdd.ToList())
                    AddModifierPanel(container, "Add Item", onRemove => new ItemRow(this, ItemsToAdd, item, false, () => { onRemove(); RefreshExpandedState(); }));
                foreach (var item in ItemsToRemove.ToList())
                    AddModifierPanel(container, "Remove Item", onRemove => new ItemRow(this, ItemsToRemove, item, false, () => { onRemove(); RefreshExpandedState(); }));
                foreach (var rel in RelationshipModifiers.ToList())
                {
                    var label = rel.Amount >= 0 ? "Improve relationship" : "Worsen relationship";
                    AddModifierPanel(container, label, onRemove => new RelationshipModifierRow(this, RelationshipModifiers, rel, () => { onRemove(); RefreshExpandedState(); }));
                }
                foreach (var trait in TraitsToAdd.ToList())
                    AddModifierPanel(container, "Add trait", onRemove => new TraitRow(this, TraitsToAdd, trait, false, () => { onRemove(); RefreshExpandedState(); }));
                foreach (var flag in FlagsToTrigger.ToList())
                    AddModifierPanel(container, "Trigger flag", onRemove => new FlagRow(this, FlagsToTrigger, flag, false, typeof(bool), () => { onRemove(); RefreshExpandedState(); }));
                foreach (var flag in FlagsToModify.ToList())
                    AddModifierPanel(container, "Modify value", onRemove => new FlagRow(this, FlagsToModify, flag, true, typeof(int), () => { onRemove(); RefreshExpandedState(); }));
                RefreshExpandedState();
            });
        }
    }
}
