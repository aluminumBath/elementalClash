using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class GatheringSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot;
        [SerializeField] private bool includeSceneNodes = true;
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
            var tracker = ResourceHarvestingTracker.Ensure();
            var save = new GatheringSaveFile
            {
                SlotIndex = Mathf.Max(0, slot),
                TotalHarvests = tracker.TotalHarvests,
                RareHarvests = tracker.RareHarvests
            };

            foreach (string id in tracker.DiscoveredNodeIds)
            {
                save.DiscoveredNodeIds.Add(id);
            }

            if (includeSceneNodes)
            {
                foreach (var node in ElementbornFindUtility.FindAll<HarvestableResourceNode>())
                {
                    if (node == null)
                    {
                        continue;
                    }

                    save.Nodes.Add(new HarvestNodeSaveRecord
                    {
                        RuntimeNodeId = node.RuntimeNodeId,
                        State = node.State,
                        HarvestsRemaining = 1,
                        RespawnAtTime = -1f
                    });
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

            var save = JsonUtility.FromJson<GatheringSaveFile>(File.ReadAllText(path));
            if (save == null)
            {
                return;
            }

            ResourceHarvestingTracker.Ensure().Import(
                save.TotalHarvests,
                save.RareHarvests,
                save.DiscoveredNodeIds ?? new List<string>());

            if (includeSceneNodes)
            {
                foreach (var node in ElementbornFindUtility.FindAll<HarvestableResourceNode>())
                {
                    var record = save.Nodes.Find(n => n.RuntimeNodeId == node.RuntimeNodeId);
                    if (record != null)
                    {
                        node.ImportState(record.State, record.HarvestsRemaining, record.RespawnAtTime);
                    }
                }
            }
        }

        public static string GetPath(int slot)
        {
            string dir = Path.Combine(Application.persistentDataPath, "gathering");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"slot_{Mathf.Max(0, slot)}_gathering.json");
        }
    }
}
