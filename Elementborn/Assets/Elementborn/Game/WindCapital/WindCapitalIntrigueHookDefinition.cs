using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Wind Capital/Intrigue Hook", fileName = "WindCapitalIntrigueHook")]
    public sealed class WindCapitalIntrigueHookDefinition : ScriptableObject
    {
        [SerializeField] private string hookId = "";
        [SerializeField] private string title = "Wind Capital Hook";
        [SerializeField] private WindCapitalHookType hookType = WindCapitalHookType.Unknown;
        [SerializeField] private WindCapitalDistrict district = WindCapitalDistrict.HighAerieCathedral;
        [SerializeField] private NpcWorldEntryDefinition primaryNpc;
        [SerializeField] private NpcWorldEntryDefinition secondaryNpc;
        [SerializeField] private QuestUiDefinition quest;
        [SerializeField] private Vector3 worldPosition;
        [TextArea]
        [SerializeField] private string summary = "";
        [TextArea]
        [SerializeField] private string secretTruth = "";
        [SerializeField] private List<WindCapitalRumorDefinition> rumors = new List<WindCapitalRumorDefinition>();
        [SerializeField] private int fervorDelta = 0;
        [SerializeField] private bool createsMapMarker = true;

        public string HookId => string.IsNullOrWhiteSpace(hookId) ? name : hookId;
        public string Title => title;
        public WindCapitalHookType HookType => hookType;
        public WindCapitalDistrict District => district;
        public NpcWorldEntryDefinition PrimaryNpc => primaryNpc;
        public NpcWorldEntryDefinition SecondaryNpc => secondaryNpc;
        public QuestUiDefinition Quest => quest;
        public Vector3 WorldPosition => worldPosition;
        public string Summary => summary;
        public string SecretTruth => secretTruth;
        public IReadOnlyList<WindCapitalRumorDefinition> Rumors => rumors;
        public int FervorDelta => fervorDelta;
        public bool CreatesMapMarker => createsMapMarker;
    }
}
