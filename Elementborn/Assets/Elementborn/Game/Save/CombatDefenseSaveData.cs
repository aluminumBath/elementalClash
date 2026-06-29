using System;

namespace Elementborn.Game
{
    [Serializable]
    public class CombatDefenseSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public float CurrentStamina = 100f;
        public float MaxStamina = 100f;
    }
}
