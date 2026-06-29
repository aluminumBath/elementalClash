using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureOrphanageRecoverySaveBridge : MonoBehaviour
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

        public void SaveCurrentSlot() => SaveSlot(currentSlot);
        public void LoadCurrentSlot() => LoadSlot(currentSlot);

        public void SaveSlot(int slot)
        {
            var registry = CreatureOrphanageRecoveryRegistry.Ensure();
            var save = new CreatureOrphanageRecoverySaveFile { SlotIndex = Mathf.Max(0, slot) };
            foreach (CreatureOrphanageResidentRecord record in registry.Residents)
            {
                if (record != null)
                {
                    save.Residents.Add(record);
                }
            }

            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
            Debug.Log($"Saved {save.Residents.Count} creature orphanage resident record(s) for slot {slot}.");
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                Debug.Log($"No creature orphanage recovery save file exists for slot {slot}: {path}");
                return;
            }

            var save = JsonUtility.FromJson<CreatureOrphanageRecoverySaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                Debug.LogWarning($"Failed to parse creature orphanage recovery save file: {path}");
                return;
            }

            CreatureOrphanageRecoveryRegistry.Ensure().ReplaceResidents(save.Residents);
            Debug.Log($"Loaded {save.Residents.Count} creature orphanage resident record(s) for slot {slot}.");
        }

        public void DeleteSlot(int slot)
        {
            string path = GetPath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "creature_orphanage_recovery");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_creature_orphanage_recovery.json");
        }
    }
}
