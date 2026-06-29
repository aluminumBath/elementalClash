using System;

namespace Elementborn.Game
{
    [Serializable]
    public class AbilityLoadoutSlot
    {
        public AbilitySlotType SlotType = AbilitySlotType.Primary;
        public string AbilityId = "";
    }
}
