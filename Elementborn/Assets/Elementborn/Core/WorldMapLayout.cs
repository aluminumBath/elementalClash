using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// The canonical overworld layout for the map UI: the world bounds used to project positions onto the
    /// map / minimap, and the fixed leyline-rift network drawn over the key-art backdrop. The playable world is
    /// seed-generated (see WORLD.md) — these positions define where the rifts sit and how world XZ maps onto the
    /// backdrop, matching the four elemental capitals arranged around the central Confluence Crystal.
    /// (+X is east, +Z is north.)
    /// </summary>
    public static class WorldMapLayout
    {
        /// <summary>South-west / lowest corner of the mapped region (XZ; Y is unused).</summary>
        public static readonly Vector3 BoundsMin = new Vector3(-250f, 0f, -250f);
        /// <summary>North-east / highest corner of the mapped region.</summary>
        public static readonly Vector3 BoundsMax = new Vector3(250f, 0f, 250f);

        /// <summary>The fixed leyline rifts: the four elemental capitals around the central Confluence Crystal,
        /// plus two crossings.</summary>
        public static IReadOnlyList<LeylineRift> Rifts { get; } = new List<LeylineRift>
        {
            new LeylineRift("crystal",   "Confluence Crystal", new Vector3(   0f, 0f,    0f)),
            // Elemental capitals — the portal hubs (their pools route to that element's discovered city portals).
            new LeylineRift("stone",     "Stonereach",         new Vector3(-160f, 0f,  150f), Element.Earth, PortalTier.Capital),
            new LeylineRift("gale",      "Gale Roost",         new Vector3( 160f, 0f,  150f), Element.Air,   PortalTier.Capital),
            new LeylineRift("tide",      "Tidewatch",          new Vector3(-160f, 0f, -150f), Element.Water, PortalTier.Capital),
            new LeylineRift("ember",     "Ember Bastion",      new Vector3( 160f, 0f, -150f), Element.Fire,  PortalTier.Capital),
            new LeylineRift("skybridge", "Sky Bridge",         new Vector3(   0f, 0f,  110f)),
            new LeylineRift("verdant",   "Verdant Crossing",   new Vector3(   0f, 0f,  -70f)),

            // City portals — discovered by visiting, then reachable from their element's capital pool.
            new LeylineRift("earth_ridgehold",  "Ridgehold",        new Vector3(-205f, 0f,  120f), Element.Earth, PortalTier.City),
            new LeylineRift("earth_looming",    "Loam Hollow",      new Vector3(-120f, 0f,  205f), Element.Earth, PortalTier.City),
            new LeylineRift("air_cirrus",       "Cirrus Landing",   new Vector3( 205f, 0f,  120f), Element.Air,   PortalTier.City),
            new LeylineRift("air_zephyr",       "Zephyr Terrace",   new Vector3( 120f, 0f,  205f), Element.Air,   PortalTier.City),
            new LeylineRift("water_reefwood",   "Neritha Reefwood", new Vector3(-210f, 0f, -110f), Element.Water, PortalTier.City),
            new LeylineRift("water_saltglass",  "Saltglass Cove",   new Vector3(-120f, 0f, -210f), Element.Water, PortalTier.City),
            new LeylineRift("fire_cinderhold",  "Cinderhold",       new Vector3( 210f, 0f, -110f), Element.Fire,  PortalTier.City),
            new LeylineRift("fire_ashmarket",   "Ash Market",       new Vector3( 120f, 0f, -210f), Element.Fire,  PortalTier.City),
        };

        /// <summary>A fresh fast-travel network with every canonical rift registered (none discovered yet).</summary>
        public static FastTravelNetwork BuildNetwork()
        {
            var net = new FastTravelNetwork();
            foreach (var r in Rifts) net.Register(r);
            return net;
        }

        /// <summary>The fixed respawn shrines — cardinal waystones ringing the central crystal, set apart from the
        /// leyline rifts. Activating one makes it the player's respawn anchor.</summary>
        public static IReadOnlyList<Checkpoint> Checkpoints { get; } = new List<Checkpoint>
        {
            new Checkpoint("waystone_n", "North Waystone", new Vector3(   0f, 0f,  75f)),
            new Checkpoint("waystone_s", "South Waystone", new Vector3(   0f, 0f, -75f)),
            new Checkpoint("waystone_e", "East Waystone",  new Vector3(  90f, 0f,   0f)),
            new Checkpoint("waystone_w", "West Waystone",  new Vector3( -90f, 0f,   0f)),
        };
    }
}
