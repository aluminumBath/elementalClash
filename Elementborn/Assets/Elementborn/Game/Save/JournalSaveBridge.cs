using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class JournalSaveBridge : MonoBehaviour
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
            var journal = PlayerJournalTracker.Ensure();
            var save = new JournalSaveFile { SlotIndex = Mathf.Max(0, slot) };

            foreach (var entry in journal.Entries)
            {
                save.Entries.Add(entry);
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

            var save = JsonUtility.FromJson<JournalSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            PlayerJournalTracker.Clear();

            foreach (var entry in save.Entries)
            {
                var runtime = PlayerJournalTracker.AddOrUpdateEntry(
                    entry.EntryId,
                    entry.Type,
                    entry.Title,
                    entry.Body,
                    entry.Region,
                    entry.RelatedId,
                    markNew: entry.IsNew);

                runtime.IsPinned = entry.IsPinned;
                runtime.IsComplete = entry.IsComplete;
                runtime.IsNew = entry.IsNew;
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "journal");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_journal.json");
        }
    }
}
