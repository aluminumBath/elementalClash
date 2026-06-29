using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Lightweight JSON save bridge for dialogue memory, rumors, and NPC relationships.
    /// Integrate with your real save-slot UI when ready.
    /// </summary>
    public sealed class DialogueMemorySaveBridge : MonoBehaviour
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
            var memory = DialogueMemoryTracker.Ensure();
            var rumors = RumorTracker.Ensure();

            var save = new DialogueMemorySaveFile
            {
                SlotIndex = Mathf.Max(0, slot)
            };

            foreach (var fact in memory.Facts)
            {
                save.Facts.Add(fact);
            }

            foreach (var rel in memory.Relationships)
            {
                save.Relationships.Add(rel);
            }

            foreach (var rumor in rumors.Rumors)
            {
                save.Rumors.Add(rumor);
            }

            File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save, prettyPrint: true));
            Debug.Log($"Saved dialogue memory for slot {slot}: {save.Facts.Count} fact(s), {save.Rumors.Count} rumor(s).");
        }

        public void LoadSlot(int slot)
        {
            string path = GetPath(slot);
            if (!File.Exists(path))
            {
                Debug.Log($"No dialogue memory save file exists for slot {slot}: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            var save = JsonUtility.FromJson<DialogueMemorySaveFile>(json);
            if (save == null)
            {
                Debug.LogWarning($"Failed to load dialogue memory save file: {path}");
                return;
            }

            DialogueMemoryTracker.Clear();
            RumorTracker.Clear();

            foreach (var fact in save.Facts)
            {
                DialogueMemoryTracker.Remember(
                    fact.Type,
                    fact.Subject,
                    fact.Value,
                    fact.Source,
                    fact.Region,
                    fact.RelatedQuestId,
                    fact.Important,
                    fact.PlayerKnows);
            }

            foreach (var rel in save.Relationships)
            {
                var runtime = DialogueMemoryTracker.GetRelationship(rel.NpcId, rel.DisplayName);
                runtime.Trust = rel.Trust;
                runtime.Fear = rel.Fear;
                runtime.Respect = rel.Respect;
                runtime.Annoyance = rel.Annoyance;
                runtime.LastTopic = rel.LastTopic;
                runtime.LastPlayerStatement = rel.LastPlayerStatement;
            }

            foreach (var rumor in save.Rumors)
            {
                RumorTracker.AddRumor(
                    rumor.Text,
                    rumor.Type,
                    rumor.Source,
                    rumor.Region,
                    rumor.Important,
                    rumor.IsTrue,
                    rumor.WorldPosition,
                    rumor.HasWorldPosition);
            }

            Debug.Log($"Loaded dialogue memory for slot {slot}: {save.Facts.Count} fact(s), {save.Rumors.Count} rumor(s).");
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
            string dir = Path.Combine(Application.persistentDataPath, "dialogue_memory");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_dialogue_memory.json");
        }
    }
}
