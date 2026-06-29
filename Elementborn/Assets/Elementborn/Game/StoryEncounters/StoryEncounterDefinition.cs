using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Story Encounters/Story Encounter", fileName = "StoryEncounter")]
    public sealed class StoryEncounterDefinition : ScriptableObject
    {
        [SerializeField] private string encounterId = "";
        [SerializeField] private string displayName = "Story Encounter";
        [SerializeField] private StoryEncounterThreatType threatType = StoryEncounterThreatType.Unknown;
        [SerializeField] private CapitalId relatedCapital = CapitalId.Unknown;
        [SerializeField] private NpcWorldEntryDefinition primaryNpc;
        [SerializeField] private NpcWorldEntryDefinition secondaryNpc;
        [SerializeField] private Vector3 worldPosition;
        [TextArea]
        [SerializeField] private string playerFacingSummary = "";
        [TextArea]
        [SerializeField] private string mechanicsNotes = "";
        [TextArea]
        [SerializeField] private string directorNotes = "";

        public string EncounterId => string.IsNullOrWhiteSpace(encounterId) ? name : encounterId;
        public string DisplayName => displayName;
        public StoryEncounterThreatType ThreatType => threatType;
        public CapitalId RelatedCapital => relatedCapital;
        public NpcWorldEntryDefinition PrimaryNpc => primaryNpc;
        public NpcWorldEntryDefinition SecondaryNpc => secondaryNpc;
        public Vector3 WorldPosition => worldPosition;
        public string PlayerFacingSummary => playerFacingSummary;
        public string MechanicsNotes => mechanicsNotes;
        public string DirectorNotes => directorNotes;
    }
}
