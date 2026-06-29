using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class EnemyAiSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<EnemyAiSaveRecord> Enemies = new List<EnemyAiSaveRecord>();
    }

    [Serializable]
    public class EnemyAiSaveRecord
    {
        public string RuntimeId = "";
        public EnemyAiState State = EnemyAiState.Idle;
        public float X;
        public float Y;
        public float Z;
    }
}
