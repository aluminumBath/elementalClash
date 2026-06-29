using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class BlackMarketListingDefinition
    {
        public string ListingId = "";
        public string DisplayName = "Black Market Listing";
        public string ItemId = "";
        public int Quantity = 1;
        public int Price = 100;
        public BlackMarketRiskLevel RiskLevel = BlackMarketRiskLevel.Watched;
        [TextArea]
        public string Rumor = "";
        [TextArea]
        public string Consequence = "";
    }
}
