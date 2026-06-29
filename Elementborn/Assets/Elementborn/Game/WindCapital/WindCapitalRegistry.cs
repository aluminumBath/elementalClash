using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class WindCapitalRegistry : MonoBehaviour
    {
        public static WindCapitalRegistry Instance { get; private set; }

        [SerializeField] private List<WindCapitalIntrigueHookDefinition> hooks = new List<WindCapitalIntrigueHookDefinition>();
        [SerializeField] private bool registerJournalEntriesOnStart = true;
        [SerializeField] private bool registerMapMarkersOnStart = true;

        public IReadOnlyList<WindCapitalIntrigueHookDefinition> Hooks => hooks;

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
            if (registerJournalEntriesOnStart)
            {
                RegisterJournalEntries();
            }

            if (registerMapMarkersOnStart)
            {
                RegisterMapMarkers();
            }
        }

        public static WindCapitalRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(WindCapitalRegistry));
            return go.AddComponent<WindCapitalRegistry>();
        }

        public void SetHooks(List<WindCapitalIntrigueHookDefinition> values)
        {
            hooks = values ?? new List<WindCapitalIntrigueHookDefinition>();
        }

        public WindCapitalIntrigueHookDefinition FindHook(string id)
        {
            return hooks.Find(h => h != null && h.HookId == id);
        }

        public void RegisterJournalEntries()
        {
            foreach (var hook in hooks)
            {
                if (hook == null)
                {
                    continue;
                }

                PlayerJournalTracker.AddOrUpdateEntry(
                    "wind_hook_" + PlayerJournalTracker.Safe(hook.HookId),
                    JournalEntryType.Quest,
                    hook.Title,
                    hook.Summary,
                    "Wind Capital",
                    hook.HookId);
            }
        }

        public void RegisterMapMarkers()
        {
            foreach (var hook in hooks)
            {
                if (hook == null || !hook.CreatesMapMarker)
                {
                    continue;
                }

                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "wind_hook_" + PlayerJournalTracker.Safe(hook.HookId),
                    MapMarkerType.QuestObjective,
                    hook.WorldPosition,
                    hook.Title,
                    isPersistent: true,
                    notes: hook.Summary);
            }
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(ReligiousFervorTracker.Ensure().BuildSummary());
            sb.AppendLine($"Wind Capital Hooks: {hooks.Count}");
            foreach (var hook in hooks)
            {
                if (hook != null)
                {
                    sb.AppendLine($"- {hook.Title} [{hook.HookType}] {hook.District}");
                }
            }
            return sb.ToString();
        }
    }
}
