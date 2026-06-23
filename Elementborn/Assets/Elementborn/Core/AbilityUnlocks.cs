using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// The ability ladder: which player level unlocks each combat <see cref="IntentType"/>. Pure lookup, the
    /// sibling of <see cref="StaminaCost"/>. The combat controller checks this before firing an intent; a locked
    /// intent no-ops. Because both Channelers and weapon users dispatch through the same intents, one table gates
    /// both kits on the same levels. All the pacing lives here, so tuning the ladder is a one-line change.
    ///
    /// Default ladder: Primary + Defend from the start, Sweep at 2, Heavy at 4, the signature casts at 6.
    /// Anything not on the ladder (Primary, Defend, Dash, None) is always available — it returns level 1.
    /// </summary>
    public static class AbilityUnlocks
    {
        // The level-gated moveset, in unlock order. Everything else is available from the start.
        private static readonly (IntentType Intent, int Level)[] Ladder =
        {
            (IntentType.Sweep, 2),
            (IntentType.Heavy, 4),
            (IntentType.SecondaryCast, 6), // the charged sub-art (e.g. Fire -> Lightning)
            (IntentType.Signature, 6),     // the hidden, element-specific signature move
        };

        /// <summary>The level at which an intent becomes usable. Ungated intents return 1 (always available).</summary>
        public static int RequiredLevel(IntentType intent)
        {
            foreach (var (i, level) in Ladder)
                if (i == intent) return level;
            return 1;
        }

        /// <summary>True if a character of <paramref name="level"/> may use the intent.</summary>
        public static bool IsUnlocked(IntentType intent, int level) => level >= RequiredLevel(intent);

        /// <summary>
        /// The gated intents that become available crossing from <paramref name="previousLevel"/> up to
        /// <paramref name="newLevel"/> (exclusive lower bound, inclusive upper) — for the level-up announcement.
        /// Handles multi-level jumps. Never includes the always-on starting kit.
        /// </summary>
        public static List<IntentType> NewlyUnlocked(int previousLevel, int newLevel)
        {
            var list = new List<IntentType>();
            foreach (var (intent, level) in Ladder)
                if (level > previousLevel && level <= newLevel)
                    list.Add(intent);
            return list;
        }

        /// <summary>A short, player-facing name for an intent (for toasts).</summary>
        public static string DisplayName(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.PrimaryCast: return "Primary";
                case IntentType.SecondaryCast: return "Secondary";
                case IntentType.Defend: return "Defend";
                case IntentType.Dash: return "Dash";
                case IntentType.Heavy: return "Heavy";
                case IntentType.Sweep: return "Sweep";
                case IntentType.Signature: return "Signature";
                default: return "Action";
            }
        }
    }
}
