using System.Collections.Generic;

namespace Elementborn.Core
{
    // The non-creature counterparts to CreatureModelNames: which asset each game entity uses. Pure,
    // edit-here-only, with a primitive fallback when an entry is missing or its file isn't present. Values use
    // the "<name>/<name>" form so each model keeps its own textures in its own folder. The Game-layer
    // ModelLibrary loads/attaches these. (Creatures live in CreatureModelNames.)

    /// <summary>Guide NPC → humanoid model (from the Meshy batch).</summary>
    public static class NpcModelNames
    {
        public const string ResourceRoot = "Models/Npcs/";
        public static readonly IReadOnlyDictionary<GuideNpcId, string> Aliases = new Dictionary<GuideNpcId, string>
        {
            { GuideNpcId.Willow, "Verdant_Dryad/Verdant_Dryad" },             // nature finder with a menagerie
            { GuideNpcId.Kiana,  "Azure_Water_Mage/Azure_Water_Mage" },       // regal water keeper of Tideholt
            { GuideNpcId.Parfa,  "Steamwright_Adventurer/Steamwright_Adventurer" }, // forge-town locator/merchant
        };
        public static string ResourceName(GuideNpcId id) => Aliases.TryGetValue(id, out var n) ? n : id.ToString();
        public static string ResourcePath(GuideNpcId id) => ResourceRoot + ResourceName(id);
    }

    /// <summary>Willow's sidekick pets → models (Parrot is the rigged raven; all five are now bound).</summary>
    public static class SidekickModelNames
    {
        public const string ResourceRoot = "Models/Sidekicks/";
        public static readonly IReadOnlyDictionary<WillowSidekick, string> Aliases = new Dictionary<WillowSidekick, string>
        {
            { WillowSidekick.Gunnar,   "Moss_Wolf/Moss_Wolf" },                       // her rock-channeling direwolf
            { WillowSidekick.Parrot,   "Raven_Parrot/Raven_Parrot" },              // Meshy biped raven-parrot, rigged, 5 baked clips
            { WillowSidekick.Blobfish,  "Lure_Fish/Lure_Fish" },
            { WillowSidekick.Mushroom,  "Luminescent_Mushroom/Luminescent_Mushroom" },
            { WillowSidekick.Chameleon, "Prism_Chameleon/Prism_Chameleon" },
        };
        public static string ResourceName(WillowSidekick s) => Aliases.TryGetValue(s, out var n) ? n : s.ToString();
        public static string ResourcePath(WillowSidekick s) => ResourceRoot + ResourceName(s);
    }

    /// <summary>Parfa's two bickering frogs (the air-vs-water accord puzzle). Static Meshy models, so a primitive
    /// fallback shows until they are imported. Hurricane = the air-storm frog, Steam = the water-heat frog.</summary>
    public static class FrogModelNames
    {
        public const string ResourceRoot = "Models/Npcs/";
        public const string Hurricane = ResourceRoot + "Hurricane_Frog/Hurricane_Frog";
        public const string Steam     = ResourceRoot + "Steam_Frog/Steam_Frog";
    }

    /// <summary>Weapon pickups → gear model. Only None lacks a model now → primitive fallback.</summary>
    public static class WeaponModelNames
    {
        public const string ResourceRoot = "Models/Weapons/";
        public static readonly IReadOnlyDictionary<WeaponType, string> Aliases = new Dictionary<WeaponType, string>
        {
            { WeaponType.Sword,   "Emberblade/Emberblade" },
            { WeaponType.LongBow, "Gilded_Arc_Bow/Gilded_Arc_Bow" },
            { WeaponType.Shield,  "Azure_Aegis/Azure_Aegis" },
            { WeaponType.Hammer,  "Stormcleaver_Axe/Stormcleaver_Axe" },  // closest heavy two-hander in the batch
            { WeaponType.Dagger,  "Fang_Dagger/Fang_Dagger" },
            { WeaponType.Sai,     "Twin_Sai/Twin_Sai" },
        };
        public static string ResourceName(WeaponType t) => Aliases.TryGetValue(t, out var n) ? n : t.ToString();
        public static string ResourcePath(WeaponType t) => ResourceRoot + ResourceName(t);
    }

    /// <summary>The third-person player model. A rigged humanoid (skinned mesh + Animator) is preferred; the
    /// static mesh is the fallback until one exists.</summary>
    public static class PlayerModelNames
    {
        public const string ResourceRoot = "Models/Characters/";
        public const string Model = "Windborne_Traveler/Windborne_Traveler"; // static fallback
        public const string Rigged = "PlayerRigged/PlayerRigged";            // skinned humanoid prefab w/ Animator
        public static string ResourcePath() => ResourceRoot + Model;
        public static string RiggedPath() => ResourceRoot + Rigged;
    }

    /// <summary>Catalog item id → a world model (for items placed/dropped in the world via WorldItemPickup).
    /// Items with a fitting Meshy model are mapped; the rest fall back to a primitive.</summary>
    public static class ItemModelNames
    {
        public const string ResourceRoot = "Models/Items/";
        public static readonly IReadOnlyDictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "ember_shard",     "Emberstone_Gem/Emberstone_Gem" },
            { "river_pearl",     "Pearl_Oyster/Pearl_Oyster" },
            { "old_relic",       "Triskelion_Disc/Triskelion_Disc" },
            { "elemental_charm", "Prismatic_Helix_Gem/Prismatic_Helix_Gem" },
            { "healing_tonic",   "Healing_Tonic/Healing_Tonic" },
            { "stamina_draught", "Stamina_Draught/Stamina_Draught" },
            { "elixir_of_vigor", "Vigor_Elixir/Vigor_Elixir" },
            { "ore_marrow_bone", "Ore_Marrow_Bone/Ore_Marrow_Bone" },
            { "sunflower_seeds", "Sunflower_Seeds/Sunflower_Seeds" },
            { "tough_leather",   "Cured_Leather/Cured_Leather" },
        };
        public static string ResourceName(string itemId) => itemId != null && Aliases.TryGetValue(itemId, out var n) ? n : itemId;
        public static string ResourcePath(string itemId) => itemId == null ? null : ResourceRoot + ResourceName(itemId);
    }

    /// <summary>Structure / set-dressing / VFX models in the batch, for placing in scenes by hand. These are
    /// world props, not bound to a runtime system — this is a reference registry, not wiring.</summary>
    public static class PropCatalog
    {
        public const string ResourceRoot = "Models/Props/";
        public static readonly IReadOnlyDictionary<string, string> Props = new Dictionary<string, string>
        {
            { "rift_portal",      "Azure_Arc_Portal" },
            { "checkpoint_spire", "Azure_Crystal_Spire" },
            { "throne",           "Throne_of_the_Crystal" },
            { "vine_gate",        "Vine_Gate" },
            { "mushroom_grove",   "Glowcap_Grove" },
            { "treasure_chest",   "Treasure_Chest" },
            { "banner",           "Azure_Ornate_Banner" },
            { "crystal_pool",     "Emerald_Cavern_Pool" },
            { "radiant_tree",     "Radiant_Purple_Tree" },
            { "coral_garden",     "Underwater_Coral_Garden" },
            { "ouroboros",        "Ouroboros" },
        };
        public static string ResourcePath(string key) => Props.TryGetValue(key, out var n) ? ResourceRoot + n : null;
    }
}
