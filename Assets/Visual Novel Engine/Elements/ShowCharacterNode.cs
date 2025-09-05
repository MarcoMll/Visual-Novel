using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using VisualNovel.Environment;

namespace VisualNovelEngine.Elements
{
    using GameAssets.ScriptableObjects.Core;
    using Utilities;
    using Utilities.Attributes;

    [Serializable]
    [NodeMenu("Show Character Node", "Scenery")]
    public class ShowCharacterNode : GraphNode
    {
        // Data
        public SceneController ScenePrefab { get; set; }
        public SceneController Scene { get => ScenePrefab; set => ScenePrefab = value; }
        public List<CharacterEntry> Characters { get; set; } = new();
        public float PreviewScale { get; set; } = 1.5f;

        [System.Serializable]
        public class CharacterEntry
        {
            public CharacterSO Character;
            public string SelectedEmotion;
            public string SelectedPositionName;
            public Vector2 Offset;
            public int Layer;
            public Color SpriteColor = Color.white;
            public Vector2 CharacterScale;

            [System.NonSerialized] public ObjectField characterField;
            [System.NonSerialized] public DropdownField emotionDropdown;
            [System.NonSerialized] public DropdownField positionDropdown;
            [System.NonSerialized] public Vector2Field offsetField;
            [System.NonSerialized] public IntegerField layerField;
            [System.NonSerialized] public ColorField colorField;
            [System.NonSerialized] public Vector2Field scaleField;
            [System.NonSerialized] public Foldout optionsFoldout;
            [System.NonSerialized] public VisualElement container;
        }

        // UI
        private ObjectField _sceneField;
        private VisualElement _charactersContainer;
        private Image _previewImage;
        private Slider _previewScaleSlider;

        private const int BasePreviewHeight = 180;

        // Position lookup
        private readonly Dictionary<string, PositionData> _positions = new();

