using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/World State/Capital World State", fileName = "CapitalWorldState")]
    public sealed class CapitalWorldStateDefinition : ScriptableObject
    {
        [SerializeField] private CapitalId capitalId = CapitalId.Unknown;
        [SerializeField] private string displayName = "Capital";
        [SerializeField] private ElementbornFactionId rulingFaction = ElementbornFactionId.Unknown;
        [SerializeField] private CapitalControlStatus controlStatus = CapitalControlStatus.Unknown;
        [SerializeField] private Vector3 worldPosition;
        [TextArea]
        [SerializeField] private string summary = "";
        [SerializeField] private List<CapitalPressureRecord> startingPressures = new List<CapitalPressureRecord>();
        [SerializeField] private List<CapitalInfluenceRecord> npcInfluences = new List<CapitalInfluenceRecord>();

        public CapitalId CapitalId => capitalId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? capitalId.ToString() : displayName;
        public ElementbornFactionId RulingFaction => rulingFaction;
        public CapitalControlStatus ControlStatus => controlStatus;
        public Vector3 WorldPosition => worldPosition;
        public string Summary => summary;
        public IReadOnlyList<CapitalPressureRecord> StartingPressures => startingPressures;
        public IReadOnlyList<CapitalInfluenceRecord> NpcInfluences => npcInfluences;
    }
}
