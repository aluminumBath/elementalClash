using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class DialogueMemorySaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<DialogueMemoryFact> Facts = new List<DialogueMemoryFact>();
        public List<NpcRelationshipState> Relationships = new List<NpcRelationshipState>();
        public List<RumorRecord> Rumors = new List<RumorRecord>();
    }
}
