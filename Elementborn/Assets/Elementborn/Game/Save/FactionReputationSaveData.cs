using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class FactionReputationSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<FactionReputationRecord> Reputations = new List<FactionReputationRecord>();
    }
}
