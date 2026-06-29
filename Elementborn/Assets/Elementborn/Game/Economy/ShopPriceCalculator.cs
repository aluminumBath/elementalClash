using UnityEngine;

namespace Elementborn.Game
{
    public static class ShopPriceCalculator
    {
        public static int GetBuyPrice(ShopDefinition shop, ShopItemListing listing, int quantity = 1)
        {
            if (listing == null)
            {
                return 0;
            }

            int each = listing.PriceMode switch
            {
                ShopPriceMode.Free => 0,
                ShopPriceMode.FixedPrice => Mathf.Max(0, listing.FixedBuyPrice),
                _ => Mathf.Max(0, Mathf.RoundToInt(listing.BaseValue * listing.BuyMultiplier))
            };

            float multiplier = shop != null ? shop.GlobalBuyMultiplier : 1f;
            float discount = GetBuyDiscountPercent(shop);
            float final = each * Mathf.Max(0f, multiplier) * (1f - Mathf.Clamp01(discount / 100f));
            return Mathf.Max(0, Mathf.RoundToInt(final) * Mathf.Max(1, quantity));
        }

        public static int GetSellPrice(ShopDefinition shop, ShopItemListing listing, int quantity = 1)
        {
            if (listing == null)
            {
                return 0;
            }

            int each = listing.PriceMode switch
            {
                ShopPriceMode.Free => 0,
                ShopPriceMode.FixedPrice => Mathf.Max(0, listing.FixedSellPrice),
                _ => Mathf.Max(0, Mathf.RoundToInt(listing.BaseValue * listing.SellMultiplier))
            };

            float multiplier = shop != null ? shop.GlobalSellMultiplier : 1f;
            float bonus = GetSellBonusPercent(shop);
            float final = each * Mathf.Max(0f, multiplier) * (1f + Mathf.Max(0f, bonus) / 100f);
            return Mathf.Max(0, Mathf.RoundToInt(final) * Mathf.Max(1, quantity));
        }

        public static int GetFallbackSellPrice(InventoryItemStack stack, int quantity = 1)
        {
            if (stack == null)
            {
                return 0;
            }

            int baseValue = stack.Definition != null ? stack.Definition.BaseValue : 1;
            return Mathf.Max(0, Mathf.RoundToInt(baseValue * 0.5f) * Mathf.Max(1, quantity));
        }

        public static float GetBuyDiscountPercent(ShopDefinition shop)
        {
            if (shop == null)
            {
                return 0f;
            }

            float discount = 0f;
            foreach (var rule in shop.FactionDiscounts)
            {
                if (rule == null || rule.Faction == ElementbornFactionId.Unknown)
                {
                    continue;
                }

                if (FactionReputationTracker.GetValue(rule.Faction) >= rule.RequiredReputation)
                {
                    discount = Mathf.Max(discount, rule.DiscountPercent);
                }
            }

            return discount;
        }

        public static float GetSellBonusPercent(ShopDefinition shop)
        {
            if (shop == null)
            {
                return 0f;
            }

            float bonus = 0f;
            foreach (var rule in shop.FactionDiscounts)
            {
                if (rule == null || rule.Faction == ElementbornFactionId.Unknown)
                {
                    continue;
                }

                if (FactionReputationTracker.GetValue(rule.Faction) >= rule.RequiredReputation)
                {
                    bonus = Mathf.Max(bonus, rule.SellBonusPercent);
                }
            }

            return bonus;
        }
    }
}
