using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class StoryEncounterProgressSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot = 0;
        [SerializeField] private bool autoLoadOnStart = false;
        [SerializeField] private bool autoSaveOnApplicationPause = true;
        [SerializeField] private bool autoSaveOnApplicationQuit = true;

        private void Start()
        {
            if (autoLoadOnStart) LoadCurrentSlot();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && autoSaveOnApplicationPause) SaveCurrentSlot();
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnApplicationQuit) SaveCurrentSlot();
        }

        public void SetCurrentSlot(int slot) => currentSlot = Mathf.Max(0, slot);
        public void SaveCurrentSlot() => SaveSlot(currentSlot);
        public void LoadCurrentSlot() => LoadSlot(currentSlot);

        public void SaveSlot(int slot)
        {
            var tracker = StoryEncounterProgressTracker.Ensure();
            var save = new StoryEncounterProgressSaveFile { SlotIndex = Mathf.Max(0, slot) };
            foreach (StoryEncounterRuntimeRecord record in tracker.Records)
            {
                if (record != null) save.Records.Add(record);
            }
            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
            Debug.Log($"Saved {save.Records.Count} story encounter progress record(s) for slot {slot}.");
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                Debug.Log($"No story encounter progress save file exists for slot {slot}: {path}");
                return;
            }
            var save = JsonUtility.FromJson<StoryEncounterProgressSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                Debug.LogWarning($"Failed to parse story encounter progress save file: {path}");
                return;
            }
            StoryEncounterProgressTracker.Ensure().ReplaceRecords(save.Records);
            Debug.Log($"Loaded {save.Records.Count} story encounter progress record(s) for slot {slot}.");
        }

        public void DeleteSlot(int slot)
        {
            string path = GetPath(slot);
            if (File.Exists(path)) File.Delete(path);
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "story_encounters");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_story_encounters.json");
        }
    }
}
