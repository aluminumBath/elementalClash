using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class PoliticalWorldEventDirectorSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public int CurrentWorldDay = 1;
        public List<PoliticalWorldEventRuntimeRecord> RuntimeRecords = new List<PoliticalWorldEventRuntimeRecord>();
    }
}
