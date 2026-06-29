using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ResourceHarvestingTracker : MonoBehaviour
    {
        public static ResourceHarvestingTracker Instance { get; private set; }

        [SerializeField] private int totalHarvests;
        [SerializeField] private int rareHarvests;
        [SerializeField] private List<string> discoveredNodeIds = new List<string>();

        public int TotalHarvests => totalHarvests;
        public int RareHarvests => rareHarvests;
        public IReadOnlyList<string> DiscoveredNodeIds => discoveredNodeIds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static ResourceHarvestingTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(ResourceHarvestingTracker));
            return go.AddComponent<ResourceHarvestingTracker>();
        }

        public static void RecordHarvest(ResourceNodeDefinition node, bool rare)
        {
            var tracker = Ensure();
            tracker.totalHarvests++;
            if (rare)
            {
                tracker.rareHarvests++;
            }

            if (node != null && !tracker.discoveredNodeIds.Contains(node.NodeId))
            {
                tracker.discoveredNodeIds.Add(node.NodeId);

                if (node.AddJournalOnFirstHarvest)
                {
                    PlayerJournalTracker.AddOrUpdateEntry(
                        "resource_" + PlayerJournalTracker.Safe(node.NodeId),
                        JournalEntryType.Item,
                        node.DisplayName,
                        string.IsNullOrWhiteSpace(node.Description) ? "A harvestable resource." : node.Description,
                        node.Region,
                        node.NodeId);
                }
            }
        }

        public static void Clear()
        {
            var tracker = Ensure();
            tracker.totalHarvests = 0;
            tracker.rareHarvests = 0;
            tracker.discoveredNodeIds.Clear();
        }

        public void Import(int total, int rare, List<string> discovered)
        {
            totalHarvests = Mathf.Max(0, total);
            rareHarvests = Mathf.Max(0, rare);
            discoveredNodeIds.Clear();
            if (discovered != null)
            {
                discoveredNodeIds.AddRange(discovered);
            }
        }
    }
}
