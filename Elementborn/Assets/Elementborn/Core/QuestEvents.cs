using System;

namespace Elementborn.Core
{
    /// <summary>
    /// A tiny static bus the gameplay raises and the quest controller listens on, so quest tracking is decoupled
    /// from combat / taming / inventory. The success points in those systems each make a single Raise call; the
    /// controller forwards them into the <see cref="QuestLog"/>. Targets are passed as names
    /// (CreatureKind / Currency / NpcId .ToString()).
    /// </summary>
    public static class QuestEvents
    {
        public static event Action<string> CreatureDefeated;
        public static event Action<string> CreatureTamed;
        public static event Action<string> CreatureSighted;
        public static event Action<string, int> CurrencyGained;
        public static event Action<string, int> ItemCollected;
        public static event Action<string> TalkedToNpc;
        public static event Action<string> QuestCompleted;
        // Element + IntentType, both as .ToString(). Raised at each resolved channeler cast; the grimoire
        // uses it to reveal Attacks entries (and to glimpse the matching base bloodline).
        public static event Action<string, string> AbilityCast;

        public static void RaiseCreatureDefeated(string creatureKind) => CreatureDefeated?.Invoke(creatureKind);
        public static void RaiseCreatureTamed(string creatureKind) => CreatureTamed?.Invoke(creatureKind);
        public static void RaiseCreatureSighted(string creatureKind) => CreatureSighted?.Invoke(creatureKind);
        public static void RaiseCurrencyGained(string currency, int amount) => CurrencyGained?.Invoke(currency, amount);
        public static void RaiseItemCollected(string itemId, int amount) => ItemCollected?.Invoke(itemId, amount);
        public static void RaiseTalkedToNpc(string npcId) => TalkedToNpc?.Invoke(npcId);
        public static void RaiseQuestCompleted(string questId) => QuestCompleted?.Invoke(questId);
        public static void RaiseAbilityCast(string element, string intent) => AbilityCast?.Invoke(element, intent);
    }
}
