using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class ShopSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<ShopSaveRecord> Shops = new List<ShopSaveRecord>();
    }

    [Serializable]
    public class ShopSaveRecord
    {
        public string ShopId = "";
        public float LastRestockedAtUnscaledTime = -1f;
        public List<ShopStockSaveRecord> Stock = new List<ShopStockSaveRecord>();
    }

    [Serializable]
    public class ShopStockSaveRecord
    {
        public string ItemId = "";
        public int Stock = 0;
    }
}
