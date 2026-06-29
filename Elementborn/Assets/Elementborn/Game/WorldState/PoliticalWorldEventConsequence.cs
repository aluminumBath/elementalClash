using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class PoliticalWorldEventConsequence
    {
        public CapitalPressureType PressureType = CapitalPressureType.Unrest;
        public int PressureDelta = 0;
        public int StabilityDelta = 0;
        public int LegitimacyDelta = 0;
        [TextArea]
        public string Reason = "";

        public void Apply(CapitalWorldStateTracker tracker, CapitalId capital)
        {
            if (tracker == null)
            {
                return;
            }

            if (PressureDelta != 0)
            {
                tracker.AddPressure(capital, PressureType, PressureDelta, Reason);
            }

            if (StabilityDelta != 0)
            {
                tracker.AddStability(capital, StabilityDelta, Reason);
            }

            if (LegitimacyDelta != 0)
            {
                tracker.AddLegitimacy(capital, LegitimacyDelta, Reason);
            }
        }
    }
}
