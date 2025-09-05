using System;
using UnityEngine;

namespace VisualNovel.Interaction
{
    public class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private AudioSource sfxPlayer;
        private IInteractable _selectedInteractable;

        public static PlayerInteractionController Instance { private set; get; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_selectedInteractable == null) return;
                _selectedInteractable.Interact(this);
            }
        }

        public void SelectInteractable(IInteractable interactable)
        {
            _selectedInteractable = interactable;
        }

        public void ResetInteractable(IInteractable interactable)
        {
            if (_selectedInteractable == interactable)
                _selectedInteractable = null;
        }

        public void PlaySoundEffect(AudioClip audioClip)
        {
            if (sfxPlayer == null || audioClip == null) return;

            sfxPlayer.PlayOneShot(audioClip);
        }
    }
}
