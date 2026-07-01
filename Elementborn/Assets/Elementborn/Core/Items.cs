using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    public enum ItemCategory { Food, Material, Consumable, Tool, Treasure }

    public sealed class ItemDef
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public ItemCategory Category { get; }
        public int Value { get; }   // base price in silver-equivalent

        public ItemDef(string id, string name, string description, ItemCategory category, int value)
        {
            Id = id; Name = name; Description = description; Category = category;
            Value = value < 1 ? 1 : value;
        }
    }

    /// <summary>The authored item set, by string id (so mods can extend it later). Foods line up with Willow's
    /// sidekick foods so feeding can consume the matching item.</summary>
    public static class ItemCatalog
    {
        private static readonly Dictionary<string, ItemDef> ById = Build();

        public static IReadOnlyList<ItemDef> All()
        {
            var list = new List<ItemDef>(ById.Values);
            return list;
        }

        public static ItemDef Get(string id) => id != null && ById.TryGetValue(id, out var d) ? d : null;
        public static bool Exists(string id) => id != null && ById.ContainsKey(id);

        /// <summary>The food item a given sidekick eats.</summary>
        public static string FoodFor(WillowSidekick sidekick)
        {
            switch (sidekick)
            {
                case WillowSidekick.Parrot: return "sunflower_seeds";
                case WillowSidekick.Blobfish: return "deep_jelly";
                case WillowSidekick.Mushroom: return "compost_truffle";
                case WillowSidekick.Chameleon: return "iridescent_beetle";
                default: return "ore_marrow_bone"; // Gunnar
            }
        }

        private static Dictionary<string, ItemDef> Build()
        {
            var d = new Dictionary<string, ItemDef>();
            void Add(ItemDef def) => d[def.Id] = def;

            // Foods (match the sidekick menagerie).
            Add(new ItemDef("ore_marrow_bone", "Ore-marrow Bone", "Gunnar gnaws these for hours.", ItemCategory.Food, 8));
            Add(new ItemDef("sunflower_seeds", "Midnight Sunflower Seeds", "The Parrot's midnight snack.", ItemCategory.Food, 4));
            Add(new ItemDef("deep_jelly", "Briny Deep-jelly", "The Blobfish slurps it down.", ItemCategory.Food, 6));
            Add(new ItemDef("compost_truffle", "Loamy Compost Truffle", "A treat for the Mushroom.", ItemCategory.Food, 6));
            Add(new ItemDef("iridescent_beetle", "Iridescent Beetle", "The Chameleon's favourite catch.", ItemCategory.Food, 5));

            // Materials.
            Add(new ItemDef("hide", "Hide", "Tanned creature hide.", ItemCategory.Material, 10));
            Add(new ItemDef("ember_shard", "Ember Shard", "Warm to the touch even in the dark.", ItemCategory.Material, 18));
            Add(new ItemDef("river_pearl", "River Pearl", "A pale glimmer from the shallows.", ItemCategory.Material, 25));

            // Consumables.
            Add(new ItemDef("healing_tonic", "Healing Tonic", "Restores vigour in a pinch.", ItemCategory.Consumable, 15));
            Add(new ItemDef("stamina_draught", "Stamina Draught", "A bottled second wind.", ItemCategory.Consumable, 12));

            // Treasure (sell-only fodder).
            Add(new ItemDef("old_relic", "Old Relic", "Worth something to the right buyer.", ItemCategory.Treasure, 40));

            // Crafted (made at no shop, only via recipes — see Crafting.cs).
            Add(new ItemDef("tough_leather", "Tough Leather", "Cured and layered hide, far tougher than the raw skin.", ItemCategory.Material, 22));
            // Craftable base armor — equip in the four armor slots, then enchant each with an element (additive resist).
            Add(new ItemDef("iron_helm", "Iron Helm", "A banded helm; takes an elemental enchant well.", ItemCategory.Material, 28));
            Add(new ItemDef("warding_cloak", "Warding Cloak", "A heavy traveller\u2019s cloak, easily imbued.", ItemCategory.Material, 26));
            Add(new ItemDef("sturdy_boots", "Sturdy Boots", "Reinforced marching boots.", ItemCategory.Material, 24));
            Add(new ItemDef("elixir_of_vigor", "Elixir of Vigor", "Restores body and stamina at once.", ItemCategory.Consumable, 60));
            Add(new ItemDef("elemental_charm", "Elemental Charm", "A charm humming with all four elements.", ItemCategory.Treasure, 140));
            Add(new ItemDef("arrow", "Arrow", "Fletched ammunition for a longbow.", ItemCategory.Material, 1));
            Add(new ItemDef("fire_arrow", "Fire Arrow", "An arrowhead that carries a Fire bite.", ItemCategory.Material, 3));
            Add(new ItemDef("water_arrow", "Water Arrow", "An arrowhead that carries a Water bite.", ItemCategory.Material, 3));
            Add(new ItemDef("earth_arrow", "Earth Arrow", "An arrowhead that carries an Earth bite.", ItemCategory.Material, 3));
            Add(new ItemDef("air_arrow", "Air Arrow", "An arrowhead that carries an Air bite.", ItemCategory.Material, 3));

            // Hidden-landmark items (Slice B). The draught is wired in Consumables; the rest are relic/flavour for now.
            Add(new ItemDef("tideglass_draught", "Tideglass Draught", "A phial of Tidecaller bubble-glass; drink to breathe water for a short while.", ItemCategory.Consumable, 35));
            Add(new ItemDef("keelwood_splinter", "Keelwood Splinter", "A sliver from a hull that once passed the Veil of Tempests; it remembers the way to Thalen'Veyr.", ItemCategory.Treasure, 55));
            Add(new ItemDef("stormwardens_token", "Stormwarden's Token", "A storm-etched token the Keepers give to those the Veil should recognise.", ItemCategory.Treasure, 45));
            Add(new ItemDef("prism_shard", "Prism Shard", "A shard of Ilyrath prism-glass holding a sliver of stored daylight.", ItemCategory.Treasure, 40));
            Add(new ItemDef("bottled_updraft", "Bottled Updraft", "A stoppered gust from the Caldera Spire; the Djinn trade them like coin.", ItemCategory.Treasure, 30));

            return d;
        }
    }

    /// <summary>A pure item bag: counts by item id, with add/remove and change notification. No Unity types.</summary>
    public sealed class Inventory
    {
        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();
        public event Action Changed;

        public int Count(string id) => id != null && _counts.TryGetValue(id, out int n) ? n : 0;
        public bool Has(string id, int n = 1) => Count(id) >= n;

        public int Total
        {
            get { int t = 0; foreach (var v in _counts.Values) t += v; return t; }
        }

        public void Add(string id, int n = 1)
        {
            if (id == null || n <= 0) return;
            _counts[id] = Count(id) + n;
            Changed?.Invoke();
        }

        public bool Remove(string id, int n = 1)
        {
            if (id == null || n <= 0 || Count(id) < n) return false;
            int left = Count(id) - n;
            if (left <= 0) _counts.Remove(id);
            else _counts[id] = left;
            Changed?.Invoke();
            return true;
        }

        public IReadOnlyList<KeyValuePair<string, int>> Entries()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var kv in _counts) if (kv.Value > 0) list.Add(kv);
            return list;
        }

        public void Clear()
        {
            if (_counts.Count == 0) return;
            _counts.Clear();
            Changed?.Invoke();
        }
    }

    public sealed class ShopResult
    {
        public bool Success { get; }
        public string Message { get; }
        private ShopResult(bool ok, string msg) { Success = ok; Message = msg; }
        public static ShopResult Ok(string m) => new ShopResult(true, m);
        public static ShopResult Fail(string m) => new ShopResult(false, m);
    }

    /// <summary>Pure shop transactions over an <see cref="Inventory"/> and a <see cref="Wallet"/>. Prices come from
    /// the catalog; buy-back is category-aware (see <see cref="SellFractionFor"/>). Nothing changes on a failed
    /// transaction.</summary>
    public static class Shop
    {
        /// <summary>Default buy-back fraction, used for any category without a specific rate.</summary>
        public const float SellFraction = 0.5f;

        /// <summary>
        /// Buy-back fraction by category. Treasure is sell-only fodder, so it fetches near full price; tools and
        /// consumables take a steeper resale haircut; crafting stock and food sell at the base rate. Every
        /// fraction is below 1.0, so buying then re-selling is always a loss (no arbitrage loop).
        /// </summary>
        public static float SellFractionFor(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Treasure:   return 0.9f;
                case ItemCategory.Tool:       return 0.4f;
                case ItemCategory.Consumable: return 0.4f;
                case ItemCategory.Material:   return SellFraction;
                case ItemCategory.Food:       return SellFraction;
                default:                      return SellFraction;
            }
        }

        public static int BuyPrice(string itemId)
        {
            var def = ItemCatalog.Get(itemId);
            return def != null ? def.Value : 0;
        }

        public static int SellPrice(string itemId)
        {
            var def = ItemCatalog.Get(itemId);
            if (def == null) return 0;
            return Math.Max(1, (int)(def.Value * SellFractionFor(def.Category) + 0.0001f));
        }

        public static ShopResult Buy(Inventory inventory, Wallet wallet, string itemId, int count = 1)
        {
            if (inventory == null || wallet == null) return ShopResult.Fail("Nothing to trade with");
            var def = ItemCatalog.Get(itemId);
            if (def == null) return ShopResult.Fail("No such item");
            if (count <= 0) return ShopResult.Fail("Nothing to buy");
            long cost = (long)def.Value * count;
            if (!wallet.CanAfford(cost)) return ShopResult.Fail("Not enough silver");
            if (!wallet.Spend(cost)) return ShopResult.Fail("Payment failed");
            inventory.Add(itemId, count);
            return ShopResult.Ok("Bought " + count + "x " + def.Name);
        }

        public static ShopResult Sell(Inventory inventory, Wallet wallet, string itemId, int count = 1)
        {
            if (inventory == null || wallet == null) return ShopResult.Fail("Nothing to trade with");
            var def = ItemCatalog.Get(itemId);
            if (def == null) return ShopResult.Fail("No such item");
            if (count <= 0) return ShopResult.Fail("Nothing to sell");
            if (!inventory.Remove(itemId, count)) return ShopResult.Fail("You don't have that");
            long payout = (long)SellPrice(itemId) * count;
            wallet.Add(Currency.Silver, (int)payout);
            return ShopResult.Ok("Sold " + count + "x " + def.Name);
        }
    }
}
