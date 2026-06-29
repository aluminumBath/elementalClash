using System;

namespace Elementborn.Game
{
    [Serializable]
    public class StoryEncounterRuntimeRecord
    {
        public string EncounterId = "";
        public StoryEncounterState State = StoryEncounterState.Unknown;
        public int TimesStarted = 0;
        public int TimesResolved = 0;
        public int TimesFailed = 0;
        public bool PrimaryLeaderDefeated = false;
        public bool SecondaryLeaderDefeated = false;
        public float FirstLeaderDefeatedTime = -1f;
        public bool SpecialMechanicComplete = false;
        public string LastChoiceId = "";
        public string Notes = "";
    }
}
