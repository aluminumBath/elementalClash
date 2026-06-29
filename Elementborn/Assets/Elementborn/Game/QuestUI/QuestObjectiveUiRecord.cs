using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestObjectiveUiRecord
    {
        public string ObjectiveId = ""; public string Title = ""; public string Description = "";
        public QuestUiState State = QuestUiState.Active; public float X; public float Y; public float Z;
        public bool CreateWaypoint = true; public bool Required = true;
        public Vector3 WorldPosition { get => new Vector3(X,Y,Z); set { X=value.x; Y=value.y; Z=value.z; } }
    }
}
