using UnityEngine;

namespace Elementborn.Core
{
    public enum EnemyKind { Grunt, Brute, Runner, Archer, Elementalist }

    /// <summary>Tuning for one enemy archetype.</summary>
    public readonly struct EnemyStats
    {
        public readonly float MaxHealth;
        public readonly float MoveSpeed;
        public readonly float Damage;
        public readonly float AttackRange;
        public readonly float AttackCooldown;
        public readonly int ScoreValue;
        public readonly bool IsRanged;

        public EnemyStats(float maxHealth, float moveSpeed, float damage, float attackRange,
            float attackCooldown, int scoreValue, bool isRanged)
        {
            MaxHealth = maxHealth; MoveSpeed = moveSpeed; Damage = damage; AttackRange = attackRange;
            AttackCooldown = attackCooldown; ScoreValue = scoreValue; IsRanged = isRanged;
        }
    }

    /// <summary>Stat block per enemy kind. Tweak here to rebalance.</summary>
    public static class EnemyArchetypes
    {
        public static EnemyStats For(EnemyKind kind)
        {
            var b = Base(kind);
            // Global difficulty dials, applied once at the single source every spawn reads (1.0 = unchanged).
            return new EnemyStats(
                Balance.ScaledEnemyHealth(b.MaxHealth), b.MoveSpeed, Balance.ScaledEnemyDamage(b.Damage),
                b.AttackRange, b.AttackCooldown, b.ScoreValue, b.IsRanged);
        }

        private static EnemyStats Base(EnemyKind kind) => kind switch
        {
            //                                       hp     spd    dmg   range   cd   score  ranged
            EnemyKind.Grunt        => new EnemyStats(30f,  3.0f,   8f,   2.0f,  1.2f, 100,  false),
            EnemyKind.Brute        => new EnemyStats(85f,  2.0f,  18f,   2.4f,  1.8f, 250,  false),
            EnemyKind.Runner       => new EnemyStats(20f,  5.5f,   6f,   1.8f,  0.9f, 120,  false),
            EnemyKind.Archer       => new EnemyStats(25f,  2.6f,  10f,  14.0f,  2.0f, 180,  true),
            EnemyKind.Elementalist => new EnemyStats(45f,  2.8f,  13f,  12.0f,  2.2f, 320,  true),
            _                      => new EnemyStats(30f,  3.0f,   8f,   2.0f,  1.2f, 100,  false)
        };
    }

    /// <summary>Chooses an enemy archetype for a region by its danger and biome. Deterministic per RNG.</summary>
    public static class EnemySelector
    {
        public static EnemyKind Pick(int dangerLevel, BiomeType biome, IRandomSource rng)
        {
            float grunt = 5f, runner = 3f, brute = 0f, archer = 1f, elem = 0f;

            if (dangerLevel >= 2) { brute += 1.5f; archer += 1.5f; }
            if (dangerLevel >= 3) { brute += 1.5f; elem += 1.0f; }
            if (dangerLevel >= 4) { elem += 2.0f; grunt -= 1.0f; }
            if (dangerLevel >= 5) { elem += 2.0f; brute += 1.0f; }

            switch (biome)
            {
                case BiomeType.Desert:      runner += 2.0f; archer += 1.5f; break;
                case BiomeType.Volcano:     elem += 2.5f; break;
                case BiomeType.CloudTemple: archer += 2.0f; elem += 1.0f; break;
                case BiomeType.Mountains:   brute += 2.0f; break;
                case BiomeType.Swamp:
                case BiomeType.Marsh:       brute += 1.0f; break;
                default: break;
            }

            grunt = Mathf.Max(0f, grunt);
            float total = grunt + runner + brute + archer + elem;
            if (total <= 0f) return EnemyKind.Grunt;

            float roll = (float)rng.NextUnit() * total;
            if ((roll -= grunt) < 0f) return EnemyKind.Grunt;
            if ((roll -= runner) < 0f) return EnemyKind.Runner;
            if ((roll -= brute) < 0f) return EnemyKind.Brute;
            if ((roll -= archer) < 0f) return EnemyKind.Archer;
            return EnemyKind.Elementalist;
        }
    }

    /// <summary>One enemy's allegiance + role, rolled for a region. Wild enemies always carry an element;
    /// weapon bandits never do.</summary>
    public readonly struct EnemyPlan
    {
        public readonly Faction Faction;
        public readonly Element? Element;
        public readonly EnemyKind Kind;

        public EnemyPlan(Faction faction, Element? element, EnemyKind kind)
        {
            Faction = faction;
            Element = element;
            Kind = kind;
        }
    }

    /// <summary>Rolls a full enemy plan (faction + element + kind) for a region. Deterministic per RNG.</summary>
    public static class EnemyComposition
    {
        /// <summary>The element a biome leans toward, or null for mixed biomes.</summary>
        public static Element? BiomeElement(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Volcano: return Element.Fire;
                case BiomeType.Beach:
                case BiomeType.Island:
                case BiomeType.CoralReefForest:
                case BiomeType.Swamp:
                case BiomeType.Marsh: return Element.Water;
                case BiomeType.Desert:
                case BiomeType.Mountains: return Element.Earth;
                case BiomeType.CloudTemple: return Element.Air;
                default: return null;
            }
        }

        public static EnemyPlan Pick(BiomeType biome, int dangerLevel, IRandomSource rng)
        {
            Element? affinity = BiomeElement(biome);

            // Chance this fighter is an element channeler (Wild) rather than a weapon bandit.
            float elementalChance = 0.55f + 0.06f * dangerLevel + (affinity.HasValue ? 0.10f : 0f);
            if (elementalChance > 0.95f) elementalChance = 0.95f;

            Faction faction = Faction.Bandit;
            Element? element = null;
            if (rng.NextUnit() < elementalChance)
            {
                faction = Faction.Wild;
                element = (affinity.HasValue && rng.NextUnit() < 0.75)
                    ? affinity
                    : (Element)(int)(rng.NextUnit() * 4); // 0..3 -> Fire/Water/Earth/Air
            }

            EnemyKind kind = EnemySelector.Pick(dangerLevel, biome, rng);
            if (faction == Faction.Bandit && kind == EnemyKind.Elementalist)
                kind = EnemyKind.Brute; // bandits aren't elementalists

            return new EnemyPlan(faction, element, kind);
        }
    }
}
