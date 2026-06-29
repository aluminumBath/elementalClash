using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class CapitalPressureRecord
    {
        public CapitalPressureType Type = CapitalPressureType.Unrest;
        [Range(0, 100)]
        public int Value = 0;
        [TextArea]
        public string Notes = "";

        public CapitalPressureSeverity Severity
        {
            get
            {
                if (Value <= 0) return CapitalPressureSeverity.None;
                if (Value < 25) return CapitalPressureSeverity.Low;
                if (Value < 55) return CapitalPressureSeverity.Moderate;
                if (Value < 80) return CapitalPressureSeverity.High;
                return CapitalPressureSeverity.Critical;
            }
        }
    }
}
