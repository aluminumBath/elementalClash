using System;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A tiny global channel that fires whenever any <see cref="Damageable"/> is hit or defeated, carrying the
    /// world position so presentation systems — camera shake, hit-stop, impact lights — can react without holding a
    /// reference to the combatants. Raised by <see cref="Damageable"/>; consumed by the Feel layer.
    /// </summary>
    public static class CombatFeedback
    {
        /// <summary>(world position, damage amount after reductions, damage element).</summary>
        public static event Action<Vector3, float, Element> Hit;

        /// <summary>(death position, last damage element).</summary>
        public static event Action<Vector3, Element> Defeated;

        public static void RaiseHit(Vector3 position, float amount, Element element) => Hit?.Invoke(position, amount, element);
        public static void RaiseDefeated(Vector3 position, Element element) => Defeated?.Invoke(position, element);
    }
}
