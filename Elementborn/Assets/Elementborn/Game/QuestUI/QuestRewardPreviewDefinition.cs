using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestRewardPreviewDefinition
    {
        public string RewardId = "";
        public string DisplayName = "Reward";
        public string ItemId = "";
        public int Quantity = 1;
        public int Currency = 0;
        public int SkillPoints = 0;
        public Sprite Icon;
    }
}
