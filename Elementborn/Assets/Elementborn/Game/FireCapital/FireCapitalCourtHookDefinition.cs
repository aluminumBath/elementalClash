using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Fire Capital/Court Hook", fileName = "FireCapitalCourtHook")]
    public sealed class FireCapitalCourtHookDefinition : ScriptableObject
    {
        [SerializeField] private string hookId = "";
        [SerializeField] private string title = "Fire Capital Hook";
        [SerializeField] private FireCapitalHookType hookType = FireCapitalHookType.Unknown;
        [SerializeField] private FireCapitalDistrict district = FireCapitalDistrict.Unknown;
        [SerializeField] private NpcWorldEntryDefinition primaryNpc;
        [SerializeField] private QuestUiDefinition questToStart;
        [SerializeField] private Vector3 worldPosition;
        [SerializeField] private CapitalPressureType pressureType = CapitalPressureType.Unrest;
        [SerializeField] private int pressureDeltaOnStart = 0;
        [SerializeField] private int stabilityDeltaOnResolve = 0;
        [TextArea]
        [SerializeField] private string playerFacingSummary = "";
        [TextArea]
        [SerializeField] private string hiddenDirectorNotes = "";

        public string HookId => string.IsNullOrWhiteSpace(hookId) ? name : hookId;
        public string Title => string.IsNullOrWhiteSpace(title) ? HookId : title;
        public FireCapitalHookType HookType => hookType;
        public FireCapitalDistrict District => district;
        public NpcWorldEntryDefinition PrimaryNpc => primaryNpc;
        public QuestUiDefinition QuestToStart => questToStart;
        public Vector3 WorldPosition => worldPosition;
        public CapitalPressureType PressureType => pressureType;
        public int PressureDeltaOnStart => pressureDeltaOnStart;
        public int StabilityDeltaOnResolve => stabilityDeltaOnResolve;
        public string PlayerFacingSummary => playerFacingSummary;
        public string HiddenDirectorNotes => hiddenDirectorNotes;
    }
}
