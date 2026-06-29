using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class CapitalWorldStateTracker : MonoBehaviour
    {
        public static CapitalWorldStateTracker Instance { get; private set; }

        [SerializeField] private List<CapitalWorldStateDefinition> capitalDefinitions = new List<CapitalWorldStateDefinition>();
        [SerializeField] private List<CapitalRuntimeState> runtimeStates = new List<CapitalRuntimeState>();
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool registerJournalAndMapOnStart = true;

        public IReadOnlyList<CapitalRuntimeState> RuntimeStates => runtimeStates;
        public IReadOnlyList<CapitalWorldStateDefinition> CapitalDefinitions => capitalDefinitions;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (initializeOnAwake)
            {
                InitializeFromDefinitions();
            }
        }

        private void Start()
        {
            if (registerJournalAndMapOnStart)
            {
                RegisterJournalAndMap();
            }
        }

        public static CapitalWorldStateTracker Ensure()
        {
            if (Instance != null) return Instance;
            GameObject go = new GameObject(nameof(CapitalWorldStateTracker));
            return go.AddComponent<CapitalWorldStateTracker>();
        }

        public void SetDefinitions(List<CapitalWorldStateDefinition> definitions)
        {
            capitalDefinitions = definitions ?? new List<CapitalWorldStateDefinition>();
            InitializeFromDefinitions();
        }

        public void InitializeFromDefinitions()
        {
            foreach (var definition in capitalDefinitions)
            {
                if (definition == null) continue;
                CapitalRuntimeState state = GetOrCreate(definition.CapitalId);
                state.ControlStatus = definition.ControlStatus;
                state.Legitimacy = StartingLegitimacy(definition.ControlStatus);

                foreach (CapitalPressureRecord pressure in definition.StartingPressures)
                {
                    if (pressure == null) continue;
                    CapitalPressureRecord runtimePressure = state.GetOrCreatePressure(pressure.Type);
                    runtimePressure.Value = pressure.Value;
                    runtimePressure.Notes = pressure.Notes;
                }

                RecalculateStability(state);
            }
        }

        public CapitalRuntimeState GetOrCreate(CapitalId capitalId)
        {
            CapitalRuntimeState state = runtimeStates.Find(s => s != null && s.CapitalId == capitalId);
            if (state != null) return state;
            state = new CapitalRuntimeState { CapitalId = capitalId };
            runtimeStates.Add(state);
            return state;
        }

        public CapitalWorldStateDefinition FindDefinition(CapitalId capitalId)
        {
            return capitalDefinitions.Find(d => d != null && d.CapitalId == capitalId);
        }

        public void ClearRuntimeStates()
        {
            runtimeStates.Clear();
        }

        public void ImportRuntimeState(CapitalRuntimeState state)
        {
            if (state == null)
            {
                return;
            }

            CapitalRuntimeState existing = runtimeStates.Find(s => s != null && s.CapitalId == state.CapitalId);
            if (existing != null)
            {
                runtimeStates.Remove(existing);
            }

            runtimeStates.Add(state);
        }

        public void ReplaceRuntimeStates(List<CapitalRuntimeState> states)
        {
            runtimeStates.Clear();
            if (states == null)
            {
                return;
            }

            foreach (CapitalRuntimeState state in states)
            {
                ImportRuntimeState(state);
            }
        }


        public void AddPressure(CapitalId capitalId, CapitalPressureType pressureType, int delta, string reason = "")
        {
            CapitalRuntimeState state = GetOrCreate(capitalId);
            CapitalPressureRecord pressure = state.GetOrCreatePressure(pressureType);
            pressure.Value = Mathf.Clamp(pressure.Value + delta, 0, 100);
            pressure.Notes = reason;
            RecalculateStability(state);
            NotificationFeed.Post($"{capitalId} {pressureType} changed by {delta}: {pressure.Value}. {reason}", NotificationType.Info);
        }

        public void AddLegitimacy(CapitalId capitalId, int delta, string reason = "")
        {
            CapitalRuntimeState state = GetOrCreate(capitalId);
            state.Legitimacy = Mathf.Clamp(state.Legitimacy + delta, 0, 100);
            RecalculateStability(state);
            NotificationFeed.Post($"{capitalId} legitimacy changed by {delta}: {state.Legitimacy}. {reason}", NotificationType.Info);
        }

        public void AddStability(CapitalId capitalId, int delta, string reason = "")
        {
            CapitalRuntimeState state = GetOrCreate(capitalId);
            state.Stability = Mathf.Clamp(state.Stability + delta, 0, 100);
            NotificationFeed.Post($"{capitalId} stability changed by {delta}: {state.Stability}. {reason}", NotificationType.Info);
        }

        public void ApplyEvent(CapitalPressureEventDefinition evt)
        {
            if (evt == null) return;

            foreach (CapitalPressureChange change in evt.PressureChanges)
            {
                if (change != null)
                {
                    AddPressure(evt.TargetCapital, change.PressureType, change.Delta, change.Reason);
                }
            }

            if (evt.StabilityDelta != 0) AddStability(evt.TargetCapital, evt.StabilityDelta, evt.DisplayName);
            if (evt.LegitimacyDelta != 0) AddLegitimacy(evt.TargetCapital, evt.LegitimacyDelta, evt.DisplayName);

            if (!string.IsNullOrWhiteSpace(evt.JournalText))
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "capital_event_" + PlayerJournalTracker.Safe(evt.EventId),
                    JournalEntryType.Quest,
                    evt.DisplayName,
                    evt.JournalText,
                    evt.TargetCapital.ToString(),
                    evt.EventId);
            }

            if (evt.NotifyPlayer)
            {
                NotificationFeed.Post(evt.DisplayName, NotificationType.Quest);
            }
        }

        public void SyncRegionalSystems()
        {
            ReligiousFervorTracker wind = ReligiousFervorTracker.Ensure();
            CapitalRuntimeState windState = GetOrCreate(CapitalId.WindCapital);
            windState.GetOrCreatePressure(CapitalPressureType.ReligiousFervor).Value = wind.Fervor;
            windState.ControlStatus = CapitalControlStatus.Usurped;
            RecalculateStability(windState);

            ThievesGuildReputationTracker guild = ThievesGuildReputationTracker.Ensure();
            CapitalRuntimeState metal = GetOrCreate(CapitalId.MetalCapital);
            int guildPressure = Mathf.Clamp(50 + guild.Reputation, 0, 100);
            metal.GetOrCreatePressure(CapitalPressureType.ThievesGuildInfluence).Value = guildPressure;
            metal.GetOrCreatePressure(CapitalPressureType.BlackMarketPressure).Value =
                Mathf.Max(metal.GetOrCreatePressure(CapitalPressureType.BlackMarketPressure).Value, 70);
            RecalculateStability(metal);
        }

        public void RegisterJournalAndMap()
        {
            foreach (var definition in capitalDefinitions)
            {
                if (definition == null) continue;
                CapitalRuntimeState state = GetOrCreate(definition.CapitalId);
                string summary = BuildCapitalSummary(definition.CapitalId);

                PlayerJournalTracker.AddOrUpdateEntry(
                    "capital_state_" + PlayerJournalTracker.Safe(definition.CapitalId.ToString()),
                    JournalEntryType.Location,
                    definition.DisplayName,
                    summary,
                    definition.DisplayName,
                    definition.CapitalId.ToString());

                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "capital_state_" + PlayerJournalTracker.Safe(definition.CapitalId.ToString()),
                    MapMarkerType.GuideNpc,
                    definition.WorldPosition,
                    definition.DisplayName,
                    true,
                    $"Stability {state.Stability}, legitimacy {state.Legitimacy}, control {state.ControlStatus}");
            }
        }

        public string BuildCapitalSummary(CapitalId capitalId)
        {
            CapitalRuntimeState state = GetOrCreate(capitalId);
            CapitalWorldStateDefinition definition = FindDefinition(capitalId);

            var sb = new System.Text.StringBuilder();
            if (definition != null)
            {
                sb.AppendLine(definition.Summary);
                sb.AppendLine($"Ruling faction: {definition.RulingFaction}");
            }

            sb.AppendLine($"Control: {state.ControlStatus}");
            sb.AppendLine($"Stability: {state.Stability}");
            sb.AppendLine($"Legitimacy: {state.Legitimacy}");
            sb.AppendLine("Pressures:");
            foreach (CapitalPressureRecord pressure in state.Pressures)
            {
                if (pressure != null)
                {
                    sb.AppendLine($"- {pressure.Type}: {pressure.Value} ({pressure.Severity}) {pressure.Notes}");
                }
            }

            return sb.ToString();
        }

        public string BuildWorldSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Capital World State");
            foreach (CapitalRuntimeState state in runtimeStates)
            {
                if (state != null)
                {
                    sb.AppendLine($"- {state.CapitalId}: stability {state.Stability}, legitimacy {state.Legitimacy}, control {state.ControlStatus}");
                }
            }
            return sb.ToString();
        }

        private void RecalculateStability(CapitalRuntimeState state)
        {
            int pressureTotal = 0;
            int counted = 0;
            foreach (CapitalPressureRecord pressure in state.Pressures)
            {
                if (pressure == null) continue;
                if (pressure.Type == CapitalPressureType.RulerLegitimacy || pressure.Type == CapitalPressureType.RoyalFamilyStability) continue;
                pressureTotal += pressure.Value;
                counted++;
            }

            int pressureAverage = counted > 0 ? pressureTotal / counted : 0;
            state.Stability = Mathf.Clamp((state.Legitimacy + 100 - pressureAverage) / 2, 0, 100);

            if (state.Stability < 20) state.ControlStatus = CapitalControlStatus.Collapsing;
            else if (state.Stability < 35 && state.ControlStatus == CapitalControlStatus.StableRule) state.ControlStatus = CapitalControlStatus.Contested;
        }

        private int StartingLegitimacy(CapitalControlStatus status)
        {
            switch (status)
            {
                case CapitalControlStatus.StableRule: return 75;
                case CapitalControlStatus.Contested: return 45;
                case CapitalControlStatus.Usurped: return 30;
                case CapitalControlStatus.ShadowControlled: return 25;
                case CapitalControlStatus.Rebellion: return 35;
                case CapitalControlStatus.Collapsing: return 10;
                default: return 50;
            }
        }
    }
}
