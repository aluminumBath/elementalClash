using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class InventorySaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public int Currency = 0;
        public List<InventorySaveStack> Stacks = new List<InventorySaveStack>();
        public List<StorageSaveRecord> Storage = new List<StorageSaveRecord>();
    }

    [Serializable]
    public class InventorySaveStack
    {
        public string ItemId = "";
        public int Quantity = 0;
    }

    [Serializable]
    public class StorageSaveRecord
    {
        public string StorageId = "";
        public List<InventorySaveStack> Stacks = new List<InventorySaveStack>();
    }
}
