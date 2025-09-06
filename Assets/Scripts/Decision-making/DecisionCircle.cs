using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualNovel.Decisions
{
    /// <summary>
    /// Purely visual component that handles the arrow indicator that rotates
    /// around the decision circle and an optional timer. Logic for handling
    /// choices is delegated elsewhere.
    /// </summary>
    public class DecisionCircle : MonoBehaviour
    {
        [Header("Arrow Settings")]
        public RectTransform arrowRect;
        public float arrowDistance = 100f;
        public float rotationOffset = 0f;

        [Header("Smoothing")]
        public float arrowMoveSpeed = 8f;

        [Header("Timer (optional)")]
        public SimpleTimer timer;

        private float _currentCircleAngle, _targetCircleAngle;
        private float _currentGraphicAngle, _targetGraphicAngle;

        private void Start()
        {
            if (arrowRect != null)
            {
                arrowRect.gameObject.SetActive(false);

                // angles from initial arrow position
                Vector2 initPos = arrowRect.anchoredPosition;
                float initAngle = Mathf.Atan2(initPos.y, initPos.x) * Mathf.Rad2Deg;
                _currentCircleAngle = _targetCircleAngle = initAngle;
                _currentGraphicAngle = _targetGraphicAngle = initAngle + rotationOffset;
            }

            if (timer != null)
            {
                timer.OnTimerComplete += HandleTimeout;
                timer.StartTimer();
            }
        }

        private void Update()
        {
            if (arrowRect == null || !arrowRect.gameObject.activeSelf)
                return;

            // lerp our circle-angle
            _currentCircleAngle = Mathf.LerpAngle(
                _currentCircleAngle,
                _targetCircleAngle,
                arrowMoveSpeed * Time.deltaTime
            );

            // lerp our graphic-angle
            _currentGraphicAngle = Mathf.LerpAngle(
                _currentGraphicAngle,
                _targetGraphicAngle,
                arrowMoveSpeed * Time.deltaTime
            );

            // apply rotation
            arrowRect.localEulerAngles = Vector3.forward * _currentGraphicAngle;

            // compute position on circle
            float rad = _currentCircleAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            arrowRect.anchoredPosition = dir * arrowDistance;
        }

        /// <summary>Points the arrow towards the given anchored position.</summary>
        public void PointAt(Vector2 anchoredPos)
        {
            if (arrowRect == null) return;
            if (!arrowRect.gameObject.activeSelf)
                arrowRect.gameObject.SetActive(true);

            Vector2 dir = anchoredPos.normalized;
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            _targetCircleAngle = baseAngle;
            _targetGraphicAngle = baseAngle + rotationOffset;
        }

        /// <summary>Hides the entire decision circle and all of its children.</summary>
        public void Hide()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        /// <summary>Shows the decision circle and prepares the arrow for use.</summary>
        public void Show()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }

            // Keep the arrow hidden until it is needed by PointAt
            if (arrowRect != null)
                arrowRect.gameObject.SetActive(false);
        }

        /// <summary>Hides only the arrow indicator without disabling options.</summary>
        public void HideArrow()
        {
            if (arrowRect != null)
                arrowRect.gameObject.SetActive(false);
        }

        private void HandleTimeout()
        {
            Debug.Log("⏰ Decision time ran out!");
        }

        private void OnDestroy()
        {
            if (timer != null)
                timer.OnTimerComplete -= HandleTimeout;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (arrowRect == null) return;
            Handles.color = Color.yellow;
            Handles.DrawWireArc(
                transform.position,
                Vector3.forward,
                transform.right,
                360f,
                arrowDistance
            );
        }
#endif
    }
}
