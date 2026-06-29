using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SpellCastingSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot;
        [SerializeField] private SpellResourcePool resourcePool;
        [SerializeField] private bool autoLoadOnStart = false;
        [SerializeField] private bool autoSaveOnApplicationPause = true;
        [SerializeField] private bool autoSaveOnApplicationQuit = true;

        private void Awake()
        {
            if (resourcePool == null)
            {
                resourcePool = GetComponent<SpellResourcePool>();
            }
        }

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
            var save = new SpellCastingSaveFile
            {
                SlotIndex = Mathf.Max(0, slot),
                CurrentResource = resourcePool != null ? resourcePool.CurrentValue : 100f,
                MaxResource = resourcePool != null ? resourcePool.MaxValue : 100f
            };

            foreach (var cooldown in SpellCooldownTracker.Ensure().Records)
            {
                if (cooldown != null)
                {
                    save.Cooldowns.Add(cooldown);
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

            var save = JsonUtility.FromJson<SpellCastingSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            if (resourcePool != null)
            {
                resourcePool.Import(save.CurrentResource, save.MaxResource);
            }

            var tracker = SpellCooldownTracker.Ensure();
            tracker.ClearRecords();

            foreach (var cooldown in save.Cooldowns)
            {
                tracker.ImportRecord(cooldown);
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "spells");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_spells.json");
        }
    }
}
