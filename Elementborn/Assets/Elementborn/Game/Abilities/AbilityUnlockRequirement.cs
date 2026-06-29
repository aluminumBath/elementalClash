using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class AbilityUnlockRequirement
    {
        [Header("Progression")]
        public int RequiredPlayerLevel = 0;
        public int SkillPointCost = 1;
        public string RequiredAbilityId = "";

        [Header("Quest")]
        public string RequiredQuestId = "";

        [Header("Faction")]
        public ElementbornFactionId RequiredFaction = ElementbornFactionId.Unknown;
        public int RequiredFactionReputation = 0;

        [Header("Creature Bond")]
        public CreatureBondStage RequiredCreatureBondStage = CreatureBondStage.Wary;
        public bool RequiresOwnedCreature = false;

        [Header("Inventory")]
        public string RequiredItemId = "";
        public int RequiredItemQuantity = 1;
        public bool ConsumeRequiredItem = false;

        [Header("Notes")]
        [TextArea]
        public string RequirementHint = "";

        public bool IsMet(out string reason)
        {
            reason = "";

            if (!string.IsNullOrWhiteSpace(RequiredAbilityId)
                && !PlayerAbilityTracker.HasUnlocked(RequiredAbilityId))
            {
                reason = $"Requires ability: {RequiredAbilityId}.";
                return false;
            }

            if (RequiredFaction != ElementbornFactionId.Unknown
                && FactionReputationTracker.GetValue(RequiredFaction) < RequiredFactionReputation)
            {
                reason = $"Requires {RequiredFaction} reputation {RequiredFactionReputation}.";
                return false;
            }

            if (RequiresOwnedCreature)
            {
                var creature = CreatureBondingTracker.GetFirstAvailable();
                if (creature == null)
                {
                    reason = "Requires an owned creature.";
                    return false;
                }

                if (creature.BondStage < RequiredCreatureBondStage)
                {
                    reason = $"Requires creature bond: {RequiredCreatureBondStage}.";
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(RequiredItemId)
                && !PlayerInventoryTracker.HasItemId(RequiredItemId, Mathf.Max(1, RequiredItemQuantity)))
            {
                reason = $"Requires {RequiredItemId} x{Mathf.Max(1, RequiredItemQuantity)}.";
                return false;
            }

            return true;
        }
    }
}
