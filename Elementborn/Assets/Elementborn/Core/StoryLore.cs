namespace Elementborn.Core
{
    /// <summary>One of the four primordial Creature Kings: a titan whose stolen power quietly runs the realms
    /// while it lies imprisoned. Each embodies an element and is mirrored by a tameable beast of that element.</summary>
    public readonly struct CreatureKing
    {
        public readonly string Name;        // e.g. "Ignivar"
        public readonly string Title;       // e.g. "the Phoenix Emperor"
        public readonly Element Element;
        public readonly CreatureKind Beast; // the creature kind that echoes this King in the wild
        public readonly string Prison;      // where it is bound
        public readonly string Lore;

        public CreatureKing(string name, string title, Element element, CreatureKind beast, string prison, string lore)
        {
            Name = name; Title = title; Element = element; Beast = beast; Prison = prison; Lore = lore;
        }

        public string FullName => Name + ", " + Title;
    }

    /// <summary>The canonical world story as pure data: the four imprisoned Creature Kings whose stolen power
    /// runs civilization, the Sundering that shattered the once-whole world into elemental realms, and the Great
    /// Betrayal that caged the Kings. The story, dialogue, and codex layers all read from here.</summary>
    public static class StoryLore
    {
        /// <summary>The neutral convergence hub where the four realms meet to trade and treat — and where the
        /// story opens, before the world tips into conflict.</summary>
        public const string CentralCity = "Concord";

        /// <summary>The great spire at Concord's heart, raised to celebrate the fragile peace. Its destruction is
        /// the inciting tragedy.</summary>
        public const string ConvergenceTower = "the Convergence Tower";

        /// <summary>The beloved peace-broker assassinated in the tower blast — the spark of the war to come.</summary>
        public const string DiplomatName = "Ambassador Sera Calderon";
        public const string DiplomatTitle = "Voice of Concord";

        public static readonly CreatureKing Ignivar = new CreatureKing(
            "Ignivar", "the Phoenix Emperor", Element.Fire, CreatureKind.Phoenix,
            "the Emberheart, sealed beneath the volcano's root",
            "First to be caged. The hearths, forges, and warmth of every realm still burn on the embers of his stolen fire.");

        public static readonly CreatureKing Thalassa = new CreatureKing(
            "Thalassa", "the Leviathan Queen", Element.Water, CreatureKind.Tidewarden,
            "the Drowned Vault, far below the tideless deep",
            "Her bound heartbeat is the tide and the rain; let it falter and the wells run dry and the harvests with them.");

        public static readonly CreatureKing Terragor = new CreatureKing(
            "Terragor", "the World-Crown", Element.Earth, CreatureKind.Rhino,
            "the Root Hollow, chained within the mountain's keel",
            "A crowned mountain given breath. The standing stones, the deep roads, and the bones of the land answer to his slow, caged pulse.");

        public static readonly CreatureKing Zephyreon = new CreatureKing(
            "Zephyreon", "the Storm Roc", Element.Air, CreatureKind.Roc,
            "the Hushed Eyrie, fettered above the cloud reaches",
            "The winds, the seasons, and the breath in every chest ride on his caged wings. The storms that scour the borders are his stifled cry.");

        public static readonly CreatureKing[] Kings = { Ignivar, Thalassa, Terragor, Zephyreon };

        /// <summary>The Creature King bound to an element (defaults to Ignivar if somehow unmatched).</summary>
        public static CreatureKing KingOf(Element element)
        {
            foreach (var k in Kings) if (k.Element == element) return k;
            return Ignivar;
        }

        public const string Sundering =
            "Once the world was whole and the elements ran together as one current — the First Convergence. The " +
            "Sundering broke that current, splitting the world into four elemental realms ringed around a single " +
            "neutral city. Most believe the Sundering simply happened. It did not.";

        public const string GreatBetrayal =
            "The realms do not run on faith or fortune. They run on the Creature Kings — four titans bound long ago " +
            "and bled of their power to light the hearths, turn the tides, raise the stones, and stir the winds. The " +
            "prisons are failing now: borders weaken, hybrids and half-elements appear, and the stolen engine that " +
            "everyone depends on is grinding toward collapse.";
    }
}
