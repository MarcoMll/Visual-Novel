using System.Collections.Generic;
using UnityEngine;

namespace VisualNovel.UI.Animations
{
    /// <summary>
    /// Container describing a list of <see cref="UIAnimStep"/> objects that
    /// form a named animation sequence.
    /// </summary>
    [System.Serializable]
    public class UIAnimSequence
    {
        public string id;

        [SerializeReference]
        public List<UIAnimStep> steps = new();
    }
}
