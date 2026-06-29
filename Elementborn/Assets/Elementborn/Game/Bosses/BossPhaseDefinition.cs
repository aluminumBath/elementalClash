using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class BossPhaseDefinition
    {
        [Range(0f, 1f)] public float StartAtHealthPercent = 1f;
        public string PhaseName = "Phase";
        public string PhaseAnnouncement = "";
        public Sprite PhaseIcon;
        public List<BossPhaseAction> Actions = new List<BossPhaseAction>();
    }
}
