using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class StoryEncounterProgressSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<StoryEncounterRuntimeRecord> Records = new List<StoryEncounterRuntimeRecord>();
    }
}
