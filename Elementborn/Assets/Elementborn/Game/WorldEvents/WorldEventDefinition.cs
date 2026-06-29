using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/World Events/World Event", fileName = "WorldEvent")]
    public sealed class WorldEventDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string eventId = "";
        [SerializeField] private string displayName = "World Event";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Classification")]
        [SerializeField] private WorldEventType eventType = WorldEventType.Unknown;
        [SerializeField] private WorldEventTriggerMode defaultTriggerMode = WorldEventTriggerMode.Manual;
        [SerializeField] private bool unique = true;
        [SerializeField] private bool important = false;

        [Header("Location")]
        [SerializeField] private string region = "";
        [SerializeField] private bool hasWorldPosition = true;
        [SerializeField] private Vector3 worldPosition;
        [SerializeField] private float activationRadius = 30f;

        [Header("Timing")]
        [SerializeField] private float activationDelaySeconds = 0f;
        [SerializeField] private float durationSeconds = 300f;
        [SerializeField] private float recurrenceSeconds = -1f;

        [Header("Requirements")]
        [SerializeField] private string requiredQuestId = "";
        [SerializeField] private string requiredRumorId = "";
        [SerializeField] private ElementbornFactionId relatedFaction = ElementbornFactionId.Unknown;
        [SerializeField] private int requiredFactionReputation = -999;

        [Header("Map / Rumor / Journal")]
        [SerializeField] private bool addMapMarker = true;
        [SerializeField] private MapMarkerType markerType = MapMarkerType.CustomPin;
        [SerializeField] private bool addRumor = true;
        [TextArea]
        [SerializeField] private string rumorText = "";
        [SerializeField] private bool addJournalEntry = true;

        [Header("Encounters")]
        [SerializeField] private List<DynamicEncounterDefinition> encounters = new List<DynamicEncounterDefinition>();

        public string EventId => string.IsNullOrWhiteSpace(eventId) ? name : eventId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? EventId : displayName;
        public string Description => description;
        public WorldEventType EventType => eventType;
        public WorldEventTriggerMode DefaultTriggerMode => defaultTriggerMode;
        public bool Unique => unique;
        public bool Important => important;
        public string Region => region;
        public bool HasWorldPosition => hasWorldPosition;
        public Vector3 WorldPosition => worldPosition;
        public float ActivationRadius => Mathf.Max(0f, activationRadius);
        public float ActivationDelaySeconds => Mathf.Max(0f, activationDelaySeconds);
        public float DurationSeconds => durationSeconds;
        public float RecurrenceSeconds => recurrenceSeconds;
        public string RequiredQuestId => requiredQuestId;
        public string RequiredRumorId => requiredRumorId;
        public ElementbornFactionId RelatedFaction => relatedFaction;
        public int RequiredFactionReputation => requiredFactionReputation;
        public bool AddMapMarker => addMapMarker;
        public MapMarkerType MarkerType => markerType;
        public bool AddRumor => addRumor;
        public string RumorText => rumorText;
        public bool AddJournalEntry => addJournalEntry;
        public IReadOnlyList<DynamicEncounterDefinition> Encounters => encounters;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(eventId)) eventId = name;
            activationRadius = Mathf.Max(0f, activationRadius);
            activationDelaySeconds = Mathf.Max(0f, activationDelaySeconds);
        }
    }
}
