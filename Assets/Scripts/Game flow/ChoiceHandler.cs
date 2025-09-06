using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualNovel.GameFlow
{
    public class ChoiceHandler : MonoBehaviour, IInitializeOnAwake
    {
        private List<ChoiceData> _choices;
        public static ChoiceHandler Instance { get; private set; }
        
        public class ChoiceData
        {
            public string choiceText { get; set; }
            public Action choiceAction { get; set; }
        }

        public void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;
            
            _choices = new List<ChoiceData>();
        }
        
        public void ClearChoices()
        {
            _choices.Clear();
        }

        public void AddChoice(string choiceText, Action choiceAction)
        {
            
        }

        public void RemoveChoice(ChoiceData choice)
        {
            
        }
    }
}