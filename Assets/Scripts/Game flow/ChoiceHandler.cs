using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VisualNovel.Decisions;

namespace VisualNovel.GameFlow
{
    /// <summary>
    /// Controls the group of decision options and coordinates them with the
    /// DecisionCircle. Provides an easy API for enabling/disabling options,
    /// changing their text and assigning callbacks.
    /// </summary>
    public class ChoiceHandler : MonoBehaviour, IInitializeOnAwake
    {
        [SerializeField] private DecisionCircle circle;
        [SerializeField] private DecisionOption[] options = new DecisionOption[4];
        private readonly List<ChoiceData> currentChoices = new List<ChoiceData>();

        public static ChoiceHandler Instance { get; private set; }

        /// <summary>Represents data for a single choice entry.</summary>
        public struct ChoiceData
        {
            public string text;
            public UnityAction action;

            public ChoiceData(string text, UnityAction action)
            {
                this.text = text;
                this.action = action;
            }
        }

        public void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;
            
            foreach (var opt in options)
            {
                if (opt == null) continue;
                opt.gameObject.SetActive(false);
                opt.Hovered += HandleHover;
                opt.Exited += HandleExit;
                opt.Clicked += HandleClick;
            }

            circle?.Hide();
        }

        /// <summary>Adds a new choice to the internal list.</summary>
        public void AddChoice(string text, UnityAction action)
        {
            currentChoices.Add(new ChoiceData(text, action));
        }

        /// <summary>Displays all collected choices on the predefined option slots.</summary>
        public void ShowChoices()
        {
            ShowChoices(currentChoices);
        }

        /// <summary>Displays the given choices on the predefined option slots.</summary>
        public void ShowChoices(IList<ChoiceData> choices)
        {
            int count = choices.Count;
            for (int i = 0; i < options.Length; i++)
            {
                if (i < count)
                {
                    var data = choices[i];
                    options[i].Setup(data.text, data.action);
                }
                else
                {
                    options[i].gameObject.SetActive(false);
                }
            }

            circle?.Show();
        }

        /// <summary>Hides all options and the decision circle.</summary>
        public void ClearChoices()
        {
            
            foreach (var opt in options)
            {
                if (opt == null) continue;
                opt.gameObject.SetActive(false);
            }
            currentChoices.Clear();
            circle?.Hide();
        }

        private void HandleHover(DecisionOption opt)
        {
            if (circle != null)
                circle.PointAt(opt.AnchoredPosition);
        }

        private void HandleExit(DecisionOption opt)
        {
            if (circle != null)
                circle.Hide();
        }

        private void HandleClick(DecisionOption opt)
        {
            if (circle != null)
                circle.Hide();
        }
    }
}