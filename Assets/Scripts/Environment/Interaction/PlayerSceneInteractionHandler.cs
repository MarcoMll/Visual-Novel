using UnityEngine;

namespace VisualNovel.Environment.Interaction
{
    using Audio;
    
    public class PlayerSceneInteractionHandler : MonoBehaviour
    {
        private ISceneInteractable _selectedSceneInteractable;

        public static PlayerSceneInteractionHandler Instance { private set; get; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_selectedSceneInteractable == null) return;
                _selectedSceneInteractable.Interact(this);
            }
        }

        public void SelectInteractable(ISceneInteractable sceneInteractable)
        {
            _selectedSceneInteractable = sceneInteractable;
        }

        public void ResetInteractable(ISceneInteractable sceneInteractable)
        {
            if (_selectedSceneInteractable == sceneInteractable)
                _selectedSceneInteractable = null;
        }

        public void PlaySoundEffect(AudioClip audioClip)
        {
            if (audioClip == null) return;

            AudioHandler.Instance.PlaySfx(audioClip);
        }
    }
}
