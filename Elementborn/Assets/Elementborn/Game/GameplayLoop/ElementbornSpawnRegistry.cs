using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornSpawnRegistry : MonoBehaviour
    {
        public static ElementbornSpawnRegistry Instance { get; private set; }

        [SerializeField] private List<ElementbornSpawnPoint> spawnPoints = new List<ElementbornSpawnPoint>();

        public IReadOnlyList<ElementbornSpawnPoint> SpawnPoints => spawnPoints;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Refresh();
        }

        public static ElementbornSpawnRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(ElementbornSpawnRegistry));
            return go.AddComponent<ElementbornSpawnRegistry>();
        }

        [ContextMenu("Refresh Spawn Points")]
        public void Refresh()
        {
            spawnPoints.Clear();
            foreach (ElementbornSpawnPoint point in ElementbornFindUtility.FindAll<ElementbornSpawnPoint>())
            {
                if (point != null && !spawnPoints.Contains(point))
                {
                    spawnPoints.Add(point);
                }
            }
        }

        public ElementbornSpawnPoint FindFirst(ElementbornSpawnRole role)
        {
            Refresh();
            return spawnPoints.Find(p => p != null && p.Role == role);
        }

        public ElementbornSpawnPoint FindById(string spawnId)
        {
            Refresh();
            string needle = (spawnId ?? "").Trim().ToLowerInvariant();
            return spawnPoints.Find(p => p != null && (p.SpawnId ?? "").ToLowerInvariant() == needle);
        }

        public List<ElementbornSpawnPoint> FindAll(ElementbornSpawnRole role)
        {
            Refresh();
            return spawnPoints.FindAll(p => p != null && p.Role == role);
        }

        public string BuildSummary()
        {
            Refresh();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Spawn Registry");
            foreach (ElementbornSpawnPoint point in spawnPoints)
            {
                if (point != null)
                {
                    sb.AppendLine($"- {point.SpawnId}: {point.Role} @ {point.transform.position}");
                }
            }

            if (spawnPoints.Count == 0)
            {
                sb.AppendLine("- No spawn points registered.");
            }

            return sb.ToString();
        }
    }
}
