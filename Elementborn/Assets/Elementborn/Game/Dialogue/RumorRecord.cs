using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class RumorRecord
    {
        public string RumorId = "";
        public RumorType Type = RumorType.Unknown;
        public string Text = "";
        public string Source = "";
        public string Region = "";
        public Vector3 WorldPosition;
        public bool HasWorldPosition;
        public bool IsTrue = true;
        public bool PlayerKnows = true;
        public bool Important = false;
        public float CreatedAtUnscaledTime;
    }
}
