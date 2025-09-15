using UnityEngine;

namespace GameAssets.ScriptableObjects.Core
{
    /// <summary>
    /// Base class for all skill types.
    /// </summary>
    public abstract class BaseSkill : BaseSO
    {
        public Sprite skillSprite;
        public string skillName;
        public string skillDescription;
    }
}
