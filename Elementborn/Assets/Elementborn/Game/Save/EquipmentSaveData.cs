using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class EquipmentSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<EquippedItemRecord> Equipped = new List<EquippedItemRecord>();
    }
}
