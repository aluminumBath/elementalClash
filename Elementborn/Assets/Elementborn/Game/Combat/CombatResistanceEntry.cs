using System;

namespace Elementborn.Game
{
    [Serializable]
    public class CombatResistanceEntry
    {
        public AbilityElementType Element = AbilityElementType.Neutral;
        public float Percent = 0f;
    }
}
