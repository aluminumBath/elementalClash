using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>The rooms/stations a player can add to their home. Wardrobe changes appearance only — never elements.</summary>
    public enum HomeAddition { Storage, Workshop, Wardrobe, Stable, EnchantingTable, Garden }

    /// <summary>Result of attempting to build a home addition.</summary>
    public enum BuildOutcome { Built, NoHouse, AlreadyBuilt, NeedLevel, NeedFunds }

    /// <summary>
    /// Costs, level gates, and labels for the home and its additions. The home itself is gated behind some
    /// progression (<see cref="RequiredLevelToClaim"/>) so it's a mid-game reward; each addition has a silver cost
    /// and (sometimes) its own level prerequisite. Pure and unit-tested.
    /// </summary>
    public static class HomesteadCatalog
    {
        /// <summary>You must reach this level before a home plot can be claimed.</summary>
        public const int RequiredLevelToClaim = 5;

        public static readonly HomeAddition[] All =
        {
            HomeAddition.Storage, HomeAddition.Workshop, HomeAddition.Wardrobe,
            HomeAddition.Stable, HomeAddition.EnchantingTable, HomeAddition.Garden,
        };

        /// <summary>Silver to build the addition.</summary>
        public static int CostOf(HomeAddition a)
        {
            switch (a)
            {
                case HomeAddition.Storage:         return 120;
                case HomeAddition.Workshop:        return 200;
                case HomeAddition.Wardrobe:        return 160;
                case HomeAddition.Stable:          return 260;
                case HomeAddition.EnchantingTable: return 320;
                case HomeAddition.Garden:          return 140;
                default:                           return 150;
            }
        }

        /// <summary>Player level required to build this addition (on top of owning a home).</summary>
        public static int RequiredLevelFor(HomeAddition a)
        {
            switch (a)
            {
                case HomeAddition.Stable:          return 8;
                case HomeAddition.EnchantingTable: return 10;
                default:                           return RequiredLevelToClaim;
            }
        }

        public static string DisplayName(HomeAddition a)
        {
            switch (a)
            {
                case HomeAddition.Storage:         return "Storage Chest";
                case HomeAddition.Workshop:        return "Workshop";
                case HomeAddition.Wardrobe:        return "Wardrobe";
                case HomeAddition.Stable:          return "Creature Stable";
                case HomeAddition.EnchantingTable: return "Enchanting Table";
                case HomeAddition.Garden:          return "Garden";
                default:                           return a.ToString();
            }
        }

        public static string Describe(HomeAddition a)
        {
            switch (a)
            {
                case HomeAddition.Storage:         return "Stash items you don't want to carry.";
                case HomeAddition.Workshop:        return "Craft and combine materials into gear.";
                case HomeAddition.Wardrobe:        return "Change your look — appearance only, never your element.";
                case HomeAddition.Stable:          return "House, rest, and manage your tamed creatures.";
                case HomeAddition.EnchantingTable: return "Imbue worn armor with an element.";
                case HomeAddition.Garden:          return "Grow plants and harvest ingredients.";
                default:                           return "";
            }
        }
    }

    /// <summary>
    /// The additions a player has built onto their home. Owning the home itself is tracked separately (the existing
    /// <c>PlayerInventory.HasHouse</c>), so the two never drift; this is purely the set of built stations. Pure and
    /// unit-tested — the Game layer charges silver and persists the set alongside the house.
    /// </summary>
    public sealed class Homestead
    {
        private readonly HashSet<HomeAddition> _built = new HashSet<HomeAddition>();

        public bool Has(HomeAddition a) => _built.Contains(a);
        public IEnumerable<HomeAddition> Built => _built;
        public int Count => _built.Count;

        /// <summary>Whether the home plot may be claimed yet (the progression gate). Funds are checked separately.</summary>
        public static bool CanClaim(int playerLevel) => playerLevel >= HomesteadCatalog.RequiredLevelToClaim;

        /// <summary>Validate and (on success) record a built addition. This moves no money — the caller spends
        /// <paramref name="cost"/> only when the result is <see cref="BuildOutcome.Built"/>.</summary>
        public BuildOutcome TryBuild(HomeAddition a, bool hasHouse, int playerLevel, int funds, out int cost)
        {
            cost = HomesteadCatalog.CostOf(a);
            if (!hasHouse) return BuildOutcome.NoHouse;
            if (_built.Contains(a)) return BuildOutcome.AlreadyBuilt;
            if (playerLevel < HomesteadCatalog.RequiredLevelFor(a)) return BuildOutcome.NeedLevel;
            if (funds < cost) return BuildOutcome.NeedFunds;
            _built.Add(a);
            return BuildOutcome.Built;
        }

        // --- persistence (the Game layer folds these into the existing house save) ---
        public IEnumerable<string> SaveAdditions()
        {
            foreach (var a in _built) yield return a.ToString();
        }

        public void Restore(IEnumerable<string> additions)
        {
            _built.Clear();
            if (additions == null) return;
            foreach (var s in additions)
                if (System.Enum.TryParse(s, out HomeAddition a)) _built.Add(a);
        }
    }
}
