using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Metal Capital/Intrigue Hook", fileName = "MetalCapitalIntrigueHook")]
    public sealed class MetalCapitalIntrigueHookDefinition : ScriptableObject
    {
        [SerializeField] private string hookId = "";
        [SerializeField] private string title = "Intrigue Hook";
        [SerializeField] private MetalCapitalDistrict district = MetalCapitalDistrict.BlackMarket;
        [SerializeField] private NpcWorldEntryDefinition primaryNpc;
        [SerializeField] private NpcWorldEntryDefinition secondaryNpc;
        [SerializeField] private QuestUiDefinition quest;
        [TextArea]
        [SerializeField] private string summary = "";
        [TextArea]
        [SerializeField] private string secretTruth = "";
        [TextArea]
        [SerializeField] private string playerFacingRumor = "";
        [SerializeField] private int thievesGuildReputationDelta = 0;
        [SerializeField] private bool createsMapMarker = true;

        public string HookId => string.IsNullOrWhiteSpace(hookId) ? name : hookId;
        public string Title => title;
        public MetalCapitalDistrict District => district;
        public NpcWorldEntryDefinition PrimaryNpc => primaryNpc;
        public NpcWorldEntryDefinition SecondaryNpc => secondaryNpc;
        public QuestUiDefinition Quest => quest;
        public string Summary => summary;
        public string SecretTruth => secretTruth;
        public string PlayerFacingRumor => playerFacingRumor;
        public int ThievesGuildReputationDelta => thievesGuildReputationDelta;
        public bool CreatesMapMarker => createsMapMarker;
    }
}
