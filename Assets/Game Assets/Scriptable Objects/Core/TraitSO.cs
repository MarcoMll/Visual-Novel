using UnityEngine;

namespace GameAssets.ScriptableObjects.Core
{
    [CreateAssetMenu(menuName = "VNE/New trait")]
    public class TraitSO : BaseSO
    {
        public string traitName;
        public string traitDescription;
        public Sprite traitIcon;
    }
}