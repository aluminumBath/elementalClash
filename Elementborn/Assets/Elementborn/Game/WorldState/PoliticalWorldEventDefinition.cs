using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/World State/Political World Event", fileName = "PoliticalWorldEvent")]
    public sealed class PoliticalWorldEventDefinition : ScriptableObject
    {
        [SerializeField] private string eventId = "";
        [SerializeField] private string displayName = "Political World Event";
        [SerializeField] private PoliticalWorldEventCategory category = PoliticalWorldEventCategory.Rumor;
        [SerializeField] private CapitalId primaryCapital = CapitalId.Unknown;
        [SerializeField] private Vector3 worldPosition;
        [SerializeField] private List<PoliticalWorldEventCondition> conditions = new List<PoliticalWorldEventCondition>();
        [SerializeField] private List<PoliticalWorldEventConsequence> consequences = new List<PoliticalWorldEventConsequence>();
        [SerializeField] private QuestUiDefinition questToStart;
        [SerializeField] private ElementbornSoundEventId sound = ElementbornSoundEventId.UiConfirm;
        [SerializeField] private int cooldownDays = 1;
        [SerializeField] private bool autoActivateWhenEligible = false;
        [SerializeField] private bool createMapMarker = true;
        [TextArea]
        [SerializeField] private string playerFacingSummary = "";
        [TextArea]
        [SerializeField] private string hiddenDirectorNotes = "";

        public string EventId => string.IsNullOrWhiteSpace(eventId) ? name : eventId;
        public string DisplayName => displayName;
        public PoliticalWorldEventCategory Category => category;
        public CapitalId PrimaryCapital => primaryCapital;
        public Vector3 WorldPosition => worldPosition;
        public IReadOnlyList<PoliticalWorldEventCondition> Conditions => conditions;
        public IReadOnlyList<PoliticalWorldEventConsequence> Consequences => consequences;
        public QuestUiDefinition QuestToStart => questToStart;
        public ElementbornSoundEventId Sound => sound;
        public int CooldownDays => Mathf.Max(0, cooldownDays);
        public bool AutoActivateWhenEligible => autoActivateWhenEligible;
        public bool CreateMapMarker => createMapMarker;
        public string PlayerFacingSummary => playerFacingSummary;
        public string HiddenDirectorNotes => hiddenDirectorNotes;

        public bool AreConditionsMet(CapitalWorldStateTracker tracker)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return false;
            }

            foreach (PoliticalWorldEventCondition condition in conditions)
            {
                if (condition == null || !condition.IsMet(tracker))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
