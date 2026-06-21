using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>Flat, JsonUtility-friendly snapshot of the player's progression.</summary>
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
    }

    /// <summary>
    /// Reads and writes a <see cref="SaveData"/> as JSON in the platform's persistent data folder. Used by
    /// <see cref="SaveController"/>; the actual progression lives on <see cref="PlayerInventory"/>.
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "elementborn_save.json";
        private static string FilePath => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static bool Exists => File.Exists(FilePath);

        public static void Save(SaveData data)
        {
            if (data == null) return;
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(data, true)); }
            catch (Exception e) { Debug.LogWarning($"[Elementborn] Save failed: {e.Message}"); }
        }

        public static SaveData Load()
        {
            try { if (File.Exists(FilePath)) return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath)); }
            catch (Exception e) { Debug.LogWarning($"[Elementborn] Load failed: {e.Message}"); }
            return null;
        }

        public static void Delete()
        {
            try { if (File.Exists(FilePath)) File.Delete(FilePath); }
            catch (Exception e) { Debug.LogWarning($"[Elementborn] Delete failed: {e.Message}"); }
        }
    }
}
