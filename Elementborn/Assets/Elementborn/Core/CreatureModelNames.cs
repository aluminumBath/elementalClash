using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// Resolves where a creature's display model lives under <c>Resources</c>. Pure (no Unity) so the mapping is
    /// unit-tested; <see cref="T:Elementborn.Game.CreatureModelLibrary"/> does the actual <c>Resources.Load</c>.
    /// By default a kind maps to a prefab named exactly after the enum
    /// (e.g. <c>Resources/Models/Creatures/Phoenix</c>). If your model files are named differently — say by colour
    /// and shape — add an entry to <see cref="Aliases"/> pointing the kind at your file name. That single line is
    /// the only code change needed to bind a model to a creature.
    /// </summary>
    public static class CreatureModelNames
    {
        /// <summary>Resources sub-path (relative to any <c>Resources/</c> folder) that all creature models live under.</summary>
        public const string ResourceRoot = "Models/Creatures/";

        /// <summary>
        /// Kind → model file name, for models <i>not</i> named after the enum. Empty by default. Example:
        /// <c>{ CreatureKind.Phoenix, "ember_bird_red" }</c> binds Phoenix to
        /// <c>Resources/Models/Creatures/ember_bird_red</c>.
        /// </summary>
        public static readonly IReadOnlyDictionary<CreatureKind, string> Aliases = new Dictionary<CreatureKind, string>
        {
            // CreatureKind            ->  model at Resources/Models/Creatures/<value>(.fbx). The "<name>/<name>"
            // form keeps each model (fbx + its textures) in its own folder so Meshy texture files can't collide.
            // Edit freely — this map is the single source of truth; unmapped kinds fall back to a primitive.
            { CreatureKind.WaterDragon, "Azure_Wave_Dragon/Azure_Wave_Dragon" },
            { CreatureKind.Phoenix,     "Fire_Phoenix/Fire_Phoenix" },
            { CreatureKind.Thunderbird, "Thunderbird/Thunderbird" },
            { CreatureKind.Roc,         "Giant_Eagle/Giant_Eagle" },
            { CreatureKind.Dog,         "Patchwork_Pup/Patchwork_Pup" },
            { CreatureKind.Spider,      "Antler_Spider_Creature/Antler_Spider_Creature" },
            { CreatureKind.Crab,        "Coral_Crab_Spider/Coral_Crab_Spider" },
            { CreatureKind.Snake,       "Teal_Serpent/Teal_Serpent" },
            { CreatureKind.EarthCat,    "Leaf_Cub/Leaf_Cub" },
            { CreatureKind.Horse,       "Blue_Dino_Mount/Blue_Dino_Mount" },
            { CreatureKind.Goldkoi,     "Blue_Gold_Tuna/Blue_Gold_Tuna" },
            { CreatureKind.Skimfin,     "Teal_Fantasy_Fish/Teal_Fantasy_Fish" },
            { CreatureKind.Gillcloak,   "Abyss_Angler/Abyss_Angler" },
            { CreatureKind.Tidewarden,  "Purple_Kraken/Purple_Kraken" },
            { CreatureKind.Direstalker, "Shadow_Wolf/Shadow_Wolf" },
            { CreatureKind.Skytyrant,   "Storm_Wyvern/Storm_Wyvern" },
            { CreatureKind.Ridgewing,   "Blue_Fantasy_Bird/Blue_Fantasy_Bird" },
            { CreatureKind.Glidewisp,   "Fawn_Sprite/Fawn_Sprite" },
            // Generated across the Meshy batches (see docs/MESHY_PROMPTS.md) — every CreatureKind now has a model.
            // Extract each zip into a folder named as below (strip the "Meshy_AI_" prefix and the timestamp/suffix).
            { CreatureKind.FireDragon,       "Ember_Dragon/Ember_Dragon" },
            { CreatureKind.Eel,              "Current_Eel/Current_Eel" },
            { CreatureKind.Mermaid,          "Tide_Mermaid/Tide_Mermaid" },
            { CreatureKind.EarthMole,        "EarthMole_Stone_Mole/EarthMole_Stone_Mole" },
            { CreatureKind.AirDragonfly,     "AirDragonfly_Gale_Dragonfly/AirDragonfly_Gale_Dragonfly" },
            { CreatureKind.AirJellyfish,     "AirJellyfish_Sky_Jellyfish/AirJellyfish_Sky_Jellyfish" },
            { CreatureKind.WaterCat,         "WaterCat_Wave_Cat/WaterCat_Wave_Cat" },
            { CreatureKind.IceCat,           "IceCat/IceCat" },
            { CreatureKind.ElectricSquirrel, "Spark_Squirrel/Spark_Squirrel" },
            { CreatureKind.Monkey,           "Canopy_Monkey/Canopy_Monkey" },
            { CreatureKind.Crocodile,        "Marsh_Crocodile/Marsh_Crocodile" },
            { CreatureKind.Rhino,            "Boulder_Rhino/Boulder_Rhino" },
            { CreatureKind.Tiger,            "Tigris_Prowler/Tigris_Prowler" },
            { CreatureKind.Skyotter,         "Skyotter/Skyotter" },
            // New purchasable mounts (elemental spread) — assets currently under Models/Unmapped/ until a clean import.
            { CreatureKind.BoneBehemoth,     "Bonebound_Behemoth/Bonebound_Behemoth" },
            { CreatureKind.AncientStag,      "Ancient_Stag/Ancient_Stag" },
            { CreatureKind.CoralLeviathan,   "Coral_Whale_Monster/Coral_Whale_Monster" },
            { CreatureKind.EmberKite,        "Embercrest_Kitebeast/Embercrest_Kitebeast" },
            { CreatureKind.AzureKnight,      "Azurewing_Knight/Azurewing_Knight" },
            { CreatureKind.EarthDragon,      "Emerald_Dragon/Emerald_Dragon" },
            { CreatureKind.AirDragon,        "Blue_Dragon/Blue_Dragon" },
            { CreatureKind.StormWolf,        "Storm_Shadow_Wolf/Storm_Shadow_Wolf" },
            { CreatureKind.VoltWolf,         "Lightning_Dark_Wolf/Lightning_Dark_Wolf" },
        };

        /// <summary>The bare file name (no path, no extension) of a kind's model prefab.</summary>
        public static string ResourceName(CreatureKind kind) =>
            Aliases.TryGetValue(kind, out var name) && !string.IsNullOrEmpty(name) ? name : kind.ToString();

        /// <summary>The full Resources path passed to <c>Resources.Load</c> for a kind's model.</summary>
        public static string ResourcePath(CreatureKind kind) => ResourceRoot + ResourceName(kind);

        /// <summary>Resources paths to try (in order) for a kind's model: the canonical Creatures path first, then
        /// the as-imported Models/Unmapped/ location, so newly-added assets resolve before a clean re-import.</summary>
        public static string[] CandidatePaths(CreatureKind kind)
        {
            string name = ResourceName(kind);
            return new[] { ResourceRoot + name, "Models/Unmapped/" + name };
        }

        /// <summary>A per-kind display-size multiplier applied to the attached model. 1.0 for everything by default
        /// (no change to the 32 existing creatures); the newest mounts get tuned starting sizes so an apex reads big
        /// and a wolf reads small. These are starting values — easy to nudge once you see them in-scene.</summary>
        public static float DisplayScale(CreatureKind kind)
        {
            switch (kind)
            {
                // Apex — clearly the largest things you'll meet.
                case CreatureKind.CoralLeviathan: return 1.7f;
                case CreatureKind.BoneBehemoth:   return 1.6f;
                // Dragons & large mounts — bigger than a person, not towering.
                case CreatureKind.EarthDragon:    return 1.3f;
                case CreatureKind.AirDragon:      return 1.3f;
                case CreatureKind.AncientStag:    return 1.25f;
                // Flyers — a touch larger so wings read.
                case CreatureKind.EmberKite:      return 1.15f;
                case CreatureKind.AzureKnight:    return 1.15f;
                // Wolf companions — roughly person-height.
                case CreatureKind.StormWolf:
                case CreatureKind.VoltWolf:       return 1.0f;
                default:                          return 1.0f;
            }
        }
    }
}
