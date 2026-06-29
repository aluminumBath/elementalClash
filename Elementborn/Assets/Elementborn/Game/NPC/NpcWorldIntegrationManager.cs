using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace Elementborn.Game
{
    /// <summary>
    /// Data-driven world placement bridge for NPC rosters.
    /// It registers map markers/journal entries and can spawn simple placeholder NPC objects for playtesting.
    /// </summary>
    public sealed class NpcWorldIntegrationManager : MonoBehaviour
    {
        private static readonly HashSet<string> missingTagWarnings = new HashSet<string>();
        [SerializeField] private NpcWorldIntegrationManifest manifest;
        [SerializeField] private bool registerMarkersOnStart = true;
        [SerializeField] private bool addJournalEntriesOnStart = true;
        [SerializeField] private bool spawnPlaceholderNpcsOnStart = false;
        [SerializeField] private GameObject placeholderPrefab;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private float placeholderVerticalOffset = 1f;

        private void Start()
        {
            if (registerMarkersOnStart || addJournalEntriesOnStart)
            {
                RegisterAll();
            }

            if (spawnPlaceholderNpcsOnStart)
            {
                SpawnAllPlaceholders();
            }
        }

        [ContextMenu("Register All NPCs")]
        public void RegisterAll()
        {
            if (manifest == null)
            {
                return;
            }

            foreach (var entry in manifest.Entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (registerMarkersOnStart)
                {
                    RegisterMarker(entry);
                }

                if (addJournalEntriesOnStart)
                {
                    AddJournal(entry);
                }
            }
        }

        [ContextMenu("Spawn All Placeholder NPCs")]
        public void SpawnAllPlaceholders()
        {
            if (manifest == null)
            {
                return;
            }

            foreach (var entry in manifest.Entries)
            {
                if (entry != null)
                {
                    SpawnPlaceholder(entry);
                }
            }
        }

        public GameObject SpawnPlaceholder(NpcWorldEntryDefinition entry)
        {
            if (entry == null)
            {
                return null;
            }

            Vector3 position = entry.WorldPosition + Vector3.up * placeholderVerticalOffset;
            GameObject go = placeholderPrefab != null
                ? Instantiate(placeholderPrefab, position, Quaternion.identity, spawnParent)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            go.name = entry.DisplayName;
            go.transform.position = position;
            if (spawnParent != null)
            {
                go.transform.SetParent(spawnParent, true);
            }

            TryAssignTag(go, "Interactable");

            var marker = go.GetComponent<NpcWorldPlacementMarker>();
            if (marker == null) marker = go.AddComponent<NpcWorldPlacementMarker>();
            marker.SetNpc(entry);

            var voice = go.GetComponent<NpcVoicePlaybackController>();
            if (voice == null) voice = go.AddComponent<NpcVoicePlaybackController>();
            voice.SetNpc(entry);

            var hook = go.GetComponent<NpcDialogueHookInteractable>();
            if (hook == null) hook = go.AddComponent<NpcDialogueHookInteractable>();
            hook.Configure(entry, null, null, false);

            return go;
        }

        public static void RegisterMarker(NpcWorldEntryDefinition entry)
        {
            MapMarkerType markerType = entry.Role == NpcWorldRole.Villain
                ? MapMarkerType.DangerZone
                : entry.Role == NpcWorldRole.Merchant
                    ? MapMarkerType.VendorNpc
                    : entry.Role == NpcWorldRole.QuestGiver
                        ? MapMarkerType.QuestGiverNpc
                        : MapMarkerType.GuideNpc;

            PlayerMapMarkerTracker.ReportOrUpdateMarker(
                "npc_" + PlayerJournalTracker.Safe(entry.NpcId),
                markerType,
                entry.WorldPosition,
                entry.DisplayName,
                true,
                -1f,
                CreatureTraversalType.Unknown,
                entry.NpcId,
                entry.LocationName,
                false);
        }

        public static void AddJournal(NpcWorldEntryDefinition entry)
        {
            string body =
                $"{entry.TitleOrRank}\n" +
                $"Role: {entry.Role}\n" +
                $"Location: {entry.LocationName}, {entry.Region}\n" +
                $"Origin: {entry.Origin}\n" +
                $"Elements: {entry.PrimaryElement}{(string.IsNullOrWhiteSpace(entry.SecondaryElement) ? "" : " / " + entry.SecondaryElement)}\n" +
                $"Aliases: {entry.Aliases}\n\n" +
                $"Appearance: {entry.AppearanceNotes}\n\n" +
                $"Personality: {entry.PersonalityNotes}\n\n" +
                $"Relationships: {entry.RelationshipSummary}\n\n" +
                entry.Notes;

            PlayerJournalTracker.AddOrUpdateEntry(
                "npc_" + PlayerJournalTracker.Safe(entry.NpcId),
                JournalEntryType.Npc,
                entry.DisplayName,
                body,
                entry.Region,
                entry.NpcId);
        }

        private void TryAssignTag(GameObject go, string tag)
        {
            if (go == null || string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            if (!IsTagDefined(tag))
            {
                WarnMissingTagOnce(tag);
                return;
            }

            try
            {
                go.tag = tag;
            }
            catch
            {
                WarnMissingTagOnce(tag);
            }
        }

        private static bool IsTagDefined(string tag)
        {
#if UNITY_EDITOR
            string[] tags = InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                {
                    return true;
                }
            }

            return tag == "Untagged" || tag == "Respawn" || tag == "Finish" || tag == "EditorOnly" || tag == "MainCamera" || tag == "Player" || tag == "GameController";
#else
            return true;
#endif
        }

        private static void WarnMissingTagOnce(string tag)
        {
            if (missingTagWarnings.Contains(tag))
            {
                return;
            }

            missingTagWarnings.Add(tag);
            Debug.LogWarning($"Elementborn tag '{tag}' is missing. Run Elementborn → Unity Setup → Ensure Tags and Layers, or apply v74's TagManager repair script.");
        }
    }
}
