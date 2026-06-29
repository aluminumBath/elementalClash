using System;

namespace Elementborn.Game
{
    [Serializable]
    public class ShopFactionDiscountRule
    {
        public ElementbornFactionId Faction = ElementbornFactionId.Unknown;
        public int RequiredReputation = 40;
        public float DiscountPercent = 10f;
        public float SellBonusPercent = 0f;
    }
}
