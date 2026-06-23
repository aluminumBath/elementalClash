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
    public static class WorldMap
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
            new LeylineRift("stone",     "Stonereach",         new Vector3(-160f, 0f,  150f)), // Earth, NW
            new LeylineRift("gale",      "Gale Roost",         new Vector3( 160f, 0f,  150f)), // Air, NE
            new LeylineRift("tide",      "Tidewatch",          new Vector3(-160f, 0f, -150f)), // Water, SW
            new LeylineRift("ember",     "Ember Bastion",      new Vector3( 160f, 0f, -150f)), // Fire, SE
            new LeylineRift("skybridge", "Sky Bridge",         new Vector3(   0f, 0f,  110f)),
            new LeylineRift("verdant",   "Verdant Crossing",   new Vector3(   0f, 0f,  -70f)),
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
