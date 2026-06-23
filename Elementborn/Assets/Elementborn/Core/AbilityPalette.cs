using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// The shared colour language for ability VFX. The element colour leads; a move's sub-art (variant) supplies
    /// the secondary / backup tint, and plain moves fall back to a lighter shade of the element. Lives in Core
    /// (colour data, like the other lookups here that already use UnityEngine types) so it can be unit-tested and
    /// shared by every presentation shell — Sweep fan, Heavy ring, impact bursts.
    /// </summary>
    public static class AbilityPalette
    {
        /// <summary>The base colour of an element — the primary tone of any cast.</summary>
        public static Color Primary(Element element)
        {
            switch (element)
            {
                case Element.Fire:  return new Color(1f, 0.55f, 0.15f);
                case Element.Water: return new Color(0.2f, 0.5f, 1f);
                case Element.Earth: return new Color(0.6f, 0.45f, 0.25f);
                case Element.Air:   return new Color(0.85f, 0.95f, 0.9f);
                default:            return Color.white;
            }
        }

        /// <summary>
        /// The secondary / backup colour layered on top of <see cref="Primary"/>: a sub-art's own tint when the
        /// variant is special, otherwise a lighter shade of the element.
        /// </summary>
        public static Color Secondary(AbilityVariant variant, Element element)
        {
            switch (variant)
            {
                case AbilityVariant.Magmacraft:   return new Color(1f, 0.35f, 0.05f);   // molten deep-orange
                case AbilityVariant.Lightning:    return new Color(0.95f, 0.95f, 0.5f); // electric yellow
                case AbilityVariant.Ice:          return new Color(0.6f, 0.9f, 1f);     // pale cyan
                case AbilityVariant.Oreshaping:   return new Color(0.7f, 0.72f, 0.78f); // steel
                case AbilityVariant.SanguineGrip: return new Color(0.7f, 0.1f, 0.15f);  // blood
                case AbilityVariant.Flight:       return new Color(0.9f, 1f, 1f);       // pale sky
            }
            return Lighten(Primary(element), 0.35f);
        }

        private static Color Lighten(Color c, float t) =>
            new Color(Mathf.Lerp(c.r, 1f, t), Mathf.Lerp(c.g, 1f, t), Mathf.Lerp(c.b, 1f, t), c.a);
    }
}
