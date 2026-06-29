using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class WorldEventSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot;
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
            var tracker = WorldEventTracker.Ensure();
            var save = new WorldEventSaveFile { SlotIndex = Mathf.Max(0, slot) };
            foreach (var record in tracker.Records)
            {
                if (record != null) save.Records.Add(record);
            }
            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path)) return;
            var save = JsonUtility.FromJson<WorldEventSaveFile>(File.ReadAllText(path));
            if (save == null) return;
            var tracker = WorldEventTracker.Ensure();
            tracker.Clear();
            foreach (var record in save.Records) tracker.Import(record);
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "world_events");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_world_events.json");
        }
    }
}
