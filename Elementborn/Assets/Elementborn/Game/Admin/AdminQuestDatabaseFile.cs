using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class AdminQuestDatabaseFile
    {
        public int Version = 1;
        public List<AdminQuestRecord> Quests = new List<AdminQuestRecord>();
    }
}
