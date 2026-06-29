using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CapitalWorldStateSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot = 0;
        [SerializeField] private bool autoLoadOnStart = false;
        [SerializeField] private bool autoSaveOnApplicationPause = true;
        [SerializeField] private bool autoSaveOnApplicationQuit = true;
        [SerializeField] private bool registerJournalAndMapAfterLoad = true;

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
            var tracker = CapitalWorldStateTracker.Ensure();
            var save = new CapitalWorldStateSaveFile
            {
                SlotIndex = Mathf.Max(0, slot)
            };

            foreach (CapitalRuntimeState state in tracker.RuntimeStates)
            {
                if (state != null)
                {
                    save.RuntimeStates.Add(state);
                }
            }

            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
            Debug.Log($"Saved {save.RuntimeStates.Count} capital world-state record(s) for slot {slot}.");
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                Debug.Log($"No capital world-state save file exists for slot {slot}: {path}");
                return;
            }

            var save = JsonUtility.FromJson<CapitalWorldStateSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                Debug.LogWarning($"Failed to parse capital world-state save file: {path}");
                return;
            }

            var tracker = CapitalWorldStateTracker.Ensure();
            tracker.ReplaceRuntimeStates(save.RuntimeStates);

            if (registerJournalAndMapAfterLoad)
            {
                tracker.RegisterJournalAndMap();
            }

            Debug.Log($"Loaded {save.RuntimeStates.Count} capital world-state record(s) for slot {slot}.");
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
            string dir = Path.Combine(Application.persistentDataPath, "capital_world_state");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_capital_world_state.json");
        }
    }
}
