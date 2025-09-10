using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Utilities;
    using Utilities.Attributes;
    using VisualNovel.Environment;
    using VisualNovel.Minigames;
    using VisualNovel.Minigames.Combat;

    [Serializable]
    [NodeMenu("Combat Minigame", "Logic & Flow")]
    public class CombatMinigameNode : GraphNode
    {
        // Data
        public CombatMinigame MinigamePrefab { get; set; }
        public SceneController Scene { get; set; }
        public string SelectedParallaxLayer { get; set; }
        public Vector2 CharacterOffset { get; set; }

        [Serializable]
        public class FighterEntry : FighterData
        {
            public string SelectedEmotion;

            [NonSerialized] public ObjectField characterField;
            [NonSerialized] public DropdownField emotionDropdown;
            [NonSerialized] public IntegerField healthField;
            [NonSerialized] public IntegerField actionField;
            [NonSerialized] public IntegerField damageField;
            [NonSerialized] public IntegerField layerField;
            [NonSerialized] public Vector2Field scaleField;
            [NonSerialized] public VisualElement container;
        }

        public List<FighterEntry> Fighters { get; set; } = new();
        public float PreviewScale { get; set; } = 1.5f;

        // UI
        private ObjectField _minigameField;
        private ObjectField _sceneField;
        private DropdownField _parallaxLayerDropdown;
        private Vector2Field _offsetField;
        private VisualElement _fightersContainer;
        private Slider _previewScaleSlider;
        private Image _previewImage;

        private readonly List<string> _parallaxLayers = new();
        private const int BasePreviewHeight = 180;

        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);

            NodeName = "Combat Minigame";
            Text = string.Empty;
            Choices.Clear();
            Choices.Add("onSuccess");
            Choices.Add("onFail");
        }

        public override void Draw()
        {
            // ----- Title -----
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);
            titleContainer.Insert(0, nodeNameTextField);
            titleContainer.Q<TextField>()?.SetEnabled(false);

            // ----- Ports -----
            var inputPort = this.CreatePort("Input Link", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            foreach (var choice in Choices)
            {
                var choicePort = this.CreatePort(choice, capacity: Port.Capacity.Multi);
                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }

            // ----- Body -----
            _minigameField = new ObjectField("Minigame Prefab")
            {
                objectType = typeof(CombatMinigame),
                allowSceneObjects = false,
                value = MinigamePrefab
            };
            _minigameField.RegisterValueChangedCallback(evt => MinigamePrefab = evt.newValue as CombatMinigame);
            extensionContainer.Add(_minigameField);

            _sceneField = new ObjectField("Scene Prefab")
            {
                objectType = typeof(SceneController),
                allowSceneObjects = false,
                value = Scene
            };
            _sceneField.RegisterValueChangedCallback(OnSceneChanged);
            extensionContainer.Add(_sceneField);

            _parallaxLayerDropdown = new DropdownField("Parallax Layer");
            _parallaxLayerDropdown.RegisterValueChangedCallback(evt => SelectedParallaxLayer = evt.newValue);
            extensionContainer.Add(_parallaxLayerDropdown);

            _offsetField = new Vector2Field("Character Offset") { value = CharacterOffset };
            _offsetField.RegisterValueChangedCallback(evt =>
            {
                CharacterOffset = evt.newValue;
                RebuildPreview();
            });
            extensionContainer.Add(_offsetField);

            _fightersContainer = new VisualElement();
            extensionContainer.Add(_fightersContainer);

            var addButton = UIElementUtilities.CreateButton("Add Fighter", () =>
            {
                var entry = new FighterEntry();
                Fighters.Add(entry);
                CreateFighterUI(entry);
                RebuildPreview();
            });
            addButton.AddToClassList("gp-node__button");
            extensionContainer.Add(addButton);

            _previewScaleSlider = new Slider("Preview Scale", 0.5f, 2f)
            {
                value = PreviewScale
            };
            _previewScaleSlider.RegisterValueChangedCallback(evt =>
            {
                PreviewScale = evt.newValue;
                RebuildPreview();
            });
            extensionContainer.Add(_previewScaleSlider);

            _previewImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            _previewImage.style.marginTop = 6;
            _previewImage.style.marginBottom = 6;
            _previewImage.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

            RefreshParallaxLayers();
            foreach (var f in Fighters.ToList())
                CreateFighterUI(f);
            RebuildPreview();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void OnSceneChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Scene = evt.newValue as SceneController;
            RefreshParallaxLayers();
            RebuildPreview();
        }

        private void RefreshParallaxLayers()
        {
            _parallaxLayers.Clear();
            if (Scene != null)
            {
                var parallax = Scene.GetComponent<ParallaxController>();
                if (parallax?.layers != null)
                {
                    foreach (var layer in parallax.layers)
                    {
                        if (layer != null)
                            _parallaxLayers.Add(layer.name);
                    }
                }
            }

            _parallaxLayerDropdown.choices = _parallaxLayers;
            if (!string.IsNullOrEmpty(SelectedParallaxLayer) && _parallaxLayers.Contains(SelectedParallaxLayer))
                _parallaxLayerDropdown.SetValueWithoutNotify(SelectedParallaxLayer);
            else
                _parallaxLayerDropdown.SetValueWithoutNotify(_parallaxLayers.FirstOrDefault() ?? string.Empty);
            SelectedParallaxLayer = _parallaxLayerDropdown.value;
        }

        private void CreateFighterUI(FighterEntry entry)
        {
            var container = new VisualElement();
            entry.container = container;

            var header = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var removeButton = UIElementUtilities.CreateButton("X", () =>
            {
                Fighters.Remove(entry);
                container.RemoveFromHierarchy();
                RebuildPreview();
                RefreshExpandedState();
            });
            removeButton.AddToClassList("gp-node__button");
            header.Add(removeButton);
            container.Add(header);

            entry.characterField = new ObjectField("Character")
            {
                objectType = typeof(GameAssets.ScriptableObjects.Core.CharacterSO),
                allowSceneObjects = false,
                value = entry.characterReference
            };
            entry.characterField.RegisterValueChangedCallback(evt =>
            {
                entry.characterReference = evt.newValue as GameAssets.ScriptableObjects.Core.CharacterSO;
                entry.SelectedEmotion = string.Empty;
                RefreshEmotionDropdown(entry);
                RebuildPreview();
            });
            container.Add(entry.characterField);

            entry.emotionDropdown = new DropdownField("Emotion") { style = { flexGrow = 1 } };
            entry.emotionDropdown.RegisterValueChangedCallback(evt =>
            {
                entry.SelectedEmotion = evt.newValue;
                if (entry.characterReference != null)
                {
                    var emo = entry.characterReference.characterEmotionSpriteSheet.FirstOrDefault(e => e != null && e.spriteName == evt.newValue);
                    entry.characterEmotion = emo;
                }
                RebuildPreview();
            });
            container.Add(entry.emotionDropdown);

            entry.healthField = new IntegerField("Health") { value = entry.baseHealthPoints };
            entry.healthField.RegisterValueChangedCallback(evt => entry.baseHealthPoints = evt.newValue);
            container.Add(entry.healthField);

            entry.actionField = new IntegerField("Action Points") { value = entry.baseActionPoints };
            entry.actionField.RegisterValueChangedCallback(evt => entry.baseActionPoints = evt.newValue);
            container.Add(entry.actionField);

            entry.damageField = new IntegerField("Damage") { value = entry.baseDamage };
            entry.damageField.RegisterValueChangedCallback(evt => entry.baseDamage = evt.newValue);
            container.Add(entry.damageField);

            entry.layerField = new IntegerField("Layer") { value = entry.layer };
            entry.layerField.RegisterValueChangedCallback(evt =>
            {
                entry.layer = evt.newValue;
                RebuildPreview();
            });
            container.Add(entry.layerField);

            entry.scaleField = new Vector2Field("Scale") { value = entry.characterScale };
            entry.scaleField.RegisterValueChangedCallback(evt =>
            {
                entry.characterScale = evt.newValue;
                RebuildPreview();
            });
            container.Add(entry.scaleField);

            _fightersContainer.Add(container);

            RefreshEmotionDropdown(entry);
        }

        private void RefreshEmotionDropdown(FighterEntry entry)
        {
            var options = new List<string>();
            if (entry.characterReference != null && entry.characterReference.characterEmotionSpriteSheet != null)
            {
                options.AddRange(entry.characterReference.characterEmotionSpriteSheet.Select(e => e?.spriteName).Where(n => !string.IsNullOrEmpty(n)));
            }

            entry.emotionDropdown.choices = options;
            if (!string.IsNullOrEmpty(entry.SelectedEmotion) && options.Contains(entry.SelectedEmotion))
                entry.emotionDropdown.SetValueWithoutNotify(entry.SelectedEmotion);
            else
                entry.emotionDropdown.SetValueWithoutNotify(options.FirstOrDefault() ?? string.Empty);
            entry.SelectedEmotion = entry.emotionDropdown.value;
        }

        private void RebuildPreview()
        {
            _previewImage.RemoveFromHierarchy();

            if (Scene == null || Fighters.Count == 0)
            {
                _previewScaleSlider.SetEnabled(false);
                return;
            }

            var first = Fighters[0];
            if (first.characterReference == null || string.IsNullOrEmpty(first.SelectedEmotion))
            {
                _previewScaleSlider.SetEnabled(false);
                return;
            }

            var sprite = first.characterReference.GetEmotionSpriteByName(first.SelectedEmotion);
            if (sprite == null)
            {
                _previewScaleSlider.SetEnabled(false);
                return;
            }

            _previewScaleSlider.SetEnabled(true);
            var targetHeight = Mathf.RoundToInt(BasePreviewHeight * PreviewScale);

            var scale = first.characterScale != Vector2.zero ? first.characterScale : Vector2.one;
            var info = new CharacterPreviewInfo
            {
                sprite = sprite,
                position = CharacterOffset,
                layer = first.layer,
                color = Color.white,
                scale = scale
            };

            var tex = RenderCharacterPreview(Scene, new List<CharacterPreviewInfo> { info }, targetHeight);
            _previewImage.image = tex;
            _previewImage.style.width = tex != null ? tex.width : 0;
            _previewImage.style.height = tex != null ? tex.height : 0;
            _previewImage.MarkDirtyRepaint();

            extensionContainer.Add(_previewImage);
        }

        private struct CharacterPreviewInfo
        {
            public Sprite sprite;
            public Vector2 position;
            public int layer;
            public Color color;
            public Vector2 scale;
        }

        private static Texture2D RenderCharacterPreview(SceneController scenePrefab, List<CharacterPreviewInfo> characters, int height)
        {
            if (scenePrefab == null)
                return null;

            var previewScene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
            UnityEditor.SceneManagement.EditorSceneManager.SetSceneCullingMask(previewScene, ulong.MaxValue);

            Texture2D tex = null;
            RenderTexture rt = null;
            Camera cam = null;

            try
            {
                var sceneInstance = (GameObject)PrefabUtility.InstantiatePrefab(scenePrefab.gameObject, previewScene);
                sceneInstance.transform.position = Vector3.zero;

                var srList = new List<SpriteRenderer>();
                foreach (var info in characters)
                {
                    var go = new GameObject("CharacterPreview");
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = info.sprite;
                    sr.color = info.color;
                    sr.sortingOrder = info.layer;
                    go.transform.position = new Vector3(info.position.x, info.position.y, 0f);
                    go.transform.localScale = new Vector3(info.scale.x, info.scale.y, 1f);
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, previewScene);
                    srList.Add(sr);
                }

                Bounds? b = null;
                var focusAreas = sceneInstance.GetComponentsInChildren<PreviewFocusArea>(true);
                if (focusAreas != null && focusAreas.Length > 0)
                {
                    foreach (var f in focusAreas)
                    {
                        var fb = f.GetWorldBounds();
                        b = b.HasValue ? Encapsulate(b.Value, fb) : fb;
                    }
                }
                else
                {
                    var rens = sceneInstance.GetComponentsInChildren<Renderer>(true)
                        .Where(r => r.enabled && r.gameObject.activeInHierarchy)
                        .ToList();
                    rens.AddRange(srList);
                    if (rens.Count > 0)
                    {
                        var rb = rens[0].bounds;
                        for (int i = 1; i < rens.Count; i++) rb.Encapsulate(rens[i].bounds);
                        b = rb;
                    }
                    else
                    {
                        var rects = sceneInstance.GetComponentsInChildren<RectTransform>(true);
                        if (rects.Length > 0)
                        {
                            var corners = new Vector3[4];
                            Bounds rb = new Bounds();
                            bool init = false;
                            foreach (var rtRect in rects)
                            {
                                rtRect.GetWorldCorners(corners);
                                if (!init)
                                {
                                    rb = new Bounds(corners[0], Vector3.zero);
                                    init = true;
                                }
                                for (int i = 0; i < 4; i++) rb.Encapsulate(corners[i]);
                            }
                            b = rb;
                        }
                    }
                }

                if (!b.HasValue)
                {
                    tex = new Texture2D(height, height, TextureFormat.RGBA32, false);
                    var fill = Enumerable.Repeat(new Color(0.16f, 0.16f, 0.16f, 1f), height * height).ToArray();
                    tex.SetPixels(fill);
                    tex.Apply(false, false);
                    return tex;
                }

                var bounds = b.Value;
                var aspect = bounds.size.x / Mathf.Max(bounds.size.y, 0.0001f);
                var width = Mathf.Max(1, Mathf.RoundToInt(height * aspect));

                var camGO = new GameObject("PreviewCamera");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camGO, previewScene);
                cam = camGO.AddComponent<Camera>();
                cam.cullingMask = -1;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);
                cam.orthographic = true;
                cam.orthographicSize = Mathf.Max(bounds.extents.y, 0.01f);

                var center = bounds.center;
                cam.transform.position = new Vector3(center.x, center.y, center.z - 10f);
                cam.transform.rotation = Quaternion.identity;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 1000f;

                rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;
                cam.aspect = (float)width / height;
                cam.Render();

                var prev = RenderTexture.active;
                RenderTexture.active = rt;

                tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply(false, false);

                RenderTexture.active = prev;
            }
            finally
            {
                if (rt) RenderTexture.ReleaseTemporary(rt);
                if (cam) UnityEngine.Object.DestroyImmediate(cam.gameObject);
                UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(previewScene);
            }

            return tex;
        }

        private static Bounds Encapsulate(Bounds a, Bounds b)
        {
            a.Encapsulate(b.min);
            a.Encapsulate(b.max);
            return a;
        }
    }
}


