using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/NPC/NPC World Entry", fileName = "NpcWorldEntry")]
    public sealed class NpcWorldEntryDefinition : ScriptableObject
    {
        [SerializeField] private string npcId = "";
        [SerializeField] private string displayName = "NPC";
        [SerializeField] private NpcWorldRole role = NpcWorldRole.Unknown;
        [SerializeField] private string titleOrRank = "";
        [SerializeField] private string region = "";
        [SerializeField] private string locationName = "";
        [SerializeField] private Vector3 worldPosition;
        [SerializeField] private string aliases = "";
        [SerializeField] private string primaryElement = "";
        [SerializeField] private string secondaryElement = "";
        [SerializeField] private string origin = "";
        [TextArea]
        [SerializeField] private string appearanceNotes = "";
        [TextArea]
        [SerializeField] private string personalityNotes = "";
        [TextArea]
        [SerializeField] private string relationshipSummary = "";
        [TextArea]
        [SerializeField] private string notes = "";
        [SerializeField] private ElementbornFactionId faction = ElementbornFactionId.NeutralCity;
        [SerializeField] private ElementbornSoundEventId defaultVoiceSound = ElementbornSoundEventId.NpcVoiceWarm;
        [SerializeField] private List<NpcVoiceLineDefinition> voiceLines = new List<NpcVoiceLineDefinition>();

        public string NpcId => string.IsNullOrWhiteSpace(npcId) ? name : npcId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? NpcId : displayName;
        public NpcWorldRole Role => role;
        public string TitleOrRank => titleOrRank;
        public string Region => region;
        public string LocationName => locationName;
        public Vector3 WorldPosition => worldPosition;
        public string Aliases => aliases;
        public string PrimaryElement => primaryElement;
        public string SecondaryElement => secondaryElement;
        public string Origin => origin;
        public string AppearanceNotes => appearanceNotes;
        public string PersonalityNotes => personalityNotes;
        public string RelationshipSummary => relationshipSummary;
        public string Notes => notes;
        public ElementbornFactionId Faction => faction;
        public ElementbornSoundEventId DefaultVoiceSound => defaultVoiceSound;
        public IReadOnlyList<NpcVoiceLineDefinition> VoiceLines => voiceLines;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(npcId))
            {
                npcId = name;
            }
        }
    }
}
