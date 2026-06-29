using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class ShipEventHookDefinition
    {
        public string EventId = "";
        public ShipEventType EventType = ShipEventType.Celebration;
        public string DisplayName = "Ship Event";
        [TextArea]
        public string Description = "";
        public QuestUiDefinition QuestToStart;
        public ElementbornSoundEventId Sound = ElementbornSoundEventId.BoatWaveCreak;
        public int ReputationDelta = 0;
        public bool AddJournalEntry = true;
    }
}
