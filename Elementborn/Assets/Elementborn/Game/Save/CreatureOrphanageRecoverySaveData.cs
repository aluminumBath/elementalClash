using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class CreatureOrphanageRecoverySaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<CreatureOrphanageResidentRecord> Residents = new List<CreatureOrphanageResidentRecord>();
    }
}
