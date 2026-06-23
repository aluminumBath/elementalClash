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
                    new QuestReward(Currency.Silver, 50, "Willow nods. \"You'll do.\"")),

                new QuestDefinition(
                    "kiana_companion", "A Gentle Hand",
                    "Kiana thinks you're ready to win a creature over rather than just fight it.",
                    GuideNpcId.Kiana.ToString(),
                    new List<QuestObjective>
                    {
                        new QuestObjective(ObjectiveKind.TameCreature, "", 1, "Tame any creature"),
                    },
                    new QuestReward(Currency.Ruby, 1, "Kiana beams. \"A friend for life.\"")),

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
            };
        }
    }
}