        private class PositionData
        {
            public Vector2 position;
            public int layer;
            public Vector2 scale;
        }

        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);
            NodeName = "Show Character";
            Text = string.Empty;
            Choices.Clear();
        }

        public override void Draw()
        {
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);
            titleContainer.Insert(0, nodeNameTextField);
            titleContainer.Q<TextField>()?.SetEnabled(false);

            SetTitleIcon(GUIUtilities.GetIconByName("scenery_icon"));

            var inputPort = this.CreatePort("Input Link", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            _sceneField = new ObjectField("Scene Prefab")
            {
                objectType = typeof(SceneController),
                allowSceneObjects = false,
                value = Scene
            };
            _sceneField.RegisterValueChangedCallback(OnSceneChanged);
            extensionContainer.Add(_sceneField);

            _charactersContainer = new VisualElement();
            extensionContainer.Add(_charactersContainer);

            var addButton = UIElementUtilities.CreateButton("Add Character", () =>
            {
                var entry = new CharacterEntry();
                Characters.Add(entry);
                CreateCharacterUI(entry);
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

            RefreshPositions();
            foreach (var entry in Characters.ToList())
                CreateCharacterUI(entry);
            RebuildPreview();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void OnSceneChanged(ChangeEvent<Object> evt)
        {
            Scene = evt.newValue as SceneController;
            RefreshPositions();
            foreach (var entry in Characters)
                SetPositionChoices(entry);
            RebuildPreview();
        }

        private void CreateCharacterUI(CharacterEntry entry)
        {
            var container = new VisualElement();
            entry.container = container;

            var header = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var removeButton = UIElementUtilities.CreateButton("X", () =>
            {
                Characters.Remove(entry);
                container.RemoveFromHierarchy();
                RebuildPreview();
                RefreshExpandedState();
            });
            removeButton.AddToClassList("gp-node__button");
            header.Add(removeButton);
            container.Add(header);

            entry.characterField = new ObjectField("Character")
            {
                objectType = typeof(CharacterSO),
                allowSceneObjects = false,
                value = entry.Character
            };
            entry.characterField.RegisterValueChangedCallback(evt =>
            {
                entry.Character = evt.newValue as CharacterSO;
                entry.SelectedEmotion = string.Empty;
                RefreshEmotionDropdown(entry);
                RebuildPreview();
            });
            container.Add(entry.characterField);

            entry.emotionDropdown = new DropdownField("Emotion") { style = { flexGrow = 1 } };
            entry.emotionDropdown.RegisterValueChangedCallback(evt =>
            {
                entry.SelectedEmotion = evt.newValue;
                RebuildPreview();
            });
            container.Add(entry.emotionDropdown);

            entry.positionDropdown = new DropdownField("Position") { style = { flexGrow = 1 } };
            entry.positionDropdown.RegisterValueChangedCallback(evt =>
            {
                entry.SelectedPositionName = evt.newValue;
                UpdateFromPosition(entry);
                RebuildPreview();
            });
            container.Add(entry.positionDropdown);

            entry.optionsFoldout = UIElementUtilities.CreateFoldout("Options", false);

            entry.offsetField = new Vector2Field("Offset") { value = entry.Offset };
            entry.offsetField.RegisterValueChangedCallback(evt =>
            {
                entry.Offset = evt.newValue;
                RebuildPreview();
            });
            entry.optionsFoldout.Add(entry.offsetField);

            entry.layerField = new IntegerField("Layer") { value = entry.Layer };
            entry.layerField.RegisterValueChangedCallback(evt =>
            {
                entry.Layer = evt.newValue;
                RebuildPreview();
            });
            entry.optionsFoldout.Add(entry.layerField);

            entry.colorField = new ColorField("Color") { value = entry.SpriteColor };
            entry.colorField.RegisterValueChangedCallback(evt =>
            {
                entry.SpriteColor = evt.newValue;
                RebuildPreview();
            });
            entry.optionsFoldout.Add(entry.colorField);

            entry.scaleField = new Vector2Field("Scale") { value = entry.CharacterScale };
            entry.scaleField.RegisterValueChangedCallback(evt =>
            {
                entry.CharacterScale = evt.newValue;
                RebuildPreview();
            });
            entry.optionsFoldout.Add(entry.scaleField);

            container.Add(entry.optionsFoldout);

            _charactersContainer.Add(container);

            RefreshEmotionDropdown(entry);
            SetPositionChoices(entry);
        }

        private void RefreshEmotionDropdown(CharacterEntry entry)
        {
            var options = new List<string>();
            if (entry.Character?.characterEmotionSpriteSheet != null)
            {
                options = entry.Character.characterEmotionSpriteSheet
                    .Where(e => e != null && !string.IsNullOrEmpty(e.spriteName))
                    .Select(e => e.spriteName)
                    .ToList();
            }

            entry.emotionDropdown.choices = options;
            if (!string.IsNullOrEmpty(entry.SelectedEmotion) && options.Contains(entry.SelectedEmotion))
                entry.emotionDropdown.SetValueWithoutNotify(entry.SelectedEmotion);
            else
                entry.emotionDropdown.SetValueWithoutNotify(options.FirstOrDefault() ?? string.Empty);
            entry.SelectedEmotion = entry.emotionDropdown.value;
        }

        private void RefreshPositions()
        {
            _positions.Clear();
            foreach (var pos in LoadCharacterPositions(Scene))
                _positions[pos.name] = new PositionData { position = pos.position, layer = pos.layer, scale = pos.scale };
        }

        private void SetPositionChoices(CharacterEntry entry)
        {
            var options = _positions.Keys.ToList();
            entry.positionDropdown.choices = options;

            if (!string.IsNullOrEmpty(entry.SelectedPositionName) && options.Contains(entry.SelectedPositionName))
            {
                entry.positionDropdown.SetValueWithoutNotify(entry.SelectedPositionName);
                entry.SelectedPositionName = entry.positionDropdown.value;
                // Preserve existing layer and scale when reloading
            }
            else
            {
                entry.positionDropdown.SetValueWithoutNotify(options.FirstOrDefault() ?? string.Empty);
                entry.SelectedPositionName = entry.positionDropdown.value;
                UpdateFromPosition(entry);
            }
        }

        private void UpdateFromPosition(CharacterEntry entry)
        {
            if (_positions.TryGetValue(entry.SelectedPositionName, out var pd))
            {
                entry.Layer = pd.layer;
                entry.layerField?.SetValueWithoutNotify(entry.Layer);
                entry.CharacterScale = pd.scale;
                entry.scaleField?.SetValueWithoutNotify(entry.CharacterScale);
            }
        }
        
        private void RebuildPreview()
        {
            _previewImage.RemoveFromHierarchy();

            if (Scene == null)
            {
                _previewScaleSlider.SetEnabled(false);
                return;
            }

            var infos = new List<CharacterPreviewInfo>();
            foreach (var entry in Characters)
            {
                var sprite = GetSelectedSprite(entry);
                if (sprite == null) continue;
                if (!_positions.TryGetValue(entry.SelectedPositionName, out var pd)) continue;
                var scale = entry.CharacterScale != Vector2.zero ? entry.CharacterScale : pd.scale;
                infos.Add(new CharacterPreviewInfo
                {
                    sprite = sprite,
                    position = pd.position + entry.Offset,
                    layer = entry.Layer,
                    color = entry.SpriteColor,
                    scale = scale
                });
            }

            if (infos.Count == 0)
            {
                _previewScaleSlider.SetEnabled(false);
                return;
            }

            _previewScaleSlider.SetEnabled(true);
            var targetHeight = Mathf.RoundToInt(BasePreviewHeight * PreviewScale);
            var tex = RenderCharacterPreview(Scene, infos, targetHeight);

            _previewImage.image = tex;
            _previewImage.style.width = tex != null ? tex.width : 0;
            _previewImage.style.height = tex != null ? tex.height : 0;
            _previewImage.MarkDirtyRepaint();

            extensionContainer.Add(_previewImage);
        }

        private Sprite GetSelectedSprite(CharacterEntry entry)
        {
            if (entry.Character?.characterEmotionSpriteSheet == null)
                return null;

            foreach (var emo in entry.Character.characterEmotionSpriteSheet)
            {
                if (emo != null && emo.spriteName == entry.SelectedEmotion)
                    return emo.sprite;
            }
            return null;
        }

        private struct PositionInfo
        {
            public string name;
            public Vector2 position;
            public int layer;
            public Vector2 scale;
        }

        private static IEnumerable<PositionInfo> LoadCharacterPositions(SceneController controller)
        {
            var list = new List<PositionInfo>();
            if (controller == null)
                return list;

            var field = typeof(SceneController).GetField("characterPositions", BindingFlags.NonPublic | BindingFlags.Instance);
            var array = field?.GetValue(controller) as System.Array;
            if (array == null)
                return list;

            foreach (var item in array)
            {
                var type = item.GetType();
                var nameField = type.GetField("positionName");
                var posField = type.GetField("position");
                var layerField = type.GetField("layer");
                var scaleField = type.GetField("defaultCharacterScale");
                if (nameField == null || posField == null || layerField == null || scaleField == null) continue;

                var info = new PositionInfo
                {
                    name = nameField.GetValue(item) as string,
                    position = (Vector2)posField.GetValue(item),
                    layer = (int)layerField.GetValue(item),
                    scale = (Vector2)scaleField.GetValue(item)
                };
                if (!string.IsNullOrEmpty(info.name))
                    list.Add(info);
            }

            return list;
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
                    return null;

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
                if (cam) Object.DestroyImmediate(cam.gameObject);
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