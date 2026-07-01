namespace Elementborn.Core
{
    /// <summary>
    /// Cosmetic Channeler appearances for the home Wardrobe. APPEARANCE ONLY — selecting a look changes how the
    /// player is rendered and never touches their element, stats, or abilities. The names evoke a style, not an
    /// elemental attunement. Pure/UnityEngine-free (unit-testable); the Game layer loads the model and swaps the mesh.
    /// Looks unlock with progression: Default/Plain from the start, the rest at rising levels.
    /// </summary>
    public enum ChannelerLook { Default, Plain, Air, Water, Fire, Earth, Lava, Steam, Metal, Plant, Blood, Paralysis }

    public static class WardrobeCatalog
    {
        /// <summary>Mapped Resources sub-path (used after a clean asset import; folder + file share the leaf name).</summary>
        public const string ResourceRoot = "Models/Characters/Channeler/";

        public static readonly ChannelerLook[] All =
        {
            ChannelerLook.Default, ChannelerLook.Plain, ChannelerLook.Air, ChannelerLook.Water,
            ChannelerLook.Fire, ChannelerLook.Earth, ChannelerLook.Lava, ChannelerLook.Steam,
            ChannelerLook.Metal, ChannelerLook.Plant, ChannelerLook.Blood, ChannelerLook.Paralysis,
        };

        /// <summary>The look's folder under the mapped Channeler root (folder and model file share this name).</summary>
        public static string FolderOf(ChannelerLook look)
        {
            switch (look)
            {
                case ChannelerLook.Default:   return "Default";
                case ChannelerLook.Plain:     return "Plain";
                case ChannelerLook.Air:       return "Air";
                case ChannelerLook.Water:     return "Water";
                case ChannelerLook.Fire:      return "Fire";
                case ChannelerLook.Earth:     return "Earth";
                case ChannelerLook.Lava:      return "Lava";
                case ChannelerLook.Steam:     return "Steam";
                case ChannelerLook.Metal:     return "Metal";
                case ChannelerLook.Plant:     return "Plant";
                case ChannelerLook.Blood:     return "Blood";
                case ChannelerLook.Paralysis: return "Paralysis";
                default:                      return "Default";
            }
        }

        /// <summary>The model's actual imported FBX leaf name (the assets currently live under Models/Unmapped/).</summary>
        public static string FbxName(ChannelerLook look)
        {
            switch (look)
            {
                case ChannelerLook.Default:   return "Channeler_Hero_3D";
                case ChannelerLook.Plain:     return "Channeler_Hero_None_3";
                case ChannelerLook.Air:       return "Channeler_Hero_Air_3D";
                case ChannelerLook.Water:     return "Channeler_Hero_Water";
                case ChannelerLook.Fire:      return "Channeler_Hero_Fire_3";
                case ChannelerLook.Earth:     return "Channeler_Hero_Earth";
                case ChannelerLook.Lava:      return "Channeler_Hero_Lava_3";
                case ChannelerLook.Steam:     return "Channeler_Hero_Steam";
                case ChannelerLook.Metal:     return "Channeler_Hero_Metal";
                case ChannelerLook.Plant:     return "Channeler_Hero_Plant";
                case ChannelerLook.Blood:     return "Channeler_Hero_Blood";
                case ChannelerLook.Paralysis: return "Channeler_Hero_Paraly";
                default:                      return "Channeler_Hero_3D";
            }
        }

        /// <summary>Resources paths to try (in order) when loading a look's model: the clean mapped path first, then
        /// the as-imported Models/Unmapped/ location, so it resolves whether or not the assets have been re-imported.</summary>
        public static string[] CandidatePaths(ChannelerLook look)
        {
            string folder = FolderOf(look), fbx = FbxName(look);
            return new[]
            {
                ResourceRoot + folder + "/" + folder,
                "Models/Unmapped/" + fbx + "/" + fbx,
                "Models/Characters/Channeler/" + fbx + "/" + fbx,
            };
        }

        /// <summary>Player level at which a look becomes selectable. Default/Plain are always available; the four
        /// classic looks unlock early, the exotic looks later. This gates appearance only.</summary>
        public static int RequiredLevelFor(ChannelerLook look)
        {
            switch (look)
            {
                case ChannelerLook.Default:
                case ChannelerLook.Plain:     return 1;
                case ChannelerLook.Air:
                case ChannelerLook.Water:     return 3;
                case ChannelerLook.Fire:
                case ChannelerLook.Earth:     return 5;
                case ChannelerLook.Steam:     return 8;
                case ChannelerLook.Lava:      return 10;
                case ChannelerLook.Metal:     return 12;
                case ChannelerLook.Plant:     return 14;
                case ChannelerLook.Blood:     return 16;
                case ChannelerLook.Paralysis: return 18;
                default:                      return 1;
            }
        }

        public static bool IsUnlocked(ChannelerLook look, int playerLevel) => playerLevel >= RequiredLevelFor(look);

        public static string DisplayName(ChannelerLook look)
        {
            switch (look)
            {
                case ChannelerLook.Default:   return "Channeler (Default)";
                case ChannelerLook.Plain:     return "Plain Garb";
                case ChannelerLook.Air:       return "Gale Garb";
                case ChannelerLook.Water:     return "Tidewoven Garb";
                case ChannelerLook.Fire:      return "Ember Garb";
                case ChannelerLook.Earth:     return "Stoneweave Garb";
                case ChannelerLook.Lava:      return "Magma Garb";
                case ChannelerLook.Steam:     return "Mistveil Garb";
                case ChannelerLook.Metal:     return "Ironclad Garb";
                case ChannelerLook.Plant:     return "Verdant Garb";
                case ChannelerLook.Blood:     return "Crimson Garb";
                case ChannelerLook.Paralysis: return "Voltaic Garb";
                default:                      return look.ToString();
            }
        }
    }

    /// <summary>
    /// The player's chosen cosmetic look. Gated behind a built Wardrobe AND the look's unlock level, and persisted.
    /// Changing it alters appearance only — it never changes the player's element.
    /// </summary>
    public sealed class Wardrobe
    {
        public ChannelerLook Current { get; private set; } = ChannelerLook.Default;

        /// <summary>Choose a look. Requires the home Wardrobe to be built and the look unlocked at the given level;
        /// returns false (unchanged) otherwise. This never alters the player's element — only their appearance.</summary>
        public bool TrySelect(ChannelerLook look, bool wardrobeBuilt, int playerLevel)
        {
            if (!wardrobeBuilt) return false;
            if (!WardrobeCatalog.IsUnlocked(look, playerLevel)) return false;
            Current = look;
            return true;
        }

        public string Save() => Current.ToString();

        public void Restore(string s)
        {
            if (!string.IsNullOrEmpty(s) && System.Enum.TryParse(s, out ChannelerLook l)) Current = l;
        }
    }
}
