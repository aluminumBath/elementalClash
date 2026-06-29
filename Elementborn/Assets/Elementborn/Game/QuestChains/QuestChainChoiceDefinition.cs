using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestChainChoiceDefinition
    {
        public string ChoiceId = "";
        public string DisplayText = "Choice";
        public QuestChainChoiceType ChoiceType = QuestChainChoiceType.Diplomatic;
        public string NextStageId = "";
        [TextArea]
        public string PlayerFacingResult = "";
        [TextArea]
        public string HiddenDirectorNotes = "";
        public List<QuestChainConsequenceDefinition> Consequences = new List<QuestChainConsequenceDefinition>();
    }
}
