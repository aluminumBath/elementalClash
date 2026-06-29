using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class AdminQuestRecord
    {
        public string QuestId = "custom_quest";
        public string Title = "Custom Quest";
        [TextArea]
        public string Description = "";
        public string Region = "";
        public string GiverName = "Admin";
        public bool Repeatable = false;
        public bool Enabled = true;
        public List<AdminQuestObjectiveRecord> Objectives = new List<AdminQuestObjectiveRecord>();
        public List<AdminQuestRewardRecord> Rewards = new List<AdminQuestRewardRecord>();

        public AdminQuestObjectiveRecord FirstObjective
        {
            get
            {
                if (Objectives == null || Objectives.Count == 0)
                {
                    return null;
                }

                return Objectives[0];
            }
        }
    }
}
