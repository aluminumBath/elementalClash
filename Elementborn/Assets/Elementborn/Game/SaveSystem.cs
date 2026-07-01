using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>Flat, JsonUtility-friendly snapshot of the player's progression and chosen character.</summary>
    [Serializable]
    public class SaveData
    {
        public int silver;
        public int ruby;
        public int emerald;
        public int sapphire;
        public int diamond;

        public List<string> lureKinds = new List<string>();
        public List<int> lureCounts = new List<int>();
        public List<string> ownedKinds = new List<string>();
        public List<string> ownedVehicles = new List<string>();

        public bool hasHouse;
        public float houseX;
        public float houseY;
        public float houseZ;
        public List<string> houseAdditions = new List<string>();
        public string wardrobeLook = "";
        public double gardenAccrued = 0;

        public string playerElement = "";
        public bool isConfluence;

        // Character (so a saved run can skip creation and rebuild the exact loadout).
        public bool created;
        public int revealTier;
        public List<string> loadoutElements = new List<string>();
        public List<string> loadoutSubArts = new List<string>();
        public string loadoutWeapon = "";

        public long savedUnixSeconds;

        public List<string> itemIds = new List<string>();
        public List<int> itemCounts = new List<int>();

        public List<string> questIds = new List<string>();
        public List<int> questStatuses = new List<int>();
        public List<string> questProgress = new List<string>();   // one CSV of objective counts per quest

        public int level = 1;
        public int xp;

        public int perkPoints;
        public List<string> perkIds = new List<string>();
        public List<int> perkRanks = new List<int>();

        // Grimoire discovery: parallel lists mirroring GrimoireProgress.ToSave() ("Section:Id" -> tier int).
        public List<string> grimoireKeys = new List<string>();
        public List<int> grimoireTiers = new List<int>();

        // Player guild: a persistent, ranked roster (id/name + parallel member-id/rank lists). Not an NPC faction.
        public string guildId = "";
        public string guildName = "";
        public List<string> guildMemberIds = new List<string>();
        public List<int> guildMemberRanks = new List<int>();

        public List<string> discoveredElements = new List<string>();

        // Map: discovered leyline rifts (ids) + the local player's location-sharing opt-in.
        public List<string> discoveredRifts = new List<string>();
        public bool shareLocation;

        // Checkpoints: activated shrine ids + the active respawn anchor.
        public List<string> activatedCheckpoints = new List<string>();
        public string activeCheckpoint = "";

        // Achievements: parallel lists mirroring AchievementProgress.ToSave() ("metric|param" -> count).
        public List<string> achievementKeys = new List<string>();
        public List<int> achievementCounts = new List<int>();

        // Equipment: worn item id per slot (enum order; "" = empty).
        public List<string> equippedSlots = new List<string>();
        public List<string> equippedEnchants = new List<string>();   // element name per slot, enum order ("" = none)

        public int storyChapter;   // StoryChapter index — campaign progress
        public int storyEnding;    // StoryEnding index — 0 = none chosen yet

        // Summon Beacon (gacha): the two summon resources plus per-banner pity state (parallel lists, by banner id).
        public int summonSigils;
        public int summonMotes;
        public bool summonSeeded;   // has the one-time starter Sigil grant been applied?
        public List<string> summonBannerIds = new List<string>();
        public List<int> summonPity = new List<int>();
        public List<int> summonGuaranteed = new List<int>();   // 1 = next Legendary is a guaranteed featured
        public List<int> summonTotalPulls = new List<int>();

        // Summon Beacon lifetime stats (all banners): pulls, per-tier counts, featured wins, Sigils spent, Motes earned.
        public int summonStatPulls;
        public int summonStatRare;
        public int summonStatEpic;
        public int summonStatLegendary;
        public int summonStatFeaturedWins;
        public int summonStatSigilsSpent;
        public int summonStatMotesEarned;

        // Recent-pulls log (newest first): the last few notable summons (Epic+). Parallel lists.
        public List<string> summonHistKinds = new List<string>();
        public List<int> summonHistRarity = new List<int>();
        public List<int> summonHistFeatured = new List<int>();
        public List<string> summonHistBanner = new List<string>();
        public List<string> summonHistTicks = new List<string>();   // UTC ticks as string (JsonUtility-safe)

        // Daily free summon.
        public bool summonDailyClaimed;
        public string summonDailyTicks = "0";   // UTC ticks of last claim, as string
        public int summonLoginStreak;           // consecutive-day claim streak
    }

    /// <summary>
    /// Reads and writes <see cref="SaveData"/> as JSON in the platform's persistent data folder, with support
    /// for multiple save slots. <see cref="CurrentSlot"/> selects which file the parameterless calls use; slot 0
    /// keeps the original filename so existing saves still load. Used by <see cref="SaveController"/> and
    /// <see cref="SaveSlotController"/>; the live progression lives on <see cref="PlayerInventory"/>.
    /// </summary>
    public static class SaveSystem
    {
        public const int SlotCount = 3;

        /// <summary>The slot the parameterless Save/Load/Exists/Delete operate on.</summary>
        public static int CurrentSlot { get; set; } = 0;

        private static string PathFor(int slot) => slot <= 0
            ? Path.Combine(Application.persistentDataPath, "elementborn_save.json")
            : Path.Combine(Application.persistentDataPath, $"elementborn_save_{slot}.json");

        // ---- current-slot convenience (back-compatible API) -------------------------------
        public static bool Exists => ExistsSlot(CurrentSlot);
        public static void Save(SaveData data) => SaveSlot(CurrentSlot, data);
        public static SaveData Load() => LoadSlot(CurrentSlot);
        public static void Delete() => DeleteSlot(CurrentSlot);

        // ---- explicit slot operations -----------------------------------------------------
        public static bool ExistsSlot(int slot) => File.Exists(PathFor(slot));

        public static void SaveSlot(int slot, SaveData data)
        {
            if (data == null) return;
            data.savedUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            try { File.WriteAllText(PathFor(slot), JsonUtility.ToJson(data, true)); }
            catch (Exception e) { Debug.LogWarning($"[Elementborn] Save failed: {e.Message}"); }
        }

        public static SaveData LoadSlot(int slot)
        {
            try { if (File.Exists(PathFor(slot))) return JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFor(slot))); }
            catch (Exception e) { Debug.LogWarning($"[Elementborn] Load failed: {e.Message}"); }
            return null;
        }

        public static void DeleteSlot(int slot)
        {
            try { if (File.Exists(PathFor(slot))) File.Delete(PathFor(slot)); }
            catch (Exception e) { Debug.LogWarning($"[Elementborn] Delete failed: {e.Message}"); }
        }
    }
}
