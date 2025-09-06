using UnityEngine;
using Febucci.UI.Core;
using TMPro;

namespace VisualNovel.UI
{
    using GameFlow;
    
    public class UIDialogueTextController : MonoBehaviour, IInitializeOnAwake
    {
        [SerializeField] private TMP_Text characterNameField;
        [SerializeField] private TypewriterCore typewriter;
        public static UIDialogueTextController Instance { get; private set; }
        private bool _allComponentsAssigned = true;
        
        public void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;
            
            if (characterNameField == null)
            {
                Debug.LogError("Character name field is null!");
                _allComponentsAssigned = false;
            }

            if (typewriter == null)
            {
                Debug.LogError("Text typewriter is null!");
                _allComponentsAssigned = false;
            }
        }
        
        public void PlayText(string text, string characterName = "")
        {
            if (_allComponentsAssigned == false)
            {
                Debug.LogError("Text can't be shown as some of the components are missing!");
                return;
            }
            
            characterNameField.text = characterName;
            typewriter.ShowText(text);
        }
    }
}