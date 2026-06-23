using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>The defs parsed out of one mod file. Grows as more content types become moddable.</summary>
    public sealed class ParsedMod
    {
        public readonly List<FactionDef> Factions = new List<FactionDef>();
        public readonly List<EnemyDef> Enemies = new List<EnemyDef>();
        public bool IsEmpty => Factions.Count == 0 && Enemies.Count == 0;
    }

    // JSON shape (UnityEngine.JsonUtility needs an object at the root with array fields, and plain public
    // fields). Enum-like values are written as strings here and parsed leniently below, so mod files stay
    // human-friendly.
    [Serializable]
    public sealed class ModFactionDto
    {
        public string id;
        public string name;
        public string creed;
        public string strength;
        public string weakness;
        public float offenseMultiplier;
        public float defenseMultiplier;
        public string onConfluence;
        public string onMixedGifts;
    }

    [Serializable]
    public sealed class ModEnemyDto
    {
        public string id;
        public string name;
        public string element;
        public string behavior; // which built-in behaviour to mimic: Grunt/Brute/Runner/Archer/Elementalist
        public float maxHealth;
        public float moveSpeed;
        public float damage;
        public float attackRange;
        public float attackCooldown;
        public int scoreValue;
        public bool ranged;
    }

    [Serializable]
    public sealed class ModPackageDto
    {
        public ModFactionDto[] factions;
        public ModEnemyDto[] enemies;
    }

    /// <summary>
    /// Parses a mod file (JSON text) into strongly-typed content defs. Pure and defensive: bad or missing JSON
    /// yields an empty result rather than throwing, missing multipliers default to 1, and unknown doctrine
    /// strings fall back to "Accepts". <c>ModLoader</c> feeds the results into the registries.
    /// </summary>
    public static class ModContent
    {
        public static ParsedMod Parse(string json)
        {
            var result = new ParsedMod();
            if (string.IsNullOrWhiteSpace(json)) return result;

            ModPackageDto dto;
            try { dto = JsonUtility.FromJson<ModPackageDto>(json); }
            catch { return result; }
            if (dto == null) return result;

            if (dto.factions != null)
            {
                foreach (var f in dto.factions)
                {
                    if (f == null || string.IsNullOrEmpty(f.id)) continue;
                    result.Factions.Add(new FactionDef(
                        f.id, f.name, f.creed, f.strength, f.weakness,
                        f.offenseMultiplier > 0f ? f.offenseMultiplier : 1f,
                        f.defenseMultiplier > 0f ? f.defenseMultiplier : 1f,
                        ParseDoctrine(f.onConfluence),
                        ParseDoctrine(f.onMixedGifts)));
                }
            }

            if (dto.enemies != null)
            {
                foreach (var e in dto.enemies)
                {
                    if (e == null || string.IsNullOrEmpty(e.id)) continue;
                    var stats = new EnemyStats(
                        e.maxHealth > 0f ? e.maxHealth : 30f,
                        e.moveSpeed > 0f ? e.moveSpeed : 3f,
                        e.damage > 0f ? e.damage : 8f,
                        e.attackRange > 0f ? e.attackRange : 2f,
                        e.attackCooldown > 0f ? e.attackCooldown : 1.2f,
                        e.scoreValue > 0 ? e.scoreValue : 100,
                        e.ranged);
                    result.Enemies.Add(new EnemyDef(e.id, e.name, stats, ParseElement(e.element), ParseKind(e.behavior)));
                }
            }

            return result;
        }

        private static Doctrine ParseDoctrine(string s) =>
            Enum.TryParse(s, true, out Doctrine d) ? d : Doctrine.Accepts;

        private static EnemyKind ParseKind(string s) =>
            Enum.TryParse(s, true, out EnemyKind k) ? k : EnemyKind.Grunt;

        private static Element? ParseElement(string s) =>
            Enum.TryParse(s, true, out Element e) ? e : (Element?)null;
    }
}
