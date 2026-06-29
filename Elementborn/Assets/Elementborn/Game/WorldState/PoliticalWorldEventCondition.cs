using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class PoliticalWorldEventCondition
    {
        public CapitalId Capital = CapitalId.Unknown;
        public CapitalPressureType PressureType = CapitalPressureType.Unrest;
        [Range(0, 100)]
        public int MinimumValue = 50;
        public bool RequireAtOrAbove = true;

        public bool IsMet(CapitalWorldStateTracker tracker)
        {
            if (tracker == null)
            {
                return false;
            }

            CapitalRuntimeState state = tracker.GetOrCreate(Capital);
            CapitalPressureRecord pressure = state.GetOrCreatePressure(PressureType);
            return RequireAtOrAbove ? pressure.Value >= MinimumValue : pressure.Value <= MinimumValue;
        }

        public string Describe()
        {
            string comparison = RequireAtOrAbove ? ">=" : "<=";
            return $"{Capital}.{PressureType} {comparison} {MinimumValue}";
        }
    }
}
