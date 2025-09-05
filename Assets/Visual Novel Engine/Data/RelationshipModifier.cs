using System;
using GameAssets.ScriptableObjects.Core;

namespace VisualNovelEngine.Data
{
    [Serializable]
    public class RelationshipModifier
    {
        public CharacterSO Character;
        public int Amount;
    }
}
