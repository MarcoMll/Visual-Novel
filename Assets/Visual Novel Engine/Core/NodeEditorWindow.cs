using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Core
{
    using Utilities;
    using Data.Utilities;
    
    public class NodeEditorWindow : EditorWindow
    {
        private const string DefaultFileName = "EmptyRoot";
        private Button _saveButton;
        private NodeGraphView _graphView;
        private TextField _fileNameTextField;
        private string _currentAssetPath;

        public string CurrentAssetPath => _currentAssetPath;

        [MenuItem("Visual Novel Engine/Editor Graph")]
        public static void Open()
        {
            GetWindow<NodeEditorWindow>("Editor Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolBar();
            LoadStyles();
        }

        private void LoadStyles()
        {
            rootVisualElement.AddStyleSheets(EditorConstants.VariablesStyleSheet);
        }

        private void AddToolBar()
        {
            var toolbar = new Toolbar();

            _fileNameTextField = UIElementUtilities.CreateTextField(DefaultFileName, "File name:");
            _fileNameTextField.SetEnabled(true);

            _saveButton = UIElementUtilities.CreateButton("Save");
            var loadButton = UIElementUtilities.CreateButton("Load");

            _saveButton.clicked += () =>
            {
                if (string.IsNullOrEmpty(_currentAssetPath))
                {
                    var path = EditorUtility.SaveFilePanelInProject(
                        "Save graph",
                        _fileNameTextField.value,
                        "asset",
                        "Select location to save the graph");
                    if (string.IsNullOrEmpty(path)) return;
                    GraphSaveUtility.SaveGraph(_graphView, path);
                    _currentAssetPath = path;
                    _fileNameTextField.value = Path.GetFileNameWithoutExtension(path);
                    _fileNameTextField.SetEnabled(false);
                }
                else
                {
                    GraphSaveUtility.SaveGraph(_graphView, _currentAssetPath);
                }
            };

            loadButton.clicked += () =>
            {
                var path = EditorUtility.OpenFilePanel("Load graph", Application.dataPath, "asset");
                if (string.IsNullOrEmpty(path)) return;
                path = path.Replace(Application.dataPath, "Assets");
                GraphSaveUtility.LoadGraph(_graphView, path);
                _currentAssetPath = path;
                _fileNameTextField.value = Path.GetFileNameWithoutExtension(path);
                _fileNameTextField.SetEnabled(false);
            };

            toolbar.Add(_fileNameTextField);
            toolbar.Add(_saveButton);
            toolbar.Add(loadButton);

            toolbar.AddStyleSheets(EditorConstants.ToolbarStyleSheet);

            rootVisualElement.Add(toolbar);
        }

        // to be implemented later
        public void ToggleSaving(bool value)
        {
            _saveButton.SetEnabled(value);
        }
        
        private void AddGraphView()
        {
            _graphView = new NodeGraphView(this);

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }
    }
}