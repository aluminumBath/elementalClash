using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class CapitalRuntimeState
    {
        public CapitalId CapitalId = CapitalId.Unknown;
        public CapitalControlStatus ControlStatus = CapitalControlStatus.Unknown;
        public int Stability = 50;
        public int Legitimacy = 50;
        public List<CapitalPressureRecord> Pressures = new List<CapitalPressureRecord>();

        public CapitalPressureRecord GetOrCreatePressure(CapitalPressureType type)
        {
            CapitalPressureRecord record = Pressures.Find(p => p != null && p.Type == type);
            if (record != null) return record;
            record = new CapitalPressureRecord { Type = type, Value = 0 };
            Pressures.Add(record);
            return record;
        }
    }
}
