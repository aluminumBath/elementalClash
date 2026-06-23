namespace Elementborn.Core
{
    /// <summary>
    /// The specialty a pair of elements unlocks in the evolution mode (order-independent). Mirrors the design:
    /// Water+Earth = plant, Water+Air = blood, Water+Fire = steam & healing, Earth+Air = metal/rust,
    /// Earth+Fire = lava, Fire+Air = flight. Pure and unit-tested.
    /// </summary>
    public static class Specialties
    {
        public static SubArt For(Element a, Element b)
        {
            if (a == b) return SubArt.None;

            bool Pair(Element x, Element y) => (a == x && b == y) || (a == y && b == x);

            if (Pair(Element.Water, Element.Earth)) return SubArt.Verdancy;     // plant control
            if (Pair(Element.Water, Element.Air))   return SubArt.SanguineGrip; // blood
            if (Pair(Element.Water, Element.Fire))  return SubArt.Steamcraft;   // steam + healing
            if (Pair(Element.Earth, Element.Air))   return SubArt.Oreshaping;   // metal (rust)
            if (Pair(Element.Earth, Element.Fire))  return SubArt.Magmacraft;   // lava
            if (Pair(Element.Fire, Element.Air))    return SubArt.Flight;       // flight
            return SubArt.None;
        }

        public static string NameOf(SubArt specialty)
        {
            switch (specialty)
            {
                case SubArt.Verdancy:     return "Plant Control";
                case SubArt.SanguineGrip: return "Blood";
                case SubArt.Steamcraft:   return "Steam & Healing";
                case SubArt.Oreshaping:   return "Metal (Rust)";
                case SubArt.Magmacraft:   return "Lava";
                case SubArt.Flight:       return "Flight";
                default:                  return "None";
            }
        }
    }
}
