using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class AbilitySaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public int PlayerLevel = 1;
        public int AvailableSkillPoints = 0;
        public List<PlayerAbilityRecord> UnlockedAbilities = new List<PlayerAbilityRecord>();
        public List<AbilityLoadoutSlot> Loadout = new List<AbilityLoadoutSlot>();
    }
}
