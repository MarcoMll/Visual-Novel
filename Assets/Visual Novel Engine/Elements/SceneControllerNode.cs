using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace VisualNovelEngine.Elements
{
    using Utilities;
    using Utilities.Attributes;
    using Utilities.EditorTools;   

    [Serializable]
    [NodeMenu("Scene Node", "Scenery")]
    public class SceneControllerNode : GraphNode
    {
        // Data
        public SceneController ScenePrefab { get; set; }   // must be a prefab with SceneController
        public SceneController Scene { get => ScenePrefab; set => ScenePrefab = value; }
        public string SelectedPresetName { get; set; }
        public float PreviewScale { get; set; } = 1.5f;

        // UI
        private ObjectField _prefabField;
        private Image _previewImage;
        private DropdownField _presetDropdown;
        private Slider _previewScaleSlider;
        private const int BasePreviewHeight = 180;

        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);

            NodeName = "Scene node";
            Text = string.Empty;
        }

        public override void Draw()
        {
            // ----- Title -----
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);
            titleContainer.Insert(0, nodeNameTextField);
            titleContainer.Q<TextField>()?.SetEnabled(false);

            SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.SceneryIcon));

            // ----- Ports -----
            var inputPort = this.CreatePort("Input Link", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            // ----- Body -----
            // Prefab field (only accepts prefabs; component type = SceneController)
            _prefabField = new ObjectField("Scene Prefab")
            {
                objectType = typeof(SceneController),
                allowSceneObjects = false,
                value = Scene
            };
            _prefabField.RegisterValueChangedCallback(OnSceneChanged);
            extensionContainer.Add(_prefabField);

            // Slider to control preview size
            _previewScaleSlider = new Slider("Preview Scale", 0.5f, 2f)
            {
                value = PreviewScale
            };
            _previewScaleSlider.RegisterValueChangedCallback(evt =>
            {
                PreviewScale = evt.newValue;
                if (Scene != null)
                    RebuildSceneUI();
            });
            extensionContainer.Add(_previewScaleSlider);

            // Big preview (created once; only shown when a prefab is assigned)
            _previewImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };

            _previewImage.style.marginTop = 6;
            _previewImage.style.marginBottom = 6;
            _previewImage.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

            // Preset dropdown (created once; only shown when a prefab is assigned)
            _presetDropdown = new DropdownField("Scene preset")
            {
                style =
                {
                    flexGrow = 1,
                    marginTop = 2,
                    marginBottom = 0
                }
            };
            _presetDropdown.RegisterValueChangedCallback(evt => SelectedPresetName = evt.newValue);

            // Build visual state based on current data (on load/redraw)
            RebuildSceneUI();

            RefreshExpandedState();
            RefreshPorts();
        }

        // -------------------- Handlers & Helpers --------------------

        private void OnSceneChanged(ChangeEvent<Object> evt)
        {
            // Ensure selection is a prefab that has SceneController (ObjectField ensures type)
            Scene = evt.newValue as SceneController;

            // Rebuild the preview + preset dropdown visibility/content
            RebuildSceneUI();

            RefreshExpandedState();
        }

        private void RebuildSceneUI()
        {
            // Remove existing preview/dropdown if they were present
            _previewImage.RemoveFromHierarchy();
            _presetDropdown.RemoveFromHierarchy();

            if (Scene == null)
            {
                // Nothing selected: show only the field, no preview, no dropdown
                _previewScaleSlider.SetEnabled(false);
                return;
            }

            // ----- Preview -----
            _previewScaleSlider.SetEnabled(true);
            var targetHeight = Mathf.RoundToInt(BasePreviewHeight * PreviewScale);
            var texture = ScenePreviewUtility.RenderPrefabPreview(Scene.gameObject, targetHeight);
            if (texture == null)
                texture = (Texture2D)TryGetPreviewTexture(Scene);
            _previewImage.image = texture;
            _previewImage.style.width = texture != null ? texture.width : 0;
            _previewImage.style.height = texture != null ? texture.height : 0;
            _previewImage.MarkDirtyRepaint();

            extensionContainer.Add(_previewImage);

            // ----- Preset Dropdown -----
            var options = GetScenePresetNames();
            _presetDropdown.choices = options;

            // Restore selection if valid; otherwise default to first (if any)
            if (!string.IsNullOrEmpty(SelectedPresetName) && options.Contains(SelectedPresetName))
                _presetDropdown.SetValueWithoutNotify(SelectedPresetName);
            else
                _presetDropdown.SetValueWithoutNotify(options.FirstOrDefault() ?? string.Empty);

            SelectedPresetName = _presetDropdown.value;

            extensionContainer.Add(_presetDropdown);
        }

        private static Texture TryGetPreviewTexture(SceneController controller)
        {
            // Prefer a full preview; fall back to mini thumbnail if preview isn't ready
            Texture tex = null;

            if (controller != null)
            {
                // For prefab components, use their GameObject for AssetPreview
                var go = controller.gameObject;
                tex = AssetPreview.GetAssetPreview(go) ?? AssetPreview.GetMiniThumbnail(go);
            }

            return tex;
        }

        private List<string> GetScenePresetNames()
        {
            // Access static array of presets from SceneController
            var presets = Scene.ScenePresets; // ScenePreset[]; subclass of SceneController
            if (presets == null || presets.Length == 0)
                return new List<string>();

            // Each ScenePreset exposes PresetName (string)
            return presets
                .Where(p => p != null)
                .Select(p => p.PresetName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();
        }
    }
}