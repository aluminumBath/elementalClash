using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [System.Serializable]
    public class CapitalPressureChange
    {
        public CapitalPressureType PressureType = CapitalPressureType.Unrest;
        public int Delta = 0;
        [TextArea]
        public string Reason = "";
    }

    [CreateAssetMenu(menuName = "Elementborn/World State/Capital Pressure Event", fileName = "CapitalPressureEvent")]
    public sealed class CapitalPressureEventDefinition : ScriptableObject
    {
        [SerializeField] private string eventId = "";
        [SerializeField] private string displayName = "Capital Pressure Event";
        [SerializeField] private CapitalId targetCapital = CapitalId.Unknown;
        [SerializeField] private List<CapitalPressureChange> pressureChanges = new List<CapitalPressureChange>();
        [SerializeField] private int stabilityDelta = 0;
        [SerializeField] private int legitimacyDelta = 0;
        [TextArea]
        [SerializeField] private string journalText = "";
        [SerializeField] private bool notifyPlayer = true;

        public string EventId => string.IsNullOrWhiteSpace(eventId) ? name : eventId;
        public string DisplayName => displayName;
        public CapitalId TargetCapital => targetCapital;
        public IReadOnlyList<CapitalPressureChange> PressureChanges => pressureChanges;
        public int StabilityDelta => stabilityDelta;
        public int LegitimacyDelta => legitimacyDelta;
        public string JournalText => journalText;
        public bool NotifyPlayer => notifyPlayer;
    }
}
