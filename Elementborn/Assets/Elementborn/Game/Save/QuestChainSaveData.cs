using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestChainSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<QuestChainRuntimeRecord> RuntimeRecords = new List<QuestChainRuntimeRecord>();
    }
}
