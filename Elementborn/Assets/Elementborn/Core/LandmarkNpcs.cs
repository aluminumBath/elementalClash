namespace Elementborn.Core
{
    /// <summary>The key named characters of the four hidden landmarks, adapted from the source modules. Each is
    /// keyed so the dialogue/quest/spawn layers can reference them; the roster stays pure data (fits the modding
    /// pass) and can grow. D&D classes are mapped onto our disciplines per the agreed table.</summary>
    public enum LandmarkNpc
    {
        // Thalen'Veyr — the Shrouded Isle
        GaiusValmora, WelVana, AliefirlKaida, VarrandurSunvein, MorgaMarlTongued, JudeSanguel,
        // Ashwind Atoll — the tri-elemental caldera
        Zephra, KhalimTidebinder, SultanAshkar, OldDiverMira,
        // Ilyrath — the Prism City
        AlaricBlackthorn, VeyraShardSight, PrismKeeperSolenne, ThalenMirawen, AstraethBoundSource,
        // Tidecaller Village — gateway to the deep
        NaiaTubewright
    }

    /// <summary>The gameplay role a landmark NPC plays.</summary>
    public enum NpcAlignment { Ruler, Faction, Scholar, Merchant, Healer, Guide, Gatekeeper, Antagonist }

    /// <summary>A landmark NPC's identity, home, mapped discipline, and hook. <see cref="IsChanneler"/> distinguishes
    /// Channelers from weapon/martial or non-magical figures (for whom <see cref="Element"/> is only thematic).</summary>
    public readonly struct LandmarkNpcInfo
    {
        public readonly LandmarkNpc Id;
        public readonly string Name;
        public readonly string Title;
        public readonly Landmark Home;
        public readonly NpcAlignment Alignment;
        public readonly Element Element;   // mapped/thematic discipline
        public readonly SubArt SubArt;     // advanced art where fitting, else None
        public readonly bool IsChanneler;  // false for weapon/martial or non-magical figures
        public readonly string Blurb;

        public LandmarkNpcInfo(LandmarkNpc id, string name, string title, Landmark home, NpcAlignment alignment,
                               Element element, SubArt subArt, bool isChanneler, string blurb)
        {
            Id = id; Name = name; Title = title; Home = home; Alignment = alignment;
            Element = element; SubArt = subArt; IsChanneler = isChanneler; Blurb = blurb;
        }
    }

    public static class LandmarkNpcCatalog
    {
        public static LandmarkNpc[] All => (LandmarkNpc[])System.Enum.GetValues(typeof(LandmarkNpc));

        public static LandmarkNpcInfo For(LandmarkNpc id)
        {
            switch (id)
            {
                // --- Thalen'Veyr ---
                case LandmarkNpc.GaiusValmora:
                    return new LandmarkNpcInfo(id, "Gaius Valmora", "Stormwarden Prime", Landmark.ThalenVeyr,
                        NpcAlignment.Ruler, Element.Air, SubArt.None, true,
                        "Master of the Stormwarden Spire and keeper of the Returning Tides; he alone knows the secret storm-eye way onto the isle.");
                case LandmarkNpc.WelVana:
                    return new LandmarkNpcInfo(id, "Wel Vana", "Archdruid of Aelorwyn", Landmark.ThalenVeyr,
                        NpcAlignment.Faction, Element.Earth, SubArt.Verdancy, true,
                        "Tends the world-tree Aelorwyn and reads the isle's health in its darkening leaves.");
                case LandmarkNpc.AliefirlKaida:
                    return new LandmarkNpcInfo(id, "Aliefirl Kaida", "Keeper of the Archives", Landmark.ThalenVeyr,
                        NpcAlignment.Scholar, Element.Air, SubArt.None, true,
                        "Warden of the Umbral Archive, where the records of the Shrouding lie sealed under the world-tree.");
                case LandmarkNpc.VarrandurSunvein:
                    return new LandmarkNpcInfo(id, "Varrandur Sunvein", "Keeper of the Golden Glade", Landmark.ThalenVeyr,
                        NpcAlignment.Merchant, Element.Fire, SubArt.None, true,
                        "Runs the Golden Glade's flame-lit baths, where a traveller can find anything — or anyone — for the right price.");
                case LandmarkNpc.MorgaMarlTongued:
                    return new LandmarkNpcInfo(id, "Morga the Marl-Tongued", "Siren of the Murkpool", Landmark.ThalenVeyr,
                        NpcAlignment.Gatekeeper, Element.Water, SubArt.None, true,
                        "An ancient siren in the glade's lowest pool; she alone knows the drowned way to the crystal prison, and won't share it cheaply.");
                case LandmarkNpc.JudeSanguel:
                    return new LandmarkNpcInfo(id, "Jude (Jiudani Sanguel)", "The Shadow Weaver", Landmark.ThalenVeyr,
                        NpcAlignment.Antagonist, Element.Water, SubArt.SanguineGrip, true,
                        "A vampire who has hunted the isle's old bloodlines for centuries, gathering their sleeping power at the Shadowwell.");

                // --- Ashwind Atoll ---
                case LandmarkNpc.Zephra:
                    return new LandmarkNpcInfo(id, "Zephra, Voice of the Updraft", "Djinn Overlord", Landmark.AshwindAtoll,
                        NpcAlignment.Ruler, Element.Air, SubArt.Flight, true,
                        "Skyborne ruler of the caldera; she speaks the law on the wind and issues her edicts from the Heart Vent.");
                case LandmarkNpc.KhalimTidebinder:
                    return new LandmarkNpcInfo(id, "Khalim Tidebinder", "Marid Steward", Landmark.AshwindAtoll,
                        NpcAlignment.Faction, Element.Water, SubArt.None, true,
                        "Elder of the Court of Coral who reads the volcano's moods in its currents and keeps the tide-ledgers.");
                case LandmarkNpc.SultanAshkar:
                    return new LandmarkNpcInfo(id, "Sultan Ashkar", "Ifrit Sultan, Hammer of the Hearth", Landmark.AshwindAtoll,
                        NpcAlignment.Faction, Element.Fire, SubArt.Magmacraft, true,
                        "Commands the Sulfur Foundry's legions and believes, loudly, that the strongest tribe should rule the Atoll.");
                case LandmarkNpc.OldDiverMira:
                    return new LandmarkNpcInfo(id, "Old Diver Mira", "Salvager", Landmark.AshwindAtoll,
                        NpcAlignment.Guide, Element.Water, SubArt.None, false,
                        "A mortal salvager who knows the secret currents and the hidden ways into the Boiling Grottoes.");

                // --- Ilyrath ---
                case LandmarkNpc.AlaricBlackthorn:
                    return new LandmarkNpcInfo(id, "Alaric Blackthorn", "Headmaster, the Last Founder", Landmark.Ilyrath,
                        NpcAlignment.Ruler, Element.Air, SubArt.None, true,
                        "Ancient and kindly archmage of Blackthorn Academy; the only one who remembers House Null — and what sleeps beneath the school.");
                case LandmarkNpc.VeyraShardSight:
                    return new LandmarkNpcInfo(id, "Veyra Shard-Sight", "Keeper of the Violet Archives", Landmark.Ilyrath,
                        NpcAlignment.Scholar, Element.Air, SubArt.None, true,
                        "A brilliant, unstable information-broker whose cracked-amethyst eyes see several moments at once; she watches the failing seal.");
                case LandmarkNpc.PrismKeeperSolenne:
                    return new LandmarkNpcInfo(id, "Solenne", "High Prism Keeper", Landmark.Ilyrath,
                        NpcAlignment.Ruler, Element.Air, SubArt.None, true,
                        "Rarely seen, always feared; she maintains the rituals that hold the six chromatic chains fast.");
                case LandmarkNpc.ThalenMirawen:
                    return new LandmarkNpcInfo(id, "Thalen Mirawen", "Arch-Calmer of the Sanctuary", Landmark.Ilyrath,
                        NpcAlignment.Healer, Element.Water, SubArt.Steamcraft, true,
                        "Runs the Sanctuary of Returning Breath, healing corrupted creatures; quietly burdened by the Green Prism's weakening.");
                case LandmarkNpc.AstraethBoundSource:
                    return new LandmarkNpcInfo(id, "Astraeth", "The Bound Source", Landmark.Ilyrath,
                        NpcAlignment.Antagonist, Element.Water, SubArt.None, false,
                        "A primordial consciousness of emotion and raw magic, chained behind the waterfall by the six prisms; it stirs as the seventh chain fails.");

                // --- Tidecaller Village ---
                case LandmarkNpc.NaiaTubewright:
                    return new LandmarkNpcInfo(id, "Naia the Tubewright", "Tidecaller Bubblewright", Landmark.TidecallerVillage,
                        NpcAlignment.Merchant, Element.Water, SubArt.None, true,
                        "A Marid who tends the magic tubes and brews Tideglass Draughts, sending non-water folk down to the bubble-city and the sunken cities beyond.");

                default:
                    return new LandmarkNpcInfo(id, id.ToString(), "", Landmark.ThalenVeyr, NpcAlignment.Guide,
                        Element.Water, SubArt.None, false, "");
            }
        }

        /// <summary>Every named NPC whose home is the given landmark.</summary>
        public static System.Collections.Generic.IEnumerable<LandmarkNpc> InLandmark(Landmark landmark)
        {
            foreach (var npc in All)
                if (For(npc).Home == landmark) yield return npc;
        }
    }
}
