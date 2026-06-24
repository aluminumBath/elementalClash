namespace Elementborn.Core
{
    /// <summary>How an attacking element fares against a defender's affinity — drives both the damage multiplier
    /// and the on-hit weak/resist cue.</summary>
    public enum Effectiveness { Neutral, Strong, Weak }

    /// <summary>
    /// Elemental effectiveness: a damage multiplier for an attacking element against a defender's affinity. The
    /// four elements form a cycle — Fire ▶ Earth ▶ Air ▶ Water ▶ Fire — where each is strong against the next and
    /// weak against the one before it; the remaining pairing is neutral. UnityEngine-free and unit-tested.
    /// </summary>
    public static class ElementMatchup
    {
        public const float Strong = 1.5f;
        public const float Weak = 0.6f;
        public const float Neutral = 1f;

        /// <summary>Classify how an attacking element fares against a defender's affinity.</summary>
        public static Effectiveness Classify(Element attacker, Element defender)
        {
            if (Beats(attacker) == defender) return Effectiveness.Strong; // attacker counters the defender
            if (Beats(defender) == attacker) return Effectiveness.Weak;   // defender resists the attacker
            return Effectiveness.Neutral;
        }

        public static float Multiplier(Element attacker, Element defender)
        {
            switch (Classify(attacker, defender))
            {
                case Effectiveness.Strong: return Strong;
                case Effectiveness.Weak:   return Weak;
                default:                   return Neutral;
            }
        }

        private static Element Beats(Element e)
        {
            switch (e)
            {
                case Element.Fire: return Element.Earth;
                case Element.Earth: return Element.Air;
                case Element.Air: return Element.Water;
                default: return Element.Fire; // Water beats Fire
            }
        }
    }
}
