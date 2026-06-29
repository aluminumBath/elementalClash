using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EnemyAiSaveBridge : MonoBehaviour
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
            var save = new EnemyAiSaveFile { SlotIndex = Mathf.Max(0, slot) };

            foreach (var brain in ElementbornFindUtility.FindAll<EnemyCombatBrain>())
            {
                if (brain == null)
                {
                    continue;
                }

                save.Enemies.Add(new EnemyAiSaveRecord
                {
                    RuntimeId = brain.gameObject.name,
                    State = brain.State,
                    X = brain.transform.position.x,
                    Y = brain.transform.position.y,
                    Z = brain.transform.position.z
                });
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

            var save = JsonUtility.FromJson<EnemyAiSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            foreach (var brain in ElementbornFindUtility.FindAll<EnemyCombatBrain>())
            {
                var record = save.Enemies.Find(e => e.RuntimeId == brain.gameObject.name);
                if (record == null)
                {
                    continue;
                }

                brain.transform.position = new Vector3(record.X, record.Y, record.Z);
                brain.ForceState(record.State);
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "enemy_ai");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_enemy_ai.json");
        }
    }
}
