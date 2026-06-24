using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>Turns the elements enchanted into worn armor into an incoming-damage multiplier. Each enchanted
    /// piece resists the element it dominates and is vulnerable to the element that dominates it (same cycle as
    /// <see cref="ElementMatchup"/>); the effects of every piece add up, so four pieces of one element make a
    /// strong specialist with a matching glaring weakness, while a mixed set evens out.</summary>
    public static class ArmorResist
    {
        public const float ResistStep = 0.15f; // per piece: damage cut vs an element this piece beats
        public const float WeakStep   = 0.15f; // per piece: damage added vs the element that beats this piece
        public const float Floor      = 0.10f; // incoming damage is never scaled below this fraction

        /// <summary>Multiplier applied to an incoming hit of <paramref name="incoming"/>, given the elements the
        /// wearer's armor is enchanted with. 1 = neutral.</summary>
        public static float IncomingMultiplier(IEnumerable<Element> enchants, Element incoming)
        {
            if (enchants == null) return 1f;
            float sum = 0f;
            foreach (var armor in enchants)
            {
                Effectiveness cls = ElementMatchup.Classify(incoming, armor); // incoming attacks the armor's element
                if (cls == Effectiveness.Strong) sum += WeakStep;        // incoming beats this piece -> more damage
                else if (cls == Effectiveness.Weak) sum -= ResistStep;   // this piece beats incoming -> less damage
            }
            float m = 1f + sum;
            return m < Floor ? Floor : m;
        }
    }
}
