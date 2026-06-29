using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class FactionReputationSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot = 0;
        [SerializeField] private bool autoLoadOnStart = false;
        [SerializeField] private bool autoSaveOnApplicationPause = true;
        [SerializeField] private bool autoSaveOnApplicationQuit = true;

        private void Start()
        {
            if (autoLoadOnStart)
            {
                LoadCurrentSlot();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && autoSaveOnApplicationPause)
            {
                SaveCurrentSlot();
            }
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnApplicationQuit)
            {
                SaveCurrentSlot();
            }
        }

        public void SetCurrentSlot(int slot)
        {
            currentSlot = Mathf.Max(0, slot);
        }

        public void SaveCurrentSlot()
        {
            SaveSlot(currentSlot);
        }

        public void LoadCurrentSlot()
        {
            LoadSlot(currentSlot);
        }

        public void SaveSlot(int slot)
        {
            var tracker = FactionReputationTracker.Ensure();
            var save = new FactionReputationSaveFile { SlotIndex = Mathf.Max(0, slot) };

            foreach (var record in tracker.Reputations)
            {
                save.Reputations.Add(record);
            }

            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                return;
            }

            var save = JsonUtility.FromJson<FactionReputationSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            FactionReputationTracker.Clear();

            foreach (var record in save.Reputations)
            {
                var runtime = FactionReputationTracker.Get(record.Faction);
                runtime.Reputation = record.Reputation;
                runtime.LastReason = record.LastReason;
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "factions");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_factions.json");
        }
    }
}
