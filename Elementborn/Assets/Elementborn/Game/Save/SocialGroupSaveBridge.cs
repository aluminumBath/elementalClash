using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SocialGroupSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot = 0;

        public void SetCurrentSlot(int slot)
        {
            currentSlot = Mathf.Max(0, slot);
        }

        public void SaveCurrentSlot() => SaveSlot(currentSlot);
        public void LoadCurrentSlot() => LoadSlot(currentSlot);

        public void SaveSlot(int slot)
        {
            var registry = SocialGroupRegistry.Ensure();
            var save = new SocialGroupSaveFile { SlotIndex = Mathf.Max(0, slot) };
            foreach (SocialGroupRuntimeRecord record in registry.RuntimeRecords)
            {
                if (record != null)
                {
                    save.RuntimeRecords.Add(record);
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

            var save = JsonUtility.FromJson<SocialGroupSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            foreach (SocialGroupRuntimeRecord record in save.RuntimeRecords)
            {
                if (record != null)
                {
                    SocialGroupRuntimeRecord target = SocialGroupRegistry.Ensure().GetOrCreateRecord(record.GroupId);
                    target.LastEventId = record.LastEventId;
                    target.TimesActivated = record.TimesActivated;
                    target.NeighborhoodTrust = record.NeighborhoodTrust;
                    target.ChaosLevel = record.ChaosLevel;
                    target.Notes = record.Notes;
                }
            }
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
            string dir = Path.Combine(Application.persistentDataPath, "social_groups");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_social_groups.json");
        }
    }
}
