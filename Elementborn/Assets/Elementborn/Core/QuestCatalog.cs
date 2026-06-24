using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>The hand-authored starter quests, one per guide NPC. Objectives map to real gameplay events
    /// (defeating, taming, gathering currency, talking to an NPC), so they advance from play. Add more here.</summary>
    public static class QuestCatalog
    {
        public static IReadOnlyList<QuestDefinition> All { get; } = Build();

        private static IReadOnlyList<QuestDefinition> Build()
        {
            return new List<QuestDefinition>
            {
                new QuestDefinition(
                    "willow_first_hunt", "A Wild Start",
                    "Willow wants to see you handle yourself out there before pointing you to anything rarer.",
                    GuideNpcId.Willow.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.DefeatCreature, "", 3, "Defeat 3 wild creatures"),
                    },
                    new QuestReward(Currency.Silver, 50, "Willow nods. \"You'll do.\"", sigils: 120)),

                new QuestDefinition(
                    "kiana_companion", "A Gentle Hand",
                    "Kiana thinks you're ready to win a creature over rather than just fight it.",
                    GuideNpcId.Kiana.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.TameCreature, "", 1, "Tame any creature"),
                    },
                    new QuestReward(Currency.Ruby, 1, "Kiana beams. \"A friend for life.\"", sigils: 200)),

                new QuestDefinition(
                    "parfa_errand", "Word to Kiana",
                    "Parfa needs word carried to Kiana — and wants to know you can earn your keep on the way.",
                    GuideNpcId.Parfa.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.TalkToNpc, GuideNpcId.Kiana.ToString(), 1, "Speak with Kiana"),
                        new QuestObjective(ObjectiveKind.CollectCurrency, Currency.Silver.ToString(), 30, "Gather 30 silver"),
                    },
                    new QuestReward(Currency.Emerald, 1, "Parfa grins. \"Knew you had it in you.\"")),

                new QuestDefinition(
                    "willow_pelts", "Pelts for the Tanner",
                    "Willow's tanner friend will pay well for fresh hides.",
                    GuideNpcId.Willow.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.CollectItem, "hide", 3, "Gather 3 hides"),
                    },
                    new QuestReward(Currency.Ruby, 1, "Willow hands over the coin. \"Good work.\"", sigils: 160)),

                // --- onboarding: one quest per basic procedure, chained into two short lines ---
                new QuestDefinition(
                    "willow_first_cast", "First Channeling",
                    "Before anything else, Willow wants to see you channel your element.",
                    GuideNpcId.Willow.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.CastAbility, "", 1, "Channel your element once"),
                    },
                    new QuestReward(Currency.Silver, 50, "Willow nods. \"It flows in you.\"")),

                new QuestDefinition(
                    "willow_first_summon", "Answer the Beacon",
                    "Now that you can channel, Willow points you to the Summon Beacon. \"Spend some Sigils — see who answers.\"",
                    GuideNpcId.Willow.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.SummonCreature, "", 1, "Summon a creature at the Beacon (press U)"),
                    },
                    new QuestReward(Currency.Silver, 40, "Willow grins. \"The wilds answer the bold.\"", sigils: 80),
                    new List<string> { "willow_first_cast" }),

                new QuestDefinition(
                    "willow_claim_featured", "Claim the Featured",
                    "Willow explains the Beacon's promise: duplicates become Motes, and enough Motes claim the featured creature outright. \"Spend them on the spark.\"",
                    GuideNpcId.Willow.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.ClaimFeatured, "", 1, "Claim a featured creature with Motes"),
                    },
                    new QuestReward(Currency.Silver, 50, "Willow whistles. \"Knew it'd answer you.\"", sigils: 100),
                    new List<string> { "willow_first_summon" }),

                new QuestDefinition(
                    "parfa_first_craft", "First Craft",
                    "Parfa swears by making your own kit. \"Turn that loot into something useful.\"",
                    GuideNpcId.Parfa.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.CraftItem, "", 1, "Craft any item (press B)"),
                    },
                    new QuestReward(Currency.Silver, 60, "Parfa claps you on the back. \"A maker now.\"")),

                new QuestDefinition(
                    "parfa_gear_up", "Gear Up",
                    "With something crafted, Parfa won't send you out soft. \"Put it on before you go.\"",
                    GuideNpcId.Parfa.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.EquipItem, "", 1, "Equip a piece of gear (press V)"),
                    },
                    new QuestReward(Currency.Silver, 60, "Parfa looks you over. \"Better.\""),
                    new List<string> { "parfa_first_craft" }),
            };
        }
    }
}
