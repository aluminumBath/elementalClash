namespace Elementborn.Core
{
    /// <summary>
    /// Arrow ammunition: maps each arrow item id to the element its shot carries. Plain arrows are physical (null);
    /// elemental arrowheads carry their element into the projectile so the matchup (<see cref="ElementMatchup"/>)
    /// against a target's affinity applies. UnityEngine-free and unit-tested.
    /// </summary>
    public static class Ammo
    {
        public const string Plain = "arrow";

        /// <summary>The element an arrow's shot carries, or null for a plain (physical) arrow.</summary>
        public static Element? ElementOf(string arrowId)
        {
            switch (arrowId)
            {
                case "fire_arrow":  return Element.Fire;
                case "water_arrow": return Element.Water;
                case "earth_arrow": return Element.Earth;
                case "air_arrow":   return Element.Air;
                default:            return null; // "arrow" and anything unrecognized: physical
            }
        }

        /// <summary>Every arrow id the bow can spend, elemental first so an attuned shot is preferred over a plain one.</summary>
        public static readonly string[] All = { "fire_arrow", "water_arrow", "earth_arrow", "air_arrow", Plain };
    }
}
