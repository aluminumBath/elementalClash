using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestObjectiveUiDefinition
    {
        public string ObjectiveId = "";
        public string Title = "Objective";
        [TextArea] public string Description = "";
        public Vector3 WorldPosition;
        public bool CreateWaypoint = true;
        public bool Required = true;
    }
}
