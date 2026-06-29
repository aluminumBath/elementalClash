using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class HarvestRequirement
    {
        public HarvestToolType ToolType = HarvestToolType.Hands;
        public string RequiredToolItemId = "";
        public AbilityElementType RequiredElement = AbilityElementType.Neutral;
        public string RequiredAbilityId = "";
        public int RequiredFactionReputation = -999;
        public ElementbornFactionId RequiredFaction = ElementbornFactionId.Unknown;

        public bool IsMet(out string reason)
        {
            reason = "";

            if (!string.IsNullOrWhiteSpace(RequiredToolItemId)
                && !PlayerInventoryTracker.HasItemId(RequiredToolItemId, 1))
            {
                reason = $"Requires tool: {RequiredToolItemId}.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(RequiredAbilityId)
                && !PlayerAbilityTracker.HasUnlocked(RequiredAbilityId))
            {
                reason = $"Requires ability: {RequiredAbilityId}.";
                return false;
            }

            if (RequiredFaction != ElementbornFactionId.Unknown
                && RequiredFactionReputation > -999
                && FactionReputationTracker.GetValue(RequiredFaction) < RequiredFactionReputation)
            {
                reason = $"Requires {RequiredFaction} reputation {RequiredFactionReputation}.";
                return false;
            }

            return true;
        }
    }
}
