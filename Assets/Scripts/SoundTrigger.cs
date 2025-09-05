using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class SoundTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip audioClipToPlay;
    [SerializeField] private bool random;
    [SerializeField] private AudioClip[] audioClipsToPlay;
    
    // This flag tracks whether the cursor is currently over the sprite
    private bool isHovering = false;

    // Called once when the mouse cursor enters the Collider2D zone
    private void OnMouseEnter()
    {
        isHovering = true;
        DoSomething();
    }

    // Called once when the mouse cursor exits the Collider2D zone
    private void OnMouseExit()
    {
        isHovering = false;
        UIManager.Instance.ToggleSoundPanel(false);
        PlayerInteractionController.Instance.ResetInteractable(this);
    }

    // This is called every frame while the mouse stays over the Collider2D
    private void OnMouseOver()
    {
        // If you need to do something continuously while hovering,
        // you could put it here. For a one‐time action, OnMouseEnter() is enough.
        // Example: continuously change color while hovering
        // GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    // Your “hover action” goes here. Called once when entering
    private void DoSomething()
    {
        UIManager.Instance.ToggleSoundPanel(true);
        PlayerInteractionController.Instance.SelectInteractable(this);
    }

    public void Interact(PlayerInteractionController interactionController)
    {
        if (random && audioClipsToPlay.Length > 0)
        {
            interactionController.PlaySoundEffect(audioClipsToPlay[Random.Range(0, audioClipsToPlay.Length)]); 
        }
        else
        {
            interactionController.PlaySoundEffect(audioClipToPlay);
        }
    }
}
