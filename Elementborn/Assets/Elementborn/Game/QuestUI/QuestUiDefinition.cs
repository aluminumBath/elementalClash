using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Quest UI/Quest Definition", fileName = "QuestUiDefinition")]
    public sealed class QuestUiDefinition : ScriptableObject
    {
        [SerializeField] private string questId = "";
        [SerializeField] private string title = "Quest";
        [TextArea][SerializeField] private string description = "";
        [SerializeField] private string region = "";
        [SerializeField] private string giverName = "";
        [SerializeField] private bool autoTrack = true;
        [SerializeField] private Sprite icon;
        [SerializeField] private List<QuestObjectiveUiDefinition> objectives = new List<QuestObjectiveUiDefinition>();
        [SerializeField] private List<QuestRewardPreviewDefinition> rewards = new List<QuestRewardPreviewDefinition>();
        public string QuestId => string.IsNullOrWhiteSpace(questId) ? name : questId;
        public string Title => string.IsNullOrWhiteSpace(title) ? QuestId : title;
        public string Description => description;
        public string Region => region;
        public string GiverName => giverName;
        public bool AutoTrack => autoTrack;
        public Sprite Icon => icon;
        public IReadOnlyList<QuestObjectiveUiDefinition> Objectives => objectives;
        public IReadOnlyList<QuestRewardPreviewDefinition> Rewards => rewards;
        private void OnValidate(){ if(string.IsNullOrWhiteSpace(questId)) questId = name; }
    }
}
