using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Bosses/Boss Definition", fileName = "BossDefinition")]
    public sealed class BossDefinition : ScriptableObject
    {
        [SerializeField] private string bossId = "";
        [SerializeField] private string displayName = "Boss";
        [TextArea] [SerializeField] private string description = "";
        [SerializeField] private string region = "";
        [SerializeField] private Vector3 mapPosition;
        [SerializeField] private bool addMapMarker = true;
        [SerializeField] private MapMarkerType mapMarkerType = MapMarkerType.BossLair;
        [SerializeField] private LootDropTableDefinition lootTable;
        [SerializeField] private int currencyReward = 0;
        [SerializeField] private int skillPointReward = 0;
        [SerializeField] private string questIdOnDefeat = "";
        [SerializeField] private string objectiveIdOnDefeat = "";
        [SerializeField] private List<BossPhaseDefinition> phases = new List<BossPhaseDefinition>();
        [SerializeField] private Sprite icon;
        [SerializeField] private string introMessage = "";
        [SerializeField] private string defeatMessage = "";
        public string BossId => string.IsNullOrWhiteSpace(bossId) ? name : bossId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? BossId : displayName;
        public string Description => description; public string Region => region; public Vector3 MapPosition => mapPosition;
        public bool AddMapMarker => addMapMarker; public MapMarkerType MarkerType => mapMarkerType; public LootDropTableDefinition LootTable => lootTable;
        public int CurrencyReward => Mathf.Max(0, currencyReward); public int SkillPointReward => Mathf.Max(0, skillPointReward);
        public string QuestIdOnDefeat => questIdOnDefeat; public string ObjectiveIdOnDefeat => objectiveIdOnDefeat; public IReadOnlyList<BossPhaseDefinition> Phases => phases;
        public Sprite Icon => icon; public string IntroMessage => introMessage; public string DefeatMessage => defeatMessage;
        private void OnValidate(){ if(string.IsNullOrWhiteSpace(bossId)) bossId=name; currencyReward=Mathf.Max(0,currencyReward); skillPointReward=Mathf.Max(0,skillPointReward); }
    }
}
