using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureBondingSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot = 0;
        [SerializeField] private bool includeSceneStables = true;
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
            var tracker = CreatureBondingTracker.Ensure();
            var save = new CreatureBondingSaveFile { SlotIndex = Mathf.Max(0, slot) };

            foreach (var creature in tracker.OwnedCreatures)
            {
                save.OwnedCreatures.Add(creature);
            }

            if (includeSceneStables)
            {
                foreach (var stable in ElementbornFindUtility.FindAll<CreatureStable>())
                {
                    var record = new CreatureStableSaveRecord
                    {
                        StableId = stable.StableId
                    };

                    foreach (string creatureId in stable.StoredCreatureRecordIds)
                    {
                        record.StoredCreatureRecordIds.Add(creatureId);
                    }

                    save.Stables.Add(record);
                }
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

            var save = JsonUtility.FromJson<CreatureBondingSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            var tracker = CreatureBondingTracker.Ensure();
            tracker.Clear();

            foreach (var creature in save.OwnedCreatures)
            {
                tracker.Import(creature);
            }

            if (includeSceneStables)
            {
                foreach (var stable in ElementbornFindUtility.FindAll<CreatureStable>())
                {
                    stable.Clear();
                    var record = save.Stables.Find(s => s.StableId == stable.StableId);
                    if (record == null)
                    {
                        continue;
                    }

                    foreach (string creatureId in record.StoredCreatureRecordIds)
                    {
                        stable.ImportStored(creatureId);
                    }
                }
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "creatures");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_creatures.json");
        }
    }
}
