using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Stores what the game/NPCs know about the player, world, quests, locations, factions, and rumors.
    /// This is a runtime foundation that can later be serialized into the main save system.
    /// </summary>
    public sealed class DialogueMemoryTracker : MonoBehaviour
    {
        public static DialogueMemoryTracker Instance { get; private set; }

        [SerializeField] private List<DialogueMemoryFact> facts = new List<DialogueMemoryFact>();
        [SerializeField] private List<NpcRelationshipState> relationships = new List<NpcRelationshipState>();

        public IReadOnlyList<DialogueMemoryFact> Facts => facts;
        public IReadOnlyList<NpcRelationshipState> Relationships => relationships;

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

        public static DialogueMemoryTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(DialogueMemoryTracker));
            return go.AddComponent<DialogueMemoryTracker>();
        }

        public static DialogueMemoryFact Remember(
            DialogueMemoryType type,
            string subject,
            string value,
            string source = "",
            string region = "",
            string relatedQuestId = "",
            bool important = false,
            bool playerKnows = true)
        {
            var tracker = Ensure();
            string factId = BuildFactId(type, subject, region, relatedQuestId);
            var existing = tracker.facts.Find(f => f.FactId == factId);

            if (existing == null)
            {
                existing = new DialogueMemoryFact
                {
                    FactId = factId,
                    CreatedAtUnscaledTime = Time.unscaledTime
                };
                tracker.facts.Add(existing);
            }

            existing.Type = type;
            existing.Subject = subject ?? "";
            existing.Value = value ?? "";
            existing.Source = source ?? "";
            existing.Region = region ?? "";
            existing.RelatedQuestId = relatedQuestId ?? "";
            existing.Important = important;
            existing.PlayerKnows = playerKnows;
            existing.LastMentionedAtUnscaledTime = Time.unscaledTime;

            return existing;
        }

        public static List<DialogueMemoryFact> Search(string query, int maxResults = 5, bool onlyPlayerKnown = true)
        {
            var tracker = Ensure();
            var results = new List<DialogueMemoryFact>();

            foreach (var fact in tracker.facts)
            {
                if (fact == null)
                {
                    continue;
                }

                if (onlyPlayerKnown && !fact.PlayerKnows)
                {
                    continue;
                }

                if (fact.Matches(query))
                {
                    results.Add(fact);
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            return results;
        }

        public static List<DialogueMemoryFact> GetImportantFacts(int maxResults = 5)
        {
            var tracker = Ensure();
            var results = new List<DialogueMemoryFact>();

            foreach (var fact in tracker.facts)
            {
                if (fact == null || !fact.Important || !fact.PlayerKnows)
                {
                    continue;
                }

                results.Add(fact);
                if (results.Count >= maxResults)
                {
                    break;
                }
            }

            return results;
        }

        public static NpcRelationshipState GetRelationship(string npcId, string displayName = "")
        {
            var tracker = Ensure();
            npcId = string.IsNullOrWhiteSpace(npcId) ? displayName : npcId;
            if (string.IsNullOrWhiteSpace(npcId))
            {
                npcId = "npc";
            }

            var relationship = tracker.relationships.Find(r => r.NpcId == npcId);
            if (relationship != null)
            {
                return relationship;
            }

            relationship = new NpcRelationshipState
            {
                NpcId = npcId,
                DisplayName = displayName
            };
            tracker.relationships.Add(relationship);
            return relationship;
        }

        public static void RecordNpcInteraction(string npcId, string displayName, string topic, string playerStatement)
        {
            var rel = GetRelationship(npcId, displayName);
            rel.LastTopic = topic ?? "";
            rel.LastPlayerStatement = playerStatement ?? "";

            string lower = (playerStatement ?? "").ToLowerInvariant();
            if (lower.Contains("thank") || lower.Contains("help"))
            {
                rel.Trust += 1;
                rel.Respect += 1;
            }

            if (lower.Contains("threat") || lower.Contains("kill") || lower.Contains("attack"))
            {
                rel.Fear += 1;
                rel.Annoyance += 1;
            }

            if (lower.Contains("sorry") || lower.Contains("apolog"))
            {
                rel.Annoyance = Mathf.Max(0, rel.Annoyance - 1);
            }
        }

        public static void Clear()
        {
            var tracker = Ensure();
            tracker.facts.Clear();
            tracker.relationships.Clear();
        }

        private static string BuildFactId(DialogueMemoryType type, string subject, string region, string questId)
        {
            return $"{type}_{Safe(subject)}_{Safe(region)}_{Safe(questId)}";
        }

        public static string Safe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "none";
            }

            var chars = value.Trim().ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                bool ok = char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-';
                chars[i] = ok ? chars[i] : '_';
            }

            return new string(chars).Trim('_');
        }
    }
}
