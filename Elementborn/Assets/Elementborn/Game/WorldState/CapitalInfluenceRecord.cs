using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class CapitalInfluenceRecord
    {
        public NpcWorldEntryDefinition Npc;
        [Range(-100, 100)]
        public int Influence = 0;
        public CapitalPressureType PressureType = CapitalPressureType.FactionControl;
        [TextArea]
        public string Notes = "";
    }
}
