using System;

namespace Elementborn.Game
{
    [Serializable]
    public class CreatureOrphanageResidentRecord
    {
        public string CreatureId = "";
        public string DisplayName = "Creature";
        public CreatureOrphanageDepartureReason DepartureReason = CreatureOrphanageDepartureReason.Unknown;
        public CreatureOrphanageResidentState State = CreatureOrphanageResidentState.Recovering;
        public int CareDebt = 0;
        public int TrustPenalty = 0;
        public int TimesRecovered = 0;
        public bool CanBeLuredBack = true;
        public bool CanBeBoughtBack = true;
        public string LastKnownScene = "";
        public string Notes = "";
    }
}
