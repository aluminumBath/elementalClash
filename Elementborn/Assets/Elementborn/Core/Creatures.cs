namespace Elementborn.Core
{
    /// <summary>Every tameable/ownable creature — mounts and bestiary plus the rare combat companions.</summary>
    public enum CreatureKind
    {
        // Bestiary / mounts
        FireDragon, WaterDragon, Mermaid, EarthMole, EarthCat, AirDragonfly, AirJellyfish, Horse,
        // Rare combat companions
        Spider, WaterCat, IceCat, Phoenix, ElectricSquirrel, Dog
    }

    /// <summary>Static data for a creature: theme, who may own it, and how it's acquired.</summary>
    public readonly struct CreatureInfo
    {
        public readonly string Name;
        public readonly Element? Element;          // thematic element
        public readonly Element? RequiredElement;  // a user must channel this to own/use it (null = anyone)
        public readonly bool Rideable;             // large enough to ride
        public readonly bool IsCompanion;          // a rare combat helper
        public readonly bool Purchasable;          // can be bought (else tame-only)
        public readonly long Price;                // silver value if purchasable
        public readonly float TameChance;          // base success once weakened + lured

        public CreatureInfo(string name, Element? element, Element? requiredElement, bool rideable,
            bool isCompanion, bool purchasable, long price, float tameChance)
        {
            Name = name;
            Element = element;
            RequiredElement = requiredElement;
            Rideable = rideable;
            IsCompanion = isCompanion;
            Purchasable = purchasable;
            Price = price;
            TameChance = tameChance;
        }
    }

    /// <summary>Lookup table for creature data. Tweak balance here.</summary>
    public static class CreatureCatalog
    {
        public static CreatureInfo For(CreatureKind kind)
        {
            switch (kind)
            {
                // name, element, required, rideable, companion, purchasable, price, tameChance
                case CreatureKind.FireDragon:
                    return new CreatureInfo("Fire Dragon", Core.Element.Fire, Core.Element.Fire, true, false, true, 5000, 0.20f);
                case CreatureKind.WaterDragon:
                    return new CreatureInfo("Water Dragon", Core.Element.Water, Core.Element.Water, true, false, true, 5000, 0.20f);
                case CreatureKind.Mermaid:
                    return new CreatureInfo("Mermaid", Core.Element.Water, Core.Element.Water, false, false, true, 1500, 0.35f);
                case CreatureKind.EarthMole:
                    return new CreatureInfo("Earth Mole", Core.Element.Earth, Core.Element.Earth, true, false, true, 1200, 0.40f);
                case CreatureKind.EarthCat:
                    return new CreatureInfo("Earth Cat", Core.Element.Earth, Core.Element.Earth, false, false, true, 400, 0.60f);
                case CreatureKind.AirDragonfly:
                    return new CreatureInfo("Air Dragonfly", Core.Element.Air, Core.Element.Air, true, false, true, 1800, 0.35f);
                case CreatureKind.AirJellyfish:
                    return new CreatureInfo("Air Jellyfish", Core.Element.Air, Core.Element.Air, true, false, true, 1600, 0.35f);
                case CreatureKind.Horse:
                    return new CreatureInfo("Horse", Core.Element.Earth, Core.Element.Earth, true, false, true, 300, 0.60f);

                // Companions are tame-only and harder to win over.
                case CreatureKind.Spider:
                    return new CreatureInfo("Web Spider", Core.Element.Earth, Core.Element.Earth, false, true, false, 0, 0.22f);
                case CreatureKind.WaterCat:
                    return new CreatureInfo("Water Cat", Core.Element.Water, Core.Element.Water, false, true, false, 0, 0.25f);
                case CreatureKind.IceCat:
                    return new CreatureInfo("Ice Cat", Core.Element.Water, Core.Element.Water, false, true, false, 0, 0.25f);
                case CreatureKind.Phoenix:
                    return new CreatureInfo("Phoenix", Core.Element.Fire, Core.Element.Fire, true, true, false, 0, 0.20f);
                case CreatureKind.ElectricSquirrel:
                    return new CreatureInfo("Storm Squirrel", Core.Element.Fire, Core.Element.Fire, false, true, false, 0, 0.25f);
                case CreatureKind.Dog:
                    return new CreatureInfo("Earth Hound", Core.Element.Earth, Core.Element.Earth, false, true, false, 0, 0.30f);

                default:
                    return new CreatureInfo("Unknown", null, null, false, false, false, 0, 0.5f);
            }
        }
    }
}
