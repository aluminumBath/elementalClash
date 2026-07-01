using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// The four hand-authored hidden landmarks adapted from the source modules. Unlike the procedurally-seeded
    /// <see cref="SiteKind"/> destinations, each of these is a unique, named mega-location with a bespoke way in.
    /// This catalogue is the pure, testable spine everything else hangs off: the world spawner reads it to place
    /// the entrance, the Grimoire reads it for discovery, and the interior loaders read it to know what to build.
    /// Each stays hidden until found — none appear on the chart by default.
    /// </summary>
    public enum Landmark
    {
        /// <summary>The Shrouded Isle — storm-veiled elven island; a hidden cliff-port and a world-tree capital.</summary>
        ThalenVeyr,
        /// <summary>The Ashwind Atoll — a living volcano that hinges the Air, Water, and Fire courts (Djinn/Marid/Ifrit).</summary>
        AshwindAtoll,
        /// <summary>The Prism City of Ilyrath — a cavern-basin city that refracts daylight into rivers of colour.</summary>
        Ilyrath,
        /// <summary>The Tidecaller Village — a floating village hiding a bubble-city/water palace in the deep below.</summary>
        TidecallerVillage
    }

    /// <summary>How a landmark is reached. The mechanic — not just the domain — is the design contract; the world
    /// layer turns each of these into the actual entrance object and gate.</summary>
    public enum LandmarkAccess
    {
        /// <summary>Thalen'Veyr: the Veil of Tempests turns away anyone approaching by sea. Enter only by descending
        /// from far above the storm's eye (Flight), or by diving through the crushing deep and rising beneath it.</summary>
        AerialDescentOrDeepDive,
        /// <summary>Ashwind Atoll: a portal hidden in a lava cave at the volcano's foot opens into the caldera.</summary>
        VolcanicCavePortal,
        /// <summary>Ilyrath: the curtain of a colossal waterfall is itself a portal into the cavern-basin behind it.</summary>
        WaterfallPortal,
        /// <summary>Tidecaller: magic tubes drop from a floating surface village to the bubble-city hanging below.</summary>
        FloatingVillageTube
    }

    /// <summary>Static description of a hidden landmark: its names, the elemental courts it hosts, how it is reached,
    /// whether it starts hidden, and whether it hands out the temporary underwater-breathing boon.</summary>
    public readonly struct LandmarkInfo
    {
        public Landmark Landmark { get; }
        public string DisplayName { get; }
        public string Subtitle { get; }
        public LandmarkAccess Access { get; }
        /// <summary>The elemental disciplines whose courts/culture dominate the location (drives faction + creature theming).</summary>
        public IReadOnlyList<Element> ThemeElements { get; }
        /// <summary>True when the landmark does not appear on the chart until the player discovers it.</summary>
        public bool HiddenByDefault { get; }
        /// <summary>True when this location is where a non-water Channeler can pick up a temporary underwater-breathing boon.</summary>
        public bool OffersWaterBreathingBoon { get; }
        public string AccessHint { get; }
        public string Lore { get; }

        public LandmarkInfo(Landmark landmark, string displayName, string subtitle, LandmarkAccess access,
                            Element[] themeElements, bool hiddenByDefault, bool offersWaterBreathingBoon,
                            string accessHint, string lore)
        {
            Landmark = landmark;
            DisplayName = displayName;
            Subtitle = subtitle;
            Access = access;
            ThemeElements = themeElements ?? System.Array.Empty<Element>();
            HiddenByDefault = hiddenByDefault;
            OffersWaterBreathingBoon = offersWaterBreathingBoon;
            AccessHint = accessHint;
            Lore = lore;
        }
    }

    /// <summary>The catalogue of hidden landmarks and the traversal each access mechanic implies. Pure and testable;
    /// the Game layer consumes this to place entrances, gate them, and build interiors.</summary>
    public static class LandmarkCatalog
    {
        public static Landmark[] All => (Landmark[])System.Enum.GetValues(typeof(Landmark));

        public static LandmarkInfo For(Landmark landmark)
        {
            switch (landmark)
            {
                case Landmark.ThalenVeyr:
                    return new LandmarkInfo(landmark, "Thalen'Veyr", "The Shrouded Isle",
                        LandmarkAccess.AerialDescentOrDeepDive,
                        new[] { Element.Air, Element.Water },
                        hiddenByDefault: true, offersWaterBreathingBoon: false,
                        "The Veil of Tempests turns away all who approach by sea. Enter only from far above the storm's eye, or by rising through the crushing deep beneath it.",
                        "An island that appears on no chart, walled by a living storm. Within its cliffs hides the port of Vael'Tirath, and at its heart the world-tree capital Sylthariel — grown, not built. Beneath its roots the planar boundary runs thin.");
                case Landmark.AshwindAtoll:
                    return new LandmarkInfo(landmark, "Ashwind Atoll", "The Dancing-Djinn Caldera",
                        LandmarkAccess.VolcanicCavePortal,
                        new[] { Element.Air, Element.Water, Element.Fire },
                        hiddenByDefault: true, offersWaterBreathingBoon: false,
                        "Find the portal hidden in a lava cave at the volcano's foot; step through into the caldera.",
                        "A living volcano that hinges three elemental courts: Djinn of the open sky, Marid of the drowned grottoes, and Ifrit of the magma foundries. At its throat sits the Heart Vent — a semi-sentient seat of power with three thrones of coral, magma, and living wood.");
                case Landmark.Ilyrath:
                    return new LandmarkInfo(landmark, "Ilyrath", "The Prism City",
                        LandmarkAccess.WaterfallPortal,
                        new[] { Element.Water },
                        hiddenByDefault: true, offersWaterBreathingBoon: false,
                        "Step into the curtain of the great waterfall — it is a portal into the cavern-basin that houses the city.",
                        "Beneath a colossal waterfall hangs a city carved from living stone and rainbows, its districts each bathed in one prism hue that shapes the mood of those beneath it. Behind the falls sleeps a bound primordial source, held by chains the city calls prisms.");
                case Landmark.TidecallerVillage:
                    return new LandmarkInfo(landmark, "Tidecaller Village", "Gateway to the Deep",
                        LandmarkAccess.FloatingVillageTube,
                        new[] { Element.Water },
                        hiddenByDefault: true, offersWaterBreathingBoon: true,
                        "A cluster of floats on the open sea. Magic tubes drop from its underside to a bubble-city below — take a temporary breath of the deep here before descending to the Water capital and the sunken cities.",
                        "A raft-village drifting on the open water, hiding a vast bubble-city and water palace suspended just beneath it. Shimmering tubes ferry folk between surface and deep. It is the jumping-off point for those who cannot breathe water — the way to Tidecrest and the nearby drowned cities.");
                default:
                    return new LandmarkInfo(landmark, landmark.ToString(), "", LandmarkAccess.WaterfallPortal,
                        System.Array.Empty<Element>(), true, false, "", "");
            }
        }

        /// <summary>The domain(s) an entrance for this access mechanic lives in — i.e. how a player physically gets to
        /// the threshold. Aerial+Underwater for the storm isle; Subterranean for the cave portal; Surface otherwise.</summary>
        public static SiteDomain[] ApproachDomains(LandmarkAccess access)
        {
            switch (access)
            {
                case LandmarkAccess.AerialDescentOrDeepDive:
                    return new[] { SiteDomain.Aerial, SiteDomain.Underwater };
                case LandmarkAccess.VolcanicCavePortal:
                    return new[] { SiteDomain.Subterranean };
                case LandmarkAccess.WaterfallPortal:
                    return new[] { SiteDomain.Surface };
                case LandmarkAccess.FloatingVillageTube:
                    return new[] { SiteDomain.Surface };
                default:
                    return new[] { SiteDomain.Surface };
            }
        }

        /// <summary>True when a player arriving from <paramref name="domain"/> can reach this landmark's threshold.</summary>
        public static bool AllowsApproachFrom(Landmark landmark, SiteDomain domain)
        {
            foreach (var d in ApproachDomains(For(landmark).Access))
                if (d == domain) return true;
            return false;
        }

        /// <summary>Every landmark that starts hidden (currently all of them).</summary>
        public static IEnumerable<Landmark> HiddenLandmarks()
        {
            foreach (var l in All)
                if (For(l).HiddenByDefault) yield return l;
        }

        /// <summary>Landmarks whose dominant courts include the given element (for faction/creature theming).</summary>
        public static IEnumerable<Landmark> ThemedTo(Element element)
        {
            foreach (var l in All)
            {
                foreach (var e in For(l).ThemeElements)
                {
                    if (e == element) { yield return l; break; }
                }
            }
        }

        /// <summary>The single landmark that offers the temporary underwater-breathing boon, if any.</summary>
        public static Landmark? WaterBreathingHub()
        {
            foreach (var l in All)
                if (For(l).OffersWaterBreathingBoon) return l;
            return null;
        }
    }
}
