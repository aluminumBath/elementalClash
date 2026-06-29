using System;
using System.Collections.Generic;
namespace Elementborn.Game
{
    [Serializable]
    public class QuestUiSaveFile
    {
        public int Version = 1; public int SlotIndex = 0; public string TrackedQuestId = ""; public List<QuestUiRecord> Quests = new List<QuestUiRecord>();
    }
}
