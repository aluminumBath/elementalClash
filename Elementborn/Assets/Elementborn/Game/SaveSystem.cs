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
