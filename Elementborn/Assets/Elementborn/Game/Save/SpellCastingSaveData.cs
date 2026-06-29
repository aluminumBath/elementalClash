using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class SpellCastingSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public float CurrentResource = 100f;
        public float MaxResource = 100f;
        public List<SpellCooldownRecord> Cooldowns = new List<SpellCooldownRecord>();
    }
}
