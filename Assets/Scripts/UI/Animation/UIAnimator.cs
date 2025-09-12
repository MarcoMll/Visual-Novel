using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace VisualNovel.UI.Animations
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIAnimator : MonoBehaviour
    {
        [SerializeField] public List<UIAnimSequence> sequences = new();

        RectTransform _rect;
        CanvasGroup _canvas;
        Sequence _current;

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvas = GetComponent<CanvasGroup>();
            if (_canvas == null) _canvas = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>Plays a sequence identified by <paramref name="id"/>.</summary>
        public Sequence Play(string id)
        {
            var data = sequences.Find(s => s.id == id);
            if (data == null)
            {
                Debug.LogWarning($"UIAnimator: No sequence with id '{id}'");
                return null;
            }

            _current?.Kill();
            var seq = DOTween.Sequence();
            foreach (var step in data.steps)
                seq.Append(step.Build(_rect, _canvas));
            _current = seq;
            return _current.Play();
        }

        /// <summary>Stops any running sequence.</summary>
        public void Stop()
        {
            _current?.Kill();
            _current = null;
        }
    }
}
