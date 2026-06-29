using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class RumorTracker : MonoBehaviour
    {
        public static RumorTracker Instance { get; private set; }

        [SerializeField] private List<RumorRecord> rumors = new List<RumorRecord>();

        public IReadOnlyList<RumorRecord> Rumors => rumors;

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

        public static RumorTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(RumorTracker));
            return go.AddComponent<RumorTracker>();
        }

        public static RumorRecord AddRumor(
            string text,
            RumorType type = RumorType.Unknown,
            string source = "",
            string region = "",
            bool important = false,
            bool isTrue = true,
            Vector3 worldPosition = default,
            bool hasWorldPosition = false)
        {
            var tracker = Ensure();
            string id = DialogueMemoryTracker.Safe(type + "_" + source + "_" + text);
            var existing = tracker.rumors.Find(r => r.RumorId == id);
            if (existing != null)
            {
                return existing;
            }

            var rumor = new RumorRecord
            {
                RumorId = id,
                Type = type,
                Text = text ?? "",
                Source = source ?? "",
                Region = region ?? "",
                Important = important,
                IsTrue = isTrue,
                WorldPosition = worldPosition,
                HasWorldPosition = hasWorldPosition,
                CreatedAtUnscaledTime = Time.unscaledTime
            };

            tracker.rumors.Add(rumor);

            if (hasWorldPosition)
            {
                MapMarkerType markerType = type switch
                {
                    RumorType.Threat => MapMarkerType.DangerZone,
                    RumorType.Treasure => MapMarkerType.Treasure,
                    RumorType.Location => MapMarkerType.CustomPin,
                    RumorType.Creature => MapMarkerType.RareEnemySighting,
                    RumorType.Sea => MapMarkerType.SeaMonsterSighting,
                    _ => MapMarkerType.CustomPin
                };

                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "rumor_" + id,
                    markerType,
                    worldPosition,
                    string.IsNullOrWhiteSpace(source) ? "Rumor" : $"Rumor from {source}",
                    isPersistent: true,
                    notes: text);
            }

            DialogueMemoryTracker.Remember(
                DialogueMemoryType.WorldFact,
                "Rumor",
                text,
                source,
                region,
                important: important);

            return rumor;
        }

        public static RumorRecord GetBestRumor(string query = "")
        {
            var tracker = Ensure();

            foreach (var rumor in tracker.rumors)
            {
                if (rumor == null || !rumor.PlayerKnows)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(query) || rumor.Text.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                {
                    return rumor;
                }
            }

            return null;
        }

        public static List<RumorRecord> GetKnownRumors(int maxResults = 5)
        {
            var tracker = Ensure();
            var results = new List<RumorRecord>();

            foreach (var rumor in tracker.rumors)
            {
                if (rumor == null || !rumor.PlayerKnows)
                {
                    continue;
                }

                results.Add(rumor);
                if (results.Count >= maxResults)
                {
                    break;
                }
            }

            return results;
        }

        public static void Clear()
        {
            Ensure().rumors.Clear();
        }
    }
}
