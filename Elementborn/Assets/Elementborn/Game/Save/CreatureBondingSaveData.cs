using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class CreatureBondingSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<OwnedCreatureRecord> OwnedCreatures = new List<OwnedCreatureRecord>();
        public List<CreatureStableSaveRecord> Stables = new List<CreatureStableSaveRecord>();
    }

    [Serializable]
    public class CreatureStableSaveRecord
    {
        public string StableId = "";
        public List<string> StoredCreatureRecordIds = new List<string>();
    }
}
