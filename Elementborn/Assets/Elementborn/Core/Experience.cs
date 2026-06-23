using System;

namespace Elementborn.Core
{
    /// <summary>How much experience things are worth. Creature XP scales with how tough the creature is, so a
    /// dragon is worth far more than a barn cat. Pure — unit-tested.</summary>
    public static class Experience
    {
        public const int MinCreatureXp = 5;

        public static int ForCreature(float maxHealth, float damage)
        {
            int xp = (int)Math.Round(maxHealth * 0.15 + damage * 1.0);
            return xp < MinCreatureXp ? MinCreatureXp : xp;
        }
    }
}
