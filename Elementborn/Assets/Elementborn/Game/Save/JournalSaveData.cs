using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class JournalSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<JournalEntryRecord> Entries = new List<JournalEntryRecord>();
    }
}
