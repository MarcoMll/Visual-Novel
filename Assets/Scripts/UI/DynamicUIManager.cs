using System;
using UnityEngine;

namespace VisualNovel.UI
{
    public class DynamicUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject soundPanel;
        [SerializeField] private Vector3 panelOffset;

        private bool _updateSoundPanelPosition;

        public static DynamicUIManager Instance { private set; get; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ToggleSoundPanel(false);
        }

        private void Update()
        {
            if (_updateSoundPanelPosition == true)
                soundPanel.transform.position = Input.mousePosition + panelOffset;
        }

        public void ToggleSoundPanel(bool active)
        {
            soundPanel.SetActive(active);
            _updateSoundPanelPosition = active;
        }
    }
}
