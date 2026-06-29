using System;

namespace Elementborn.Game
{
    [Serializable]
    public class FactionReputationRecord
    {
        public ElementbornFactionId Faction = ElementbornFactionId.Unknown;
        public int Reputation = 0;
        public string LastReason = "";

        public string GetStanding()
        {
            if (Reputation >= 75) return "Revered";
            if (Reputation >= 40) return "Trusted";
            if (Reputation >= 15) return "Friendly";
            if (Reputation <= -75) return "Hated";
            if (Reputation <= -40) return "Hostile";
            if (Reputation <= -15) return "Wary";
            return "Neutral";
        }
    }
}
