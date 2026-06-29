using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AbilitySaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot;
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
            var tracker = PlayerAbilityTracker.Ensure();
            var save = new AbilitySaveFile
            {
                SlotIndex = Mathf.Max(0, slot),
                PlayerLevel = tracker.PlayerLevel,
                AvailableSkillPoints = tracker.AvailableSkillPoints
            };

            foreach (var record in tracker.UnlockedAbilities)
            {
                if (record != null)
                {
                    save.UnlockedAbilities.Add(record);
                }
            }

            foreach (var loadoutSlot in tracker.Loadout)
            {
                if (loadoutSlot != null)
                {
                    save.Loadout.Add(loadoutSlot);
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

            var save = JsonUtility.FromJson<AbilitySaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            var tracker = PlayerAbilityTracker.Ensure();
            tracker.Clear();
            tracker.ImportLevelAndPoints(save.PlayerLevel, save.AvailableSkillPoints);

            foreach (var record in save.UnlockedAbilities)
            {
                tracker.ImportRecord(record);
            }

            foreach (var loadoutSlot in save.Loadout)
            {
                tracker.ImportLoadoutSlot(loadoutSlot);
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "abilities");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_abilities.json");
        }
    }
}
