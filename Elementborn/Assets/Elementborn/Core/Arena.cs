using System;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>What one arena wave is made of. Built by <see cref="ArenaProgression"/>.</summary>
    public readonly struct WavePlan
    {
        public readonly int EnemyCount;
        public readonly int DangerLevel; // feeds EnemyComposition/EnemySelector (1-5)

        public WavePlan(int enemyCount, int dangerLevel)
        {
            EnemyCount = enemyCount;
            DangerLevel = dangerLevel;
        }
    }

    /// <summary>
    /// Deterministic wave escalation for the dedicated combat mode: each wave adds enemies and ramps the
    /// danger level (which the existing <see cref="EnemyComposition"/>/<see cref="EnemySelector"/> use to pick
    /// tougher archetypes). Pure, so it unit-tests without a scene.
    /// </summary>
    public static class ArenaProgression
    {
        public const int TotalWaves = 8;

        public static WavePlan For(int wave)
        {
            int w = Mathf.Max(1, wave);
            int count = 3 + w;                       // wave 1 = 4 enemies, wave 8 = 11
            int danger = Mathf.Clamp(1 + w / 2, 1, 5);
            return new WavePlan(count, danger);
        }
    }

    /// <summary>Stamina cost per action. Defends are free; specials cost the most. Pure lookup.</summary>
    public static class StaminaCost
    {
        public static float For(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.PrimaryCast: return 12f;
                case IntentType.SecondaryCast: return 20f;
                case IntentType.Heavy: return 30f;
                case IntentType.Sweep: return 22f;
                case IntentType.Dash: return 12f;
                case IntentType.Defend: return 0f;
                default: return 0f;
            }
        }
    }

    /// <summary>
    /// A regenerating stamina pool that paces motion combat: actions spend it, it refills over time, and when
    /// it's empty you can't throw the heavy stuff (encouraging rhythm over flailing). Pure and unit-tested;
    /// a controller ticks it each frame and the combat path checks <see cref="TrySpend"/>.
    /// </summary>
    public sealed class StaminaModel
    {
        public float Max { get; }
        public float Current { get; private set; }
        public float RegenPerSecond { get; set; }

        public float Current01 => Max > 0f ? Mathf.Clamp01(Current / Max) : 0f;

        public StaminaModel(float max = 100f, float regenPerSecond = 22f)
        {
            Max = Mathf.Max(1f, max);
            RegenPerSecond = Mathf.Max(0f, regenPerSecond);
            Current = Max;
        }

        public bool CanAfford(float cost) => Current >= cost;

        /// <summary>Spend if affordable; returns false (and spends nothing) if too low. Zero-cost always succeeds.</summary>
        public bool TrySpend(float cost)
        {
            if (cost <= 0f) return true;
            if (Current < cost) return false;
            Current -= cost;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            Current = Mathf.Min(Max, Current + RegenPerSecond * deltaTime);
        }

        public void Refill() => Current = Max;
    }
}
