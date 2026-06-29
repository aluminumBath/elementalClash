using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Social NPC/Social Group Event", fileName = "SocialGroupEvent")]
    public sealed class SocialGroupEventDefinition : ScriptableObject
    {
        [SerializeField] private string eventId = "";
        [SerializeField] private string displayName = "Social Group Event";
        [SerializeField] private SocialGroupEventType eventType = SocialGroupEventType.Hangout;
        [SerializeField] private SocialNpcGroupDefinition group;
        [SerializeField] private QuestUiDefinition questToStart;
        [SerializeField] private CapitalPressureType pressureType = CapitalPressureType.Unrest;
        [SerializeField] private int pressureDelta = 0;
        [SerializeField] private bool createJournalEntry = true;
        [SerializeField] private bool createMapMarker = true;
        [TextArea]
        [SerializeField] private string playerFacingSummary = "";
        [TextArea]
        [SerializeField] private string directorNotes = "";
        [SerializeField] private List<string> involvedNpcIds = new List<string>();

        public string EventId => string.IsNullOrWhiteSpace(eventId) ? name : eventId;
        public string DisplayName => displayName;
        public SocialGroupEventType EventType => eventType;
        public SocialNpcGroupDefinition Group => group;
        public QuestUiDefinition QuestToStart => questToStart;
        public CapitalPressureType PressureType => pressureType;
        public int PressureDelta => pressureDelta;
        public bool CreateJournalEntry => createJournalEntry;
        public bool CreateMapMarker => createMapMarker;
        public string PlayerFacingSummary => playerFacingSummary;
        public string DirectorNotes => directorNotes;
        public IReadOnlyList<string> InvolvedNpcIds => involvedNpcIds;
    }
}
