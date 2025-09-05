using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualNovel.Decisions
{
    [RequireComponent(typeof(AudioSource))]
    public class DecisionCircle : MonoBehaviour
    {
        [Header("Arrow Settings")]
        public RectTransform arrowRect;
        public float arrowDistance = 100f;
        public float rotationOffset = 0f;

        [Header("Smoothing")]
        public float arrowMoveSpeed = 8f;

        [Header("SFX")]
        [Tooltip("Played when hovering a new choice")]
        public AudioClip hoverSfx;
        [Tooltip("Played when clicking a choice")]
        public AudioClip confirmSfx;

        [Header("Timer (optional)")]
        public SimpleTimer timer;

        private AudioSource _audio;
        private DecisionOption _current;
        private float _currentCircleAngle, _targetCircleAngle;
        private float _currentGraphicAngle, _targetGraphicAngle;

        void Awake()
        {
            _audio = GetComponent<AudioSource>();
        }

        void Start()
        {
            arrowRect.gameObject.SetActive(false);

        // angles from initial arrow position
        Vector2 initPos = arrowRect.anchoredPosition;
        float initAngle = Mathf.Atan2(initPos.y, initPos.x) * Mathf.Rad2Deg;
        _currentCircleAngle  = _targetCircleAngle  = initAngle;
        _currentGraphicAngle = _targetGraphicAngle = initAngle + rotationOffset;

            if (timer != null)
            {
                timer.OnTimerComplete += HandleTimeout;
                timer.StartTimer();
            }
        }

        void Update()
        {
            if (!arrowRect.gameObject.activeSelf) return;

        // lerp our circle‐angle
            _currentCircleAngle = Mathf.LerpAngle(
                _currentCircleAngle,
                _targetCircleAngle,
                arrowMoveSpeed * Time.deltaTime
            );

        // lerp our graphic‐angle
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

    /// <summary>Called by each DecisionOption on hover.</summary>
        public void SelectOption(DecisionOption opt)
        {
            if (!arrowRect.gameObject.activeSelf)
                arrowRect.gameObject.SetActive(true);

            if (_current != null && _current != opt)
            {
                _current.Deselect();

                // Play hover SFX
                if (hoverSfx != null)
                    _audio.PlayOneShot(hoverSfx);
            }

            _current = opt;
            _current.Select();

            // compute angles
            Vector2 rawPos = opt.GetCurrentAnchoredPosition();
            Vector2 dir    = rawPos.normalized;
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            _targetCircleAngle  = baseAngle;
            _targetGraphicAngle = baseAngle + rotationOffset;
        }

    /// <summary>Called by DecisionOption when clicked.</summary>
        public void ConfirmOption(DecisionOption opt)
        {
            // Play confirm SFX
            if (confirmSfx != null)
                _audio.PlayOneShot(confirmSfx);

            Debug.Log($"✅ Choice confirmed: {opt.name}");
            // TODO: invoke your actual choice logic here
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
