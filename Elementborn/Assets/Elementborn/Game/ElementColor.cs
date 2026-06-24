using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Shared cel-friendly tint per element, so a Fire hit always reads orange, Water blue, Earth green, and Air
    /// pale-cyan across the feel layer (impact sparks, floating damage numbers, …).
    /// </summary>
    public static class ElementColor
    {
        public static Color For(Element element)
        {
            switch (element)
            {
                case Element.Fire: return new Color(1f, 0.55f, 0.2f);
                case Element.Water: return new Color(0.3f, 0.6f, 1f);
                case Element.Earth: return new Color(0.55f, 0.8f, 0.35f);
                case Element.Air: return new Color(0.7f, 0.95f, 1f);
                default: return Color.white;
            }
        }
    }
}
