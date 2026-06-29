using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    /// <summary>
    /// The QA backbone for #7: invariants that lock Elementborn's balance against careless tuning edits. These run
    /// over the <em>live</em> Core content (the actual gacha config, ability constants, XP curve, item catalog),
    /// so if a number drifts out of a sane range — a zero-damage attack, a legendary rate that creeps up, a
    /// negative price — a test goes red instead of the mistake shipping. Bounds come from <see cref="Balance"/>,
    /// which is the single place to retune the policy. Pure; no Unity types.
    /// </summary>
    public class BalanceSanityTests
    {
        // ---- Global difficulty dials ------------------------------------------------------------------------

        [Test]
        public void DifficultyDials_AreAllPositiveAndSane()
        {
            Assert.IsTrue(Balance.ScaleIsSane(Balance.EnemyHealthScale),  "EnemyHealthScale");
            Assert.IsTrue(Balance.ScaleIsSane(Balance.EnemyDamageScale),  "EnemyDamageScale");
            Assert.IsTrue(Balance.ScaleIsSane(Balance.PlayerDamageScale), "PlayerDamageScale");
            Assert.IsTrue(Balance.ScaleIsSane(Balance.RewardScale),       "RewardScale");
            Assert.IsTrue(Balance.ScaleIsSane(Balance.DropRateScale),     "DropRateScale");
            Assert.IsTrue(Balance.ScaleIsSane(Balance.XpScale),           "XpScale");
            Assert.IsTrue(Balance.ScaleIsSane(Balance.GachaRateScale),    "GachaRateScale");
        }

        [Test]
        public void SanityBounds_AreInternallyConsistent()
        {
            Assert.Less(Balance.MinAbilityBaseDamage, Balance.MaxAbilityBaseDamage);
            Assert.Greater(Balance.MaxDamageMultiplier, 1f);
            Assert.That(Balance.MaxLegendaryRate, Is.GreaterThan(0.0).And.LessThan(Balance.MaxEpicRate));
            Assert.Greater(Balance.MaxSingleSummonCost, 0);
            Assert.Greater(Balance.MaxScale, 1f);
        }

        // ---- Combat: ability tuning -------------------------------------------------------------------------

        [Test]
        public void AbilityBaseDamage_IsAlwaysPositiveAndWithinBounds()
        {
            float[] baseDamage =
            {
                AbilitySystem.FireBaseDamage, AbilitySystem.WaterBaseDamage, AbilitySystem.EarthBaseDamage,
                AbilitySystem.AirBaseDamage, AbilitySystem.LightningBaseDamage, AbilitySystem.IceBaseDamage,
                AbilitySystem.BloodDamage,
            };
            foreach (var d in baseDamage)
                Assert.That(d, Is.GreaterThanOrEqualTo(Balance.MinAbilityBaseDamage)
                                 .And.LessThanOrEqualTo(Balance.MaxAbilityBaseDamage),
                            "base damage out of range: " + d);
        }

        [Test]
        public void AbilityChargeBonuses_AreNonNegative()
        {
            float[] chargeBonus =
            {
                AbilitySystem.FireChargeBonus, AbilitySystem.WaterChargeBonus, AbilitySystem.EarthChargeBonus,
                AbilitySystem.AirChargeBonus, AbilitySystem.LightningChargeBonus, AbilitySystem.IceChargeBonus,
            };
            foreach (var b in chargeBonus)
                Assert.GreaterOrEqual(b, 0f, "charge bonus should never reduce damage");
        }

        [Test]
        public void DamageMultipliers_AreBonusesNotPenalties()
        {
            float[] multipliers =
            {
                AbilitySystem.LavaDamageMultiplier, AbilitySystem.MetalDamageMultiplier,
                AbilitySystem.BoulderDamageMultiplier,
            };
            foreach (var m in multipliers)
                Assert.That(m, Is.GreaterThanOrEqualTo(1f).And.LessThanOrEqualTo(Balance.MaxDamageMultiplier),
                            "an upgrade multiplier must be >= 1 and not absurd: " + m);
        }

        [Test]
        public void ProjectileSpeeds_ArePositive()
        {
            float[] speeds =
            {
                AbilitySystem.FireProjectileSpeed, AbilitySystem.WaterProjectileSpeed,
                AbilitySystem.EarthProjectileSpeed, AbilitySystem.AirProjectileSpeed,
                AbilitySystem.LightningSpeed, AbilitySystem.IceSpeed,
            };
            foreach (var s in speeds)
                Assert.Greater(s, 0f, "a projectile must travel");
        }

        [Test]
        public void StatusEffectDurationsAndForces_ArePositive()
        {
            float[] durations =
            {
                AbilitySystem.LightningStunDuration, AbilitySystem.IceSlowDuration,
                AbilitySystem.BloodControlDuration,
            };
            foreach (var t in durations)
                Assert.Greater(t, 0f, "a status effect must last a non-zero time");

            float[] forces = { AbilitySystem.BoulderKnockback, AbilitySystem.AirKnockback, AbilitySystem.BloodFlingForce };
            foreach (var f in forces)
                Assert.Greater(f, 0f, "a knockback/fling must push");

            // A slow scales remaining speed, so it must stay above 0 and at most 1.
            Assert.That(AbilitySystem.IceSlowMagnitude, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f));
        }

        // ---- Summon Beacon gacha ----------------------------------------------------------------------------

        [Test]
        public void SummonBeacon_RatesAreRareAndOrdered()
        {
            var c = SummonConfig.Default;
            Assert.That(c.LegendaryChance, Is.GreaterThan(0.0).And.LessThan(1.0));
            Assert.That(c.EpicChance, Is.GreaterThan(0.0).And.LessThan(1.0));
            Assert.Less(c.LegendaryChance, c.EpicChance, "legendary must be rarer than epic");
            Assert.Less(c.LegendaryChance + c.EpicChance, 1.0, "the rare band needs the remaining probability");
            Assert.LessOrEqual(c.LegendaryChance, Balance.MaxLegendaryRate, "legendary rate drifted out of 'rare'");
            Assert.LessOrEqual(c.EpicChance, Balance.MaxEpicRate, "epic rate drifted too high");
            Assert.That(c.FeaturedChance, Is.GreaterThan(0.0).And.LessThanOrEqualTo(1.0));
        }

        [Test]
        public void SummonBeacon_CostsAndPityAreFair()
        {
            var c = SummonConfig.Default;
            Assert.Greater(c.SingleCost, 0);
            Assert.Greater(c.TenCost, 0);
            Assert.Greater(c.TenPullSize, 0);
            Assert.Greater(c.HardPity, 0);
            Assert.LessOrEqual(c.SingleCost, Balance.MaxSingleSummonCost);
            Assert.LessOrEqual(c.TenCost, c.SingleCost * c.TenPullSize, "a ten-pull must not cost more than ten singles");
            Assert.Greater(c.FeaturedExchangeCost, 0);
            Assert.AreEqual(c.SingleCost, c.CostFor(false));
            Assert.AreEqual(c.TenCost, c.CostFor(true));
        }

        [Test]
        public void SummonBeacon_RefundsRewardRarity()
        {
            var c = SummonConfig.Default;
            Assert.LessOrEqual(c.RefundRare, c.RefundEpic, "a rarer dupe should refund at least as much");
            Assert.LessOrEqual(c.RefundEpic, c.RefundLegendary);
            Assert.Greater(c.RefundRare, 0);
            Assert.AreEqual(c.RefundLegendary, c.RefundFor(SummonRarity.Legendary));
            Assert.AreEqual(c.RefundEpic, c.RefundFor(SummonRarity.Epic));
            Assert.AreEqual(c.RefundRare, c.RefundFor(SummonRarity.Rare));
        }

        // ---- Character-creation roll ------------------------------------------------------------------------

        [Test]
        public void CharacterRoll_RatesAreValidAndOrdered()
        {
            var g = GachaConfig.Default;
            Assert.That(g.ConfluenceChance, Is.GreaterThan(0.0).And.LessThan(1.0));
            Assert.That(g.SubArtChance, Is.GreaterThan(0.0).And.LessThan(1.0));
            Assert.Less(g.ConfluenceChance, g.SubArtChance, "the full Confluence must be rarer than a sub-art");
        }

        // ---- Progression curve ------------------------------------------------------------------------------

        [Test]
        public void XpCurve_IsStrictlyIncreasingAndPositive()
        {
            var p = new Progression();
            int previous = 0;
            for (int level = 1; level <= 50; level++)
            {
                p.Restore(level, 0);
                int needed = p.XpToNext;
                Assert.Greater(needed, 0, "every level must require positive XP");
                Assert.Greater(needed, previous, "the XP requirement must grow each level");
                previous = needed;
            }
        }

        // ---- Economy: item pricing --------------------------------------------------------------------------

        [Test]
        public void ItemCatalog_IsNonEmptyAndHasNoNegativePrices()
        {
            var all = ItemCatalog.All();
            Assert.IsNotNull(all);
            Assert.Greater(all.Count, 0, "the item catalog should not be empty");
            foreach (var item in all)
                Assert.GreaterOrEqual(item.Value, 0, "item '" + item.Id + "' has a negative price");
        }

        // ---- Difficulty-dial scaling helpers ----------------------------------------------------------------

        [Test]
        public void ScalingHelpers_AreIdentityAtDefaultDialsAndNeverNegative()
        {
            // Every dial defaults to 1.0, so the helpers pass values through unchanged.
            Assert.That(Balance.ScaledEnemyHealth(30f), Is.EqualTo(30f).Within(0.001));
            Assert.That(Balance.ScaledEnemyDamage(8f),  Is.EqualTo(8f).Within(0.001));
            Assert.That(Balance.ScaledPlayerDamage(15f), Is.EqualTo(15f).Within(0.001));
            Assert.AreEqual(100, Balance.ScaledReward(100));
            Assert.AreEqual(100, Balance.ScaledXp(100));
            Assert.AreEqual(2,   Balance.ScaledDropWeight(2));

            // Zero in stays zero; a positive whole-number reward never rounds away to nothing or goes negative.
            Assert.AreEqual(0, Balance.ScaledReward(0));
            Assert.AreEqual(0, Balance.ScaledXp(0));
            Assert.GreaterOrEqual(Balance.ScaledReward(1), 1);
            Assert.GreaterOrEqual(Balance.ScaledXp(1), 1);
            Assert.GreaterOrEqual(Balance.ScaledDropWeight(1), 1);
        }

        // ---- Enemy archetypes -------------------------------------------------------------------------------

        [Test]
        public void EnemyArchetypes_AreAllSane()
        {
            foreach (EnemyKind kind in System.Enum.GetValues(typeof(EnemyKind)))
            {
                var s = EnemyArchetypes.For(kind);
                Assert.Greater(s.MaxHealth, 0f, kind + " max health");
                Assert.Greater(s.MoveSpeed, 0f, kind + " move speed");
                Assert.GreaterOrEqual(s.Damage, 0f, kind + " damage");
                Assert.Greater(s.AttackRange, 0f, kind + " attack range");
                Assert.Greater(s.AttackCooldown, 0f, kind + " attack cooldown");
                Assert.GreaterOrEqual(s.ScoreValue, 0, kind + " score");
            }
        }

        // ---- Boss catalog -----------------------------------------------------------------------------------

        [Test]
        public void BossCatalog_AreAllSane()
        {
            foreach (SiteKind kind in System.Enum.GetValues(typeof(SiteKind)))
            {
                var b = BossCatalog.For(kind);
                Assert.Greater(b.HealthMultiplier, 0f, kind + " health multiplier");
                Assert.Greater(b.Scale, 0f, kind + " scale");
                Assert.GreaterOrEqual(b.SilverReward, 0, kind + " silver reward");
                Assert.GreaterOrEqual(b.GemReward, 0, kind + " gem reward");
                Assert.IsFalse(string.IsNullOrEmpty(b.Name), kind + " name");
            }
        }

        // ---- Loot tables ------------------------------------------------------------------------------------

        [Test]
        public void LootTables_AreWellFormedAndReferenceRealItems()
        {
            foreach (CreatureKind kind in System.Enum.GetValues(typeof(CreatureKind)))
            {
                var table = LootTables.For(kind);
                Assert.IsNotNull(table, kind + " loot table");
                Assert.GreaterOrEqual(table.Rolls, 1, kind + " rolls");
                Assert.IsNotNull(table.Entries);
                Assert.Greater(table.Entries.Count, 0, kind + " has no loot entries");

                int totalWeight = 0;
                foreach (var e in table.Entries)
                {
                    Assert.GreaterOrEqual(e.Weight, 0, kind + " negative weight");
                    Assert.GreaterOrEqual(e.MinCount, 0, kind + " negative min count");
                    Assert.LessOrEqual(e.MinCount, e.MaxCount, kind + " min count exceeds max");
                    Assert.IsFalse(string.IsNullOrEmpty(e.ItemId), kind + " empty item id");
                    Assert.IsTrue(ItemCatalog.Exists(e.ItemId), kind + " drops unknown item '" + e.ItemId + "'");
                    totalWeight += e.Weight;
                }
                Assert.Greater(totalWeight, 0, kind + " loot table can never drop anything");
            }
        }
    }
}
