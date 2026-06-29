using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestUiRecord
    {
        public string QuestId = ""; public string Title = ""; public string Description = ""; public string Region = ""; public string GiverName = "";
        public QuestUiState State = QuestUiState.Available; public bool Tracked = false; public int ActiveObjectiveIndex = 0; public float LastUpdatedAtUnscaledTime = 0f;
        public List<QuestObjectiveUiRecord> Objectives = new List<QuestObjectiveUiRecord>();
        public List<QuestRewardPreviewDefinition> Rewards = new List<QuestRewardPreviewDefinition>();
        public QuestObjectiveUiRecord CurrentObjective => Objectives == null || Objectives.Count == 0 ? null : Objectives[Mathf.Clamp(ActiveObjectiveIndex,0,Objectives.Count-1)];
    }
}
