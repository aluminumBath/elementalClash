using System;

namespace Elementborn.Core
{
    /// <summary>
    /// Pure character progression: experience, level, and the stat bonus a level confers. XP needed to reach the
    /// next level scales with the current level; gaining a lot of XP at once can cross several levels.
    /// <see cref="BonusMaxHealth"/> is what other systems read to buff the player. No Unity types — unit-tested.
    /// </summary>
    public sealed class Progression
    {
        public const int HealthPerLevel = 10;

        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }   // experience within the current level

        public event Action Changed;
        public event Action<int> LeveledUp;   // number of levels gained

        /// <summary>XP required to go from the current level to the next.</summary>
        public int XpToNext => 100 * Level;

        public float BonusMaxHealth => (Level - 1) * HealthPerLevel;

        /// <summary>Adds experience, rolling over as many levels as it covers. Returns the levels gained.</summary>
        public int AddXp(int amount)
        {
            if (amount <= 0) return 0;
            amount = Balance.ScaledXp(amount);   // global XP dial (1.0 = unchanged)
            Xp += amount;
            int gained = 0;
            while (Xp >= XpToNext)
            {
                Xp -= XpToNext;
                Level++;
                gained++;
            }
            Changed?.Invoke();
            if (gained > 0) LeveledUp?.Invoke(gained);
            return gained;
        }

        /// <summary>Restores level and within-level XP from a save (clamped to sane values).</summary>
        public void Restore(int level, int xp)
        {
            Level = level < 1 ? 1 : level;
            Xp = xp < 0 ? 0 : xp;
            Changed?.Invoke();
        }
    }
}
