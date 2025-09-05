using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace VisualNovel.Decisions
{
    [RequireComponent(typeof(TMP_Text))]
    public class DecisionOption : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [Header("Styling")]
        public Color idleColor    = Color.white;
        public Color hoverColor   = Color.yellow;
        public Vector2 hoverOffset = new Vector2(0, 10);

        [Header("Smoothing")]
        public float textMoveSpeed = 8f;

        private TMP_Text      _text;
        private RectTransform _rt;
        private DecisionCircle _manager;
        private Vector2 _initialPos;
        private Vector2 _targetPos;

        void Awake()
        {
            _text       = GetComponent<TMP_Text>();
            _rt         = GetComponent<RectTransform>();
            _initialPos = _rt.anchoredPosition;
            _targetPos  = _initialPos;

            _manager = FindObjectOfType<DecisionCircle>();
            if (_manager == null)
                Debug.LogError("DecisionCircle not found in scene!");
        }

        void Update()
        {
            _rt.anchoredPosition = Vector2.Lerp(
                _rt.anchoredPosition,
                _targetPos,
                textMoveSpeed * Time.deltaTime
            );
        }

        public void OnPointerEnter(PointerEventData e)
        {
            _manager.SelectOption(this);
        }

        public void OnPointerClick(PointerEventData e)
        {
            _manager.ConfirmOption(this);
        }

        public void Select()
        {
            _text.color = hoverColor;
            _targetPos  = _initialPos + hoverOffset;
        }

        public void Deselect()
        {
            _text.color = idleColor;
            _targetPos  = _initialPos;
        }

        public Vector2 GetCurrentAnchoredPosition()
        {
            return _initialPos + hoverOffset;
        }
    }
}
