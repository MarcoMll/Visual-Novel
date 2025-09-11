using UnityEngine;
using VisualNovel.GameFlow;

namespace VisualNovel.UI
{
    public class UserInterfaceManager : MonoBehaviour, IInitializeOnAwake
    {
        [SerializeField] private Canvas gameCanvas;
        public static UserInterfaceManager Instance { get; private set; }
        
        public void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;
        }
    }
}