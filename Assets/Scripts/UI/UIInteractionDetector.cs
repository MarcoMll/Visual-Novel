using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VisualNovel.UI
{
    public class UIInteractionDetector : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UnityEvent<int> onClick;
        [SerializeField] private UnityEvent onPointerEnter;
        [SerializeField] private UnityEvent onPointerExit;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke((int)eventData.button);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke();
        }
    }
}
