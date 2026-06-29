using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestChainStageRuntimeRecord
    {
        public string StageId = "";
        public QuestChainStageState State = QuestChainStageState.Locked;
        public string ChoiceId = "";
    }

    [Serializable]
    public class QuestChainRuntimeRecord
    {
        public string ChainId = "";
        public bool Started = false;
        public bool Completed = false;
        public string ActiveStageId = "";
        public List<QuestChainStageRuntimeRecord> Stages = new List<QuestChainStageRuntimeRecord>();

        public QuestChainStageRuntimeRecord GetOrCreateStage(string stageId)
        {
            QuestChainStageRuntimeRecord stage = Stages.Find(s => s != null && s.StageId == stageId);
            if (stage != null)
            {
                return stage;
            }

            stage = new QuestChainStageRuntimeRecord { StageId = stageId };
            Stages.Add(stage);
            return stage;
        }
    }
}
