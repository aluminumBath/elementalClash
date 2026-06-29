using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Runtime journal/codex tracker for discoveries, rumors, bestiary entries, NPC notes, faction lore, and tutorial tips.
    /// </summary>
    public sealed class PlayerJournalTracker : MonoBehaviour
    {
        public static PlayerJournalTracker Instance { get; private set; }

        [SerializeField] private List<JournalEntryRecord> entries = new List<JournalEntryRecord>();

        public IReadOnlyList<JournalEntryRecord> Entries => entries;

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

        public static PlayerJournalTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(PlayerJournalTracker));
            return go.AddComponent<PlayerJournalTracker>();
        }

        public static JournalEntryRecord AddOrUpdateEntry(
            string entryId,
            JournalEntryType type,
            string title,
            string body,
            string region = "",
            string relatedId = "",
            bool markNew = true)
        {
            var tracker = Ensure();

            if (string.IsNullOrWhiteSpace(entryId))
            {
                entryId = BuildEntryId(type, title, relatedId);
            }

            var entry = tracker.entries.Find(e => e.EntryId == entryId);
            if (entry == null)
            {
                entry = new JournalEntryRecord
                {
                    EntryId = entryId,
                    CreatedAtUnscaledTime = Time.unscaledTime
                };
                tracker.entries.Add(entry);
            }

            entry.Type = type;
            entry.Title = string.IsNullOrWhiteSpace(title) ? GetDefaultTitle(type) : title;
            entry.Body = body ?? string.Empty;
            entry.Region = region ?? string.Empty;
            entry.RelatedId = relatedId ?? string.Empty;
            entry.UpdatedAtUnscaledTime = Time.unscaledTime;
            if (markNew)
            {
                entry.IsNew = true;
            }

            NotificationFeed.Post($"Journal updated: {entry.Title}", NotificationType.Journal);
            return entry;
        }

        public static void MarkRead(string entryId)
        {
            var entry = Find(entryId);
            if (entry != null)
            {
                entry.IsNew = false;
            }
        }

        public static void Pin(string entryId, bool pinned = true)
        {
            var entry = Find(entryId);
            if (entry != null)
            {
                entry.IsPinned = pinned;
            }
        }

        public static void Complete(string entryId)
        {
            var entry = Find(entryId);
            if (entry != null)
            {
                entry.IsComplete = true;
                entry.IsNew = false;
            }
        }

        public static List<JournalEntryRecord> Search(string query, int maxResults = 20)
        {
            var tracker = Ensure();
            var results = new List<JournalEntryRecord>();

            foreach (var entry in tracker.entries)
            {
                if (entry != null && entry.Matches(query))
                {
                    results.Add(entry);
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            return results;
        }

        public static List<JournalEntryRecord> GetByType(JournalEntryType type, int maxResults = 100)
        {
            var tracker = Ensure();
            var results = new List<JournalEntryRecord>();

            foreach (var entry in tracker.entries)
            {
                if (entry != null && entry.Type == type)
                {
                    results.Add(entry);
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            return results;
        }

        public static JournalEntryRecord Find(string entryId)
        {
            return Ensure().entries.Find(e => e.EntryId == entryId);
        }

        public static void Clear()
        {
            Ensure().entries.Clear();
        }

        public static void AddDiscovery(string title, string body, string region = "")
        {
            AddOrUpdateEntry("", JournalEntryType.Discovery, title, body, region);
        }

        public static void AddLocation(string title, string body, string region = "")
        {
            AddOrUpdateEntry("", JournalEntryType.Location, title, body, region);
        }

        public static void AddCreature(string creatureName, string body, string region = "")
        {
            AddOrUpdateEntry("creature_" + Safe(creatureName), JournalEntryType.Bestiary, creatureName, body, region, creatureName);
        }

        public static void AddNpc(string npcName, string body, string region = "")
        {
            AddOrUpdateEntry("npc_" + Safe(npcName), JournalEntryType.Npc, npcName, body, region, npcName);
        }

        public static void AddFaction(string factionName, string body)
        {
            AddOrUpdateEntry("faction_" + Safe(factionName), JournalEntryType.Faction, factionName, body, relatedId: factionName);
        }

        public static void AddRumor(string title, string body, string region = "")
        {
            AddOrUpdateEntry("rumor_" + Safe(title), JournalEntryType.Rumor, title, body, region);
        }

        public static string Safe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "entry";
            }

            var chars = value.Trim().ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                bool ok = char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-';
                chars[i] = ok ? chars[i] : '_';
            }

            return new string(chars).Trim('_');
        }

        private static string BuildEntryId(JournalEntryType type, string title, string relatedId)
        {
            string key = !string.IsNullOrWhiteSpace(relatedId) ? relatedId : title;
            return type.ToString().ToLowerInvariant() + "_" + Safe(key);
        }

        private static string GetDefaultTitle(JournalEntryType type)
        {
            return type switch
            {
                JournalEntryType.Quest => "Quest",
                JournalEntryType.Rumor => "Rumor",
                JournalEntryType.Location => "Location",
                JournalEntryType.Creature => "Creature",
                JournalEntryType.Npc => "NPC",
                JournalEntryType.Faction => "Faction",
                JournalEntryType.Item => "Item",
                JournalEntryType.Element => "Element",
                JournalEntryType.Tutorial => "Tutorial",
                JournalEntryType.Discovery => "Discovery",
                JournalEntryType.Bestiary => "Bestiary",
                JournalEntryType.Boat => "Boat",
                JournalEntryType.Lore => "Lore",
                _ => "Journal Entry"
            };
        }
    }
}
