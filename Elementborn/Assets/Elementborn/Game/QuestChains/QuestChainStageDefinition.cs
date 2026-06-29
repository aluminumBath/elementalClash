using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestChainStageDefinition
    {
        public string StageId = "";
        public string Title = "Quest Chain Stage";
        [TextArea]
        public string Summary = "";
        public QuestUiDefinition Quest;
        public PoliticalWorldEventDefinition TriggeredByPoliticalEvent;
        public bool AutoStartQuest = true;
        public bool AutoCompleteWhenQuestCompletes = false;
        public string DefaultNextStageId = "";
        public List<QuestChainConsequenceDefinition> OnStartConsequences = new List<QuestChainConsequenceDefinition>();
        public List<QuestChainConsequenceDefinition> OnCompleteConsequences = new List<QuestChainConsequenceDefinition>();
        public List<QuestChainChoiceDefinition> Choices = new List<QuestChainChoiceDefinition>();
    }
}
