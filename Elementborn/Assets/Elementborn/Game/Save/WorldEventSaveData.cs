using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class WorldEventSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<WorldEventRecord> Records = new List<WorldEventRecord>();
    }
}
