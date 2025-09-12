using UnityEngine;

namespace VisualNovel.UI.Notifications
{
    public abstract class GlobalNotification : MonoBehaviour
    {
        public abstract void Show();
        public abstract void Hide();
    }
}