using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Quest Chains/Quest Chain", fileName = "QuestChain")]
    public sealed class QuestChainDefinition : ScriptableObject
    {
        [SerializeField] private string chainId = "";
        [SerializeField] private string displayName = "Quest Chain";
        [SerializeField] private CapitalId primaryCapital = CapitalId.Unknown;
        [SerializeField] private NpcWorldEntryDefinition primaryNpc;
        [SerializeField] private bool autoStartOnEvent = false;
        [TextArea]
        [SerializeField] private string summary = "";
        [SerializeField] private List<QuestChainStageDefinition> stages = new List<QuestChainStageDefinition>();

        public string ChainId => string.IsNullOrWhiteSpace(chainId) ? name : chainId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? ChainId : displayName;
        public CapitalId PrimaryCapital => primaryCapital;
        public NpcWorldEntryDefinition PrimaryNpc => primaryNpc;
        public bool AutoStartOnEvent => autoStartOnEvent;
        public string Summary => summary;
        public IReadOnlyList<QuestChainStageDefinition> Stages => stages;

        public QuestChainStageDefinition FindStage(string stageId)
        {
            return stages.Find(s => s != null && s.StageId == stageId);
        }

        public QuestChainStageDefinition FirstStage => stages != null && stages.Count > 0 ? stages[0] : null;
    }
}
