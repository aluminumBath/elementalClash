using System;

namespace Elementborn.Game
{
    [Serializable]
    public class ShipRuntimeRecord
    {
        public string ShipId = "";
        public int ReputationPoints = 0;
        public ShipReputationTier Tier = ShipReputationTier.Raucous;
        public int RaidsWon = 0;
        public int CelebrationsThrown = 0;
        public bool KnownToPlayer = false;
    }
}
