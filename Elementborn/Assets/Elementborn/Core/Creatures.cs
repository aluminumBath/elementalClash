namespace Elementborn.Core
{
    /// <summary>Every tameable/ownable creature — mounts and bestiary plus the rare combat companions.</summary>
    public enum CreatureKind
    {
        // Bestiary / mounts
        FireDragon, WaterDragon, Mermaid, EarthMole, EarthCat, AirDragonfly, AirJellyfish, Horse,
        // Rare combat companions
        Spider, WaterCat, IceCat, Phoenix, ElectricSquirrel, Dog,
        // Wildlife by habitat
        Eel, Crab, Monkey, Crocodile, Snake, Roc, Thunderbird, Rhino, Tiger,
        // Exotic apex creatures — rare, hard to tame, in tough locations (original designs)
        Ridgewing, Glidewisp, Skytyrant, Goldkoi, Direstalker, Skimfin, Gillcloak, Tidewarden
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

                // Wildlife — roams its habitat, peaceful until provoked. Tame-only; the big ones can be ridden.
                case CreatureKind.Eel:
                    return new CreatureInfo("River Eel", Core.Element.Water, null, false, false, false, 0, 0.40f);
                case CreatureKind.Crab:
                    return new CreatureInfo("Shore Crab", null, null, false, false, false, 0, 0.55f);
                case CreatureKind.Monkey:
                    return new CreatureInfo("Forest Monkey", null, null, false, false, false, 0, 0.55f);
                case CreatureKind.Crocodile:
                    return new CreatureInfo("Marsh Crocodile", null, null, false, false, false, 0, 0.20f);
                case CreatureKind.Snake:
                    return new CreatureInfo("Swamp Snake", null, null, false, false, false, 0, 0.35f);
                case CreatureKind.Roc:
                    return new CreatureInfo("Roc", Core.Element.Air, null, true, false, false, 0, 0.15f);
                case CreatureKind.Thunderbird:
                    return new CreatureInfo("Thunderbird", Core.Element.Fire, null, true, false, false, 0, 0.15f);
                case CreatureKind.Rhino:
                    return new CreatureInfo("Plains Rhino", Core.Element.Earth, null, true, false, false, 0, 0.25f);
                case CreatureKind.Tiger:
                    return new CreatureInfo("Jungle Tiger", null, null, false, false, false, 0, 0.20f);

                // Exotic apex creatures — tameable but rare and stubborn (very low tame chance), found in tough spots.
                case CreatureKind.Ridgewing:   // cliff-soaring flying mount
                    return new CreatureInfo("Ridgewing", Core.Element.Air, null, true, false, false, 0, 0.12f);
                case CreatureKind.Glidewisp:   // small forest flyer
                    return new CreatureInfo("Glidewisp", Core.Element.Air, null, true, false, false, 0, 0.14f);
                case CreatureKind.Skytyrant:   // immense apex flyer
                    return new CreatureInfo("Skytyrant", Core.Element.Air, null, true, false, false, 0, 0.05f);
                case CreatureKind.Goldkoi:     // gold-green aquatic glider
                    return new CreatureInfo("Goldkoi", Core.Element.Water, null, true, false, false, 0, 0.12f);
                case CreatureKind.Direstalker: // land apex predator
                    return new CreatureInfo("Direstalker", Core.Element.Earth, null, true, false, false, 0, 0.07f);
                case CreatureKind.Skimfin:     // fast aquatic skimmer mount
                    return new CreatureInfo("Skimfin", Core.Element.Water, null, true, false, false, 0, 0.13f);
                case CreatureKind.Gillcloak:   // mantled aquatic creature
                    return new CreatureInfo("Gillcloak", Core.Element.Water, null, true, false, false, 0, 0.11f);
                case CreatureKind.Tidewarden:  // colossal sentient sea creature
                    return new CreatureInfo("Tidewarden", Core.Element.Water, null, true, false, false, 0, 0.06f);

                default:
                    return new CreatureInfo("Unknown", null, null, false, false, false, 0, 0.5f);
            }
        }
    }
}
