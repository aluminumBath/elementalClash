using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class FireCapitalRegistry : MonoBehaviour
    {
        public static FireCapitalRegistry Instance { get; private set; }

        [SerializeField] private List<FireCapitalCourtHookDefinition> hooks = new List<FireCapitalCourtHookDefinition>();
        [SerializeField] private List<FireCapitalRuntimeRecord> records = new List<FireCapitalRuntimeRecord>();
        [SerializeField] private bool registerJournalAndMapOnStart = true;

        public IReadOnlyList<FireCapitalCourtHookDefinition> Hooks => hooks;
        public IReadOnlyList<FireCapitalRuntimeRecord> Records => records;

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

        private void Start()
        {
            if (registerJournalAndMapOnStart)
            {
                RegisterJournalAndMap();
            }
        }

        public static FireCapitalRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(FireCapitalRegistry));
            return go.AddComponent<FireCapitalRegistry>();
        }

        public void SetHooks(List<FireCapitalCourtHookDefinition> values)
        {
            hooks = values ?? new List<FireCapitalCourtHookDefinition>();
        }

        public FireCapitalCourtHookDefinition FindHook(string hookId)
        {
            string needle = (hookId ?? "").Trim().ToLowerInvariant();
            return hooks.Find(h => h != null && (h.HookId ?? "").ToLowerInvariant() == needle);
        }

        public FireCapitalRuntimeRecord GetOrCreateRecord(string hookId)
        {
            FireCapitalRuntimeRecord record = records.Find(r => r != null && r.HookId == hookId);
            if (record != null)
            {
                return record;
            }

            record = new FireCapitalRuntimeRecord { HookId = hookId };
            records.Add(record);
            return record;
        }

        public void Discover(string hookId)
        {
            FireCapitalCourtHookDefinition hook = FindHook(hookId);
            if (hook == null)
            {
                Debug.LogWarning($"Fire Capital hook not found: {hookId}");
                return;
            }

            FireCapitalRuntimeRecord record = GetOrCreateRecord(hook.HookId);
            record.Discovered = true;
            AddJournal(hook, "Discovered");
            AddMarker(hook);
        }

        public void StartHook(string hookId)
        {
            FireCapitalCourtHookDefinition hook = FindHook(hookId);
            if (hook == null)
            {
                Debug.LogWarning($"Fire Capital hook not found: {hookId}");
                return;
            }

            FireCapitalRuntimeRecord record = GetOrCreateRecord(hook.HookId);
            record.Discovered = true;
            record.Started = true;
            record.TimesStarted++;

            if (hook.PressureDeltaOnStart != 0)
            {
                CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, hook.PressureType, hook.PressureDeltaOnStart, hook.Title);
            }

            if (hook.QuestToStart != null)
            {
                QuestUiTracker.StartQuest(hook.QuestToStart);
            }

            AddJournal(hook, "Started");
            AddMarker(hook);
            NotificationFeed.Post("Fire Capital: " + hook.Title, NotificationType.Quest);
        }

        public void ResolveHook(string hookId, string notes = "")
        {
            FireCapitalCourtHookDefinition hook = FindHook(hookId);
            if (hook == null)
            {
                Debug.LogWarning($"Fire Capital hook not found: {hookId}");
                return;
            }

            FireCapitalRuntimeRecord record = GetOrCreateRecord(hook.HookId);
            record.Resolved = true;
            record.Notes = notes;

            if (hook.StabilityDeltaOnResolve != 0)
            {
                CapitalWorldStateTracker.Ensure().AddStability(CapitalId.FireCapital, hook.StabilityDeltaOnResolve, hook.Title);
            }

            PlayerMapMarkerTracker.HideMarker("fire_hook_" + PlayerJournalTracker.Safe(hook.HookId));
            AddJournal(hook, "Resolved");
            NotificationFeed.Post("Resolved Fire Capital hook: " + hook.Title, NotificationType.Info);
        }

        public void RegisterJournalAndMap()
        {
            foreach (FireCapitalCourtHookDefinition hook in hooks)
            {
                if (hook == null)
                {
                    continue;
                }

                AddJournal(hook, "Available");
                AddMarker(hook);
            }
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Fire Capital Registry");
            foreach (FireCapitalCourtHookDefinition hook in hooks)
            {
                if (hook == null)
                {
                    continue;
                }

                FireCapitalRuntimeRecord record = GetOrCreateRecord(hook.HookId);
                sb.AppendLine($"- {hook.Title} [{hook.HookType}/{hook.District}] discovered={record.Discovered}, started={record.Started}, resolved={record.Resolved}, starts={record.TimesStarted}");
            }

            if (hooks.Count == 0)
            {
                sb.AppendLine("- No Fire Capital hooks loaded.");
            }

            return sb.ToString();
        }

        private void AddJournal(FireCapitalCourtHookDefinition hook, string status)
        {
            string body =
                $"{hook.PlayerFacingSummary}\n\n" +
                $"District: {hook.District}\n" +
                $"Status: {status}\n" +
                $"Director notes: {hook.HiddenDirectorNotes}";

            PlayerJournalTracker.AddOrUpdateEntry(
                "fire_hook_" + PlayerJournalTracker.Safe(hook.HookId),
                JournalEntryType.Quest,
                hook.Title,
                body,
                "Fire Capital",
                hook.HookId);
        }

        private void AddMarker(FireCapitalCourtHookDefinition hook)
        {
            PlayerMapMarkerTracker.ReportOrUpdateMarker(
                "fire_hook_" + PlayerJournalTracker.Safe(hook.HookId),
                MapMarkerType.QuestObjective,
                hook.WorldPosition,
                hook.Title,
                true,
                notes: hook.PlayerFacingSummary);
        }
    }
}
