using System;

namespace Elementborn.Game
{
    [Serializable]
    public class AdminQuestRewardRecord
    {
        public string ItemId = "";
        public int Quantity = 1;
        public int Currency = 0;
        public int ReputationAmount = 0;
        public ElementbornFactionId ReputationFaction = ElementbornFactionId.Unknown;
    }
}
