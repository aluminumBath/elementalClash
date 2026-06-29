using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestObjectiveState
    {
        public string QuestId = "";
        public string ObjectiveId = "";
        public string Title = "Current Objective";
        public string Description = "";
        public Vector3 WorldPosition;
        public bool HasWorldPosition = true;
        public bool IsComplete = false;
    }
}
