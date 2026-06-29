using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class AdminQuestObjectiveRecord
    {
        public string ObjectiveId = "objective";
        public string Title = "Objective";
        [TextArea]
        public string Description = "";
        public bool HasWorldPosition = true;
        public float X;
        public float Y;
        public float Z;
        public bool CompleteOnReach = false;

        public Vector3 WorldPosition
        {
            get => new Vector3(X, Y, Z);
            set
            {
                X = value.x;
                Y = value.y;
                Z = value.z;
            }
        }
    }
}
