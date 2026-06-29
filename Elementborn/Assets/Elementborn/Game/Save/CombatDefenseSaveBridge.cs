using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CombatDefenseSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot;
        [SerializeField] private StaminaResource stamina;
        [SerializeField] private bool autoLoadOnStart = false;
        [SerializeField] private bool autoSaveOnApplicationPause = true;
        [SerializeField] private bool autoSaveOnApplicationQuit = true;

        private void Awake()
        {
            if (stamina == null)
            {
                stamina = GetComponent<StaminaResource>();
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
            if (stamina == null)
            {
                return;
            }

            var save = new CombatDefenseSaveFile
            {
                SlotIndex = Mathf.Max(0, slot),
                CurrentStamina = stamina.CurrentStamina,
                MaxStamina = stamina.MaxStamina
            };

            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
        }

        public void LoadSlot(int slot)
        {
            if (stamina == null)
            {
                return;
            }

            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                return;
            }

            var save = JsonUtility.FromJson<CombatDefenseSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            stamina.Import(save.CurrentStamina, save.MaxStamina);
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "combat_defense");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_combat_defense.json");
        }
    }
}
