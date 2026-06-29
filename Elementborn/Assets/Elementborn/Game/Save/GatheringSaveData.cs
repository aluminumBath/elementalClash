using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class GatheringSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public int TotalHarvests = 0;
        public int RareHarvests = 0;
        public List<string> DiscoveredNodeIds = new List<string>();
        public List<HarvestNodeSaveRecord> Nodes = new List<HarvestNodeSaveRecord>();
    }

    [Serializable]
    public class HarvestNodeSaveRecord
    {
        public string RuntimeNodeId = "";
        public HarvestNodeState State = HarvestNodeState.Available;
        public int HarvestsRemaining = 1;
        public float RespawnAtTime = -1f;
    }
}
