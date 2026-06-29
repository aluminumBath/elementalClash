using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class QuestChainSaveBridge : MonoBehaviour
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
            var director = QuestChainDirector.Ensure();
            var save = new QuestChainSaveFile
            {
                SlotIndex = Mathf.Max(0, slot)
            };

            foreach (QuestChainRuntimeRecord record in director.RuntimeRecords)
            {
                if (record != null)
                {
                    save.RuntimeRecords.Add(record);
                }
            }

            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
            Debug.Log($"Saved {save.RuntimeRecords.Count} quest-chain record(s) for slot {slot}.");
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                Debug.Log($"No quest-chain save file exists for slot {slot}: {path}");
                return;
            }

            var save = JsonUtility.FromJson<QuestChainSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                Debug.LogWarning($"Failed to parse quest-chain save file: {path}");
                return;
            }

            QuestChainDirector.Ensure().ReplaceRuntimeRecords(save.RuntimeRecords);
            Debug.Log($"Loaded {save.RuntimeRecords.Count} quest-chain record(s) for slot {slot}.");
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
            string dir = Path.Combine(Application.persistentDataPath, "quest_chains");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_quest_chains.json");
        }
    }
}
