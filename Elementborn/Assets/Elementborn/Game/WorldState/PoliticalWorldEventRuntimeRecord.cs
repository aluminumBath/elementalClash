using System;

namespace Elementborn.Game
{
    [Serializable]
    public class PoliticalWorldEventRuntimeRecord
    {
        public string EventId = "";
        public PoliticalWorldEventState State = PoliticalWorldEventState.Dormant;
        public int TimesActivated = 0;
        public int CooldownUntilDay = 0;
        public string LastActivationReason = "";
    }
}
