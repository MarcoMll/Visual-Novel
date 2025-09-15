using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CustomInspector.Documentation
{
    public enum NewPropertyD
    {
        //attributes
        AsRangeAttribute,
        AssetsOnlyAttribute,
        BackgroundColorAttribute,
        ButtonAttribute,
        CopyPasteAttribute,
        DecimalsAttribute,
        DisplayAutoPropertyAttribute,
        ForceFillAttribute,
        GetSetAttribute,
        GUIColorAttribute,
        HideFieldAttribute,
        HookAttribute,
        HorizontalGroupAttribute,
        HorizontalLineAttribute,
        IndentAttribute,
        LabelSettingsAttribute,
        LayerAttribute,
        MaskAttribute,
        MaxAttribute,
        MessageBoxAttribute,
        MultipleOfAttribute,
        PreviewAttribute,
        ProgressBarAttribute,
        ReadOnlyAttribute,
        RequireTypeAttribute,
        SelfFillAttribute,
        ShowIfAttribute,
        ShowIfIsAttribute,
        ShowIfNotAttribute,
        ShowMethodAttribute,
        TagAttribute,
        ToolbarAttribute,
        UnwrapAttribute,
        URLAttribute,
        ValidateAttribute,
        //Types
        DynamicSlider,
        FilePath,
        FolderPath,
        MessageDrawer,
        SerializableDictionary,
        SerializableSortedDictionary,
        SerializableSet,
        SerializableSortedSet,
        StaticsDrawer,
        //Unity
        DelayedAttribute,
        HeaderAttribute,
        MinAttribute,
        MultilineAttribute,
        NonReordableAttribute,
        RangeAttribute,
        SpaceAttribute,
        TooltipAttribute,
        TextAreaAttribute,
    }

    /// <summary>
    /// Groups propertys and defines custom order
    /// </summary>
    public static class PropertyDList
    {
        /// <summary>
        /// Spacing before each header
        /// </summary>
        public const float headerSpacing = 20;
        /// <summary>
        /// Height of header label
        /// </summary>
        public const float headerHeight = 20;
        /// <summary>
        /// The y-position distance addition a header does
        /// </summary>
        public const float headerDistance = headerSpacing + headerHeight;
        /// <summary>
        /// Spacing between entrys
        /// </summary>
        public const float entrySpacing = 7;
        /// <summary>
        /// Height of a single entry
        /// </summary>
        public const float entryHeight = 40;
        /// <summary>
        /// The y-position distance between two entrys
        /// </summary>
        public const float entryDistance = entrySpacing + entryHeight;
        

        //------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The sections of NewPropertyD
        /// </summary>
        public static readonly (string header, List<NewPropertyD> entrys)[] Sections;
        /// <summary>
        /// The height of all headers and entrys and their spacing
        /// </summary>
        public static readonly float TotalHeight;
        /// <summary>
        /// The height of the whole section (header plus entrys)
        /// </summary>
        public static readonly float MostUsedHeight;

        /// <summary>
        /// Constructor
        /// </summary>
        static PropertyDList()
        {
            Sections = new (string header, List<NewPropertyD> entrys)[]
            {
                ("Most used:", mostUsed),
                ("Attributes:", attributes),
                ("Types:", types),
                ("unitys field-attributes:", unityBuildIn)
            };

            TotalHeight = Sections.Length * headerDistance
                + Sections.Select(_ => _.entrys.Count).Sum() * entryDistance;

            MostUsedHeight = headerDistance + mostUsed.Count * entryDistance;
        }

        static readonly List<NewPropertyD> mostUsed = new()
        {
            NewPropertyD.ButtonAttribute,
            NewPropertyD.ForceFillAttribute,
            NewPropertyD.HideFieldAttribute,
            NewPropertyD.HorizontalLineAttribute,
            NewPropertyD.ReadOnlyAttribute,
            NewPropertyD.SelfFillAttribute,
            NewPropertyD.ShowIfAttribute,
        };
        static readonly List<NewPropertyD> attributes = new()
        {
            NewPropertyD.AsRangeAttribute,
            NewPropertyD.AssetsOnlyAttribute,
            NewPropertyD.BackgroundColorAttribute,
            NewPropertyD.ButtonAttribute,
            NewPropertyD.CopyPasteAttribute,
            NewPropertyD.DecimalsAttribute,
            NewPropertyD.DisplayAutoPropertyAttribute,
            NewPropertyD.ForceFillAttribute,
            NewPropertyD.GetSetAttribute,
            NewPropertyD.GUIColorAttribute,
            NewPropertyD.HideFieldAttribute,
            NewPropertyD.HookAttribute,
            NewPropertyD.HorizontalGroupAttribute,
            NewPropertyD.HorizontalLineAttribute,
            NewPropertyD.IndentAttribute,
            NewPropertyD.LabelSettingsAttribute,
            NewPropertyD.LayerAttribute,
            NewPropertyD.MaskAttribute,
            NewPropertyD.MaxAttribute,
            NewPropertyD.MessageBoxAttribute,
            NewPropertyD.MultipleOfAttribute,
            NewPropertyD.PreviewAttribute,
            NewPropertyD.ProgressBarAttribute,
            NewPropertyD.ReadOnlyAttribute,
            NewPropertyD.RequireTypeAttribute,
            NewPropertyD.SelfFillAttribute,
            NewPropertyD.ShowIfAttribute,
            NewPropertyD.ShowIfIsAttribute,
            NewPropertyD.ShowIfNotAttribute,
            NewPropertyD.ShowMethodAttribute,
            NewPropertyD.TagAttribute,
            NewPropertyD.ToolbarAttribute,
            NewPropertyD.UnwrapAttribute,
            NewPropertyD.URLAttribute,
            NewPropertyD.ValidateAttribute,
        };
        static readonly List<NewPropertyD> types = new List<NewPropertyD>()
        {
            NewPropertyD.DynamicSlider,
            NewPropertyD.FilePath,
            NewPropertyD.FolderPath,
            NewPropertyD.MessageDrawer,
            NewPropertyD.SerializableDictionary,
            NewPropertyD.SerializableSortedDictionary,
            NewPropertyD.SerializableSet,
            NewPropertyD.SerializableSortedSet,
            NewPropertyD.StaticsDrawer
        };
        static readonly List<NewPropertyD> unityBuildIn = new List<NewPropertyD>()
        {
            NewPropertyD.DelayedAttribute,
            NewPropertyD.HeaderAttribute,
            NewPropertyD.MinAttribute,
            NewPropertyD.MultilineAttribute,
            NewPropertyD.NonReordableAttribute,
            NewPropertyD.RangeAttribute,
            NewPropertyD.SpaceAttribute,
            NewPropertyD.TooltipAttribute,
            NewPropertyD.TextAreaAttribute,
        };
    }
}