using System;

namespace Elementborn.Game
{
    [Serializable]
    public class EquippedItemRecord
    {
        public EquipmentSlotType Slot = EquipmentSlotType.None;
        public string EquipmentId = "";
        public string ItemId = "";
        public string DisplayName = "";
        public EquipmentCategory Category = EquipmentCategory.Unknown;
        public InventoryItemRarity Rarity = InventoryItemRarity.Common;
    }
}
