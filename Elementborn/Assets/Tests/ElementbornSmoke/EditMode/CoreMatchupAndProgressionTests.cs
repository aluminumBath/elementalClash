using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Elementborn.Tests.EditMode
{
    /// <summary>
    /// Unit coverage for the pure Core progression + elemental-effectiveness math that ships in the runtime
    /// (Assembly-CSharp). This test assembly cannot reference Assembly-CSharp at compile time, so — exactly like
    /// <see cref="ElementbornEditModeSmokeTests"/> — every Core type is reached by name through reflection. That
    /// keeps the tests dependency-free while still exercising the real shipped code, not a copy.
    ///
    /// Covered: ElementMatchup (the effectiveness cycle + multipliers), AbilityUnlocks (the level ladder),
    /// Progression (XP/level/health rollover), Perks (ranking + aggregated effects), and Balance sanity helpers.
    /// </summary>
    public sealed class CoreMatchupAndProgressionTests
    {
        // ------------------------------------------------------------------ reflection plumbing
        private static Type T(string fullName)
        {
            Type t = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName, false))
                .FirstOrDefault(x => x != null);
            Assert.That(t, Is.Not.Null, "Core type not found: " + fullName);
            return t;
        }

        private static object Ev(string enumFullName, string member) => Enum.Parse(T(enumFullName), member);

        private static object SCall(string typeFullName, string method, params object[] args)
        {
            MethodInfo m = T(typeFullName).GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            Assert.That(m, Is.Not.Null, "static method not found: " + typeFullName + "." + method);
            return m.Invoke(null, args);
        }

        private static object New(string typeFullName) => Activator.CreateInstance(T(typeFullName));

        private static object ICall(object instance, string method, params object[] args)
        {
            MethodInfo m = instance.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(m, Is.Not.Null, "instance method not found: " + method);
            return m.Invoke(instance, args);
        }

        private static object Prop(object instance, string name)
        {
            PropertyInfo p = instance.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(p, Is.Not.Null, "property not found: " + name);
            return p.GetValue(instance);
        }

        private const string Element = "Elementborn.Core.Element";
        private const string Intent = "Elementborn.Core.IntentType";
        private const string Perk = "Elementborn.Core.PerkId";

        private static float Multiplier(string attacker, string defender) =>
            (float)SCall("Elementborn.Core.ElementMatchup", "Multiplier", Ev(Element, attacker), Ev(Element, defender));

        private static string Classify(string attacker, string defender) =>
            SCall("Elementborn.Core.ElementMatchup", "Classify", Ev(Element, attacker), Ev(Element, defender)).ToString();

        // ------------------------------------------------------------------ ElementMatchup
        [Test]
        public void Matchup_CycleWinner_IsStrong()
        {
            // Fire > Earth > Air > Water > Fire
            Assert.That(Multiplier("Fire", "Earth"), Is.EqualTo(1.5f).Within(1e-4f));
            Assert.That(Multiplier("Earth", "Air"), Is.EqualTo(1.5f).Within(1e-4f));
            Assert.That(Multiplier("Air", "Water"), Is.EqualTo(1.5f).Within(1e-4f));
            Assert.That(Multiplier("Water", "Fire"), Is.EqualTo(1.5f).Within(1e-4f));
        }

        [Test]
        public void Matchup_CycleLoser_IsWeak()
        {
            Assert.That(Multiplier("Earth", "Fire"), Is.EqualTo(0.6f).Within(1e-4f));
            Assert.That(Multiplier("Air", "Earth"), Is.EqualTo(0.6f).Within(1e-4f));
            Assert.That(Multiplier("Water", "Air"), Is.EqualTo(0.6f).Within(1e-4f));
            Assert.That(Multiplier("Fire", "Water"), Is.EqualTo(0.6f).Within(1e-4f));
        }

        [Test]
        public void Matchup_SelfAndOffCycle_IsNeutral()
        {
            foreach (string e in new[] { "Fire", "Water", "Earth", "Air" })
                Assert.That(Multiplier(e, e), Is.EqualTo(1f).Within(1e-4f), e + " vs itself should be neutral");

            // The remaining (non-adjacent) pairings on the 4-cycle are neutral both ways.
            Assert.That(Multiplier("Fire", "Air"), Is.EqualTo(1f).Within(1e-4f));
            Assert.That(Multiplier("Air", "Fire"), Is.EqualTo(1f).Within(1e-4f));
            Assert.That(Multiplier("Water", "Earth"), Is.EqualTo(1f).Within(1e-4f));
            Assert.That(Multiplier("Earth", "Water"), Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void Matchup_Classify_MatchesMultiplier()
        {
            Assert.That(Classify("Fire", "Earth"), Is.EqualTo("Strong"));
            Assert.That(Classify("Earth", "Fire"), Is.EqualTo("Weak"));
            Assert.That(Classify("Fire", "Air"), Is.EqualTo("Neutral"));
        }

        // ------------------------------------------------------------------ AbilityUnlocks ladder
        [Test]
        public void Ladder_GatedLevels_MatchDesign()
        {
            Assert.That((int)SCall("Elementborn.Core.AbilityUnlocks", "RequiredLevel", Ev(Intent, "Sweep")), Is.EqualTo(2));
            Assert.That((int)SCall("Elementborn.Core.AbilityUnlocks", "RequiredLevel", Ev(Intent, "Heavy")), Is.EqualTo(4));
            Assert.That((int)SCall("Elementborn.Core.AbilityUnlocks", "RequiredLevel", Ev(Intent, "SecondaryCast")), Is.EqualTo(6));
            Assert.That((int)SCall("Elementborn.Core.AbilityUnlocks", "RequiredLevel", Ev(Intent, "Signature")), Is.EqualTo(6));
        }

        [Test]
        public void Ladder_StartingKit_IsAlwaysAvailable()
        {
            foreach (string i in new[] { "PrimaryCast", "Defend", "Dash", "None" })
                Assert.That((int)SCall("Elementborn.Core.AbilityUnlocks", "RequiredLevel", Ev(Intent, i)), Is.EqualTo(1),
                    i + " should be ungated (level 1)");
        }

        [Test]
        public void Ladder_IsUnlocked_RespectsThreshold()
        {
            Assert.That((bool)SCall("Elementborn.Core.AbilityUnlocks", "IsUnlocked", Ev(Intent, "Sweep"), 1), Is.False);
            Assert.That((bool)SCall("Elementborn.Core.AbilityUnlocks", "IsUnlocked", Ev(Intent, "Sweep"), 2), Is.True);
            Assert.That((bool)SCall("Elementborn.Core.AbilityUnlocks", "IsUnlocked", Ev(Intent, "Heavy"), 3), Is.False);
            Assert.That((bool)SCall("Elementborn.Core.AbilityUnlocks", "IsUnlocked", Ev(Intent, "Heavy"), 4), Is.True);
        }

        [Test]
        public void Ladder_NewlyUnlocked_HandlesJumpsAndBounds()
        {
            Assert.That(NewlyUnlockedNames(1, 2), Is.EquivalentTo(new[] { "Sweep" }));
            Assert.That(NewlyUnlockedNames(3, 6), Is.EquivalentTo(new[] { "Heavy", "SecondaryCast", "Signature" }));
            Assert.That(NewlyUnlockedNames(1, 6), Is.EquivalentTo(new[] { "Sweep", "Heavy", "SecondaryCast", "Signature" }));
            Assert.That(NewlyUnlockedNames(6, 6), Is.Empty);
            Assert.That(NewlyUnlockedNames(2, 2), Is.Empty);
        }

        private static string[] NewlyUnlockedNames(int prev, int next)
        {
            var result = (IEnumerable)SCall("Elementborn.Core.AbilityUnlocks", "NewlyUnlocked", prev, next);
            return result.Cast<object>().Select(o => o.ToString()).ToArray();
        }

        // ------------------------------------------------------------------ Progression
        [Test]
        public void Progression_Fresh_StartsAtLevelOne()
        {
            object p = New("Elementborn.Core.Progression");
            Assert.That((int)Prop(p, "Level"), Is.EqualTo(1));
            Assert.That((int)Prop(p, "XpToNext"), Is.EqualTo(100));
            Assert.That((float)Prop(p, "BonusMaxHealth"), Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void Progression_AddXp_LevelsUpAndScalesHealth()
        {
            object p = New("Elementborn.Core.Progression");
            int gained = (int)ICall(p, "AddXp", 100);
            Assert.That(gained, Is.EqualTo(1));
            Assert.That((int)Prop(p, "Level"), Is.EqualTo(2));
            Assert.That((int)Prop(p, "XpToNext"), Is.EqualTo(200));
            Assert.That((float)Prop(p, "BonusMaxHealth"), Is.EqualTo(10f).Within(1e-4f));
        }

        [Test]
        public void Progression_AddXp_RollsOverMultipleLevels()
        {
            object p = New("Elementborn.Core.Progression");
            int gained = (int)ICall(p, "AddXp", 300); // 100 -> L2, 200 -> L3, remainder 0
            Assert.That(gained, Is.EqualTo(2));
            Assert.That((int)Prop(p, "Level"), Is.EqualTo(3));
            Assert.That((float)Prop(p, "BonusMaxHealth"), Is.EqualTo(20f).Within(1e-4f));
        }

        [Test]
        public void Progression_NonPositiveXp_IsNoOp_AndRestoreClamps()
        {
            object p = New("Elementborn.Core.Progression");
            Assert.That((int)ICall(p, "AddXp", 0), Is.EqualTo(0));
            Assert.That((int)ICall(p, "AddXp", -50), Is.EqualTo(0));
            ICall(p, "Restore", -3, -10);
            Assert.That((int)Prop(p, "Level"), Is.EqualTo(1));
            Assert.That((int)Prop(p, "Xp"), Is.EqualTo(0));
            ICall(p, "Restore", 5, 40);
            Assert.That((int)Prop(p, "Level"), Is.EqualTo(5));
            Assert.That((int)Prop(p, "Xp"), Is.EqualTo(40));
        }

        // ------------------------------------------------------------------ Perks
        [Test]
        public void Perks_CannotRankWithoutPoints()
        {
            object s = New("Elementborn.Core.PerkState");
            Assert.That((int)Prop(s, "AvailablePoints"), Is.EqualTo(0));
            Assert.That((bool)ICall(s, "CanRank", Ev(Perk, "Toughness")), Is.False);
            Assert.That((bool)ICall(s, "Rank", Ev(Perk, "Toughness")), Is.False);
        }

        [Test]
        public void Perks_GrantThenRank_AppliesAggregatedEffects()
        {
            object s = New("Elementborn.Core.PerkState");
            ICall(s, "Grant", 4);
            Assert.That((bool)ICall(s, "Rank", Ev(Perk, "Toughness")), Is.True);
            Assert.That((bool)ICall(s, "Rank", Ev(Perk, "Toughness")), Is.True);
            Assert.That((bool)ICall(s, "Rank", Ev(Perk, "Scholar")), Is.True);
            Assert.That((bool)ICall(s, "Rank", Ev(Perk, "Fortune")), Is.True);
            Assert.That((int)ICall(s, "RankOf", Ev(Perk, "Toughness")), Is.EqualTo(2));
            Assert.That((int)Prop(s, "AvailablePoints"), Is.EqualTo(0));
            Assert.That((float)Prop(s, "BonusMaxHealth"), Is.EqualTo(40f).Within(1e-4f)); // 2 * 20
            Assert.That((float)Prop(s, "XpMultiplier"), Is.EqualTo(1.10f).Within(1e-4f));  // 1 * 10%
            Assert.That((float)Prop(s, "RewardMultiplier"), Is.EqualTo(1.15f).Within(1e-4f)); // 1 * 15%
        }

        [Test]
        public void Perks_RankIsCappedAtMax()
        {
            object s = New("Elementborn.Core.PerkState");
            ICall(s, "Grant", 10);
            for (int i = 0; i < 5; i++)
                Assert.That((bool)ICall(s, "Rank", Ev(Perk, "Scholar")), Is.True, "rank " + i + " should succeed");
            Assert.That((bool)ICall(s, "CanRank", Ev(Perk, "Scholar")), Is.False, "MaxRank (5) reached");
            Assert.That((bool)ICall(s, "Rank", Ev(Perk, "Scholar")), Is.False);
            Assert.That((int)ICall(s, "RankOf", Ev(Perk, "Scholar")), Is.EqualTo(5));
            Assert.That((int)Prop(s, "AvailablePoints"), Is.EqualTo(5)); // 10 granted - 5 spent
        }

        [Test]
        public void PerkCatalog_HasThreePerks_WithExpectedCaps()
        {
            PropertyInfo allProp = T("Elementborn.Core.PerkCatalog")
                .GetProperty("All", BindingFlags.Public | BindingFlags.Static);
            Assert.That(allProp, Is.Not.Null, "PerkCatalog.All not found");
            var all = (IEnumerable)allProp.GetValue(null);
            Assert.That(all.Cast<object>().Count(), Is.EqualTo(3));

            object def = SCall("Elementborn.Core.PerkCatalog", "Get", Ev(Perk, "Toughness"));
            Assert.That(def, Is.Not.Null);
            Assert.That(Prop(def, "Name").ToString(), Is.EqualTo("Toughness"));
            Assert.That((int)Prop(def, "MaxRank"), Is.EqualTo(5));
        }

        // ------------------------------------------------------------------ Balance sanity
        [Test]
        public void Balance_DefaultXpDial_IsIdentity_AndScaleGuards()
        {
            Assert.That((int)SCall("Elementborn.Core.Balance", "ScaledXp", 100), Is.EqualTo(100));
            Assert.That((bool)SCall("Elementborn.Core.Balance", "ScaleIsSane", 1f), Is.True);
            Assert.That((bool)SCall("Elementborn.Core.Balance", "ScaleIsSane", 0f), Is.False);
            Assert.That((bool)SCall("Elementborn.Core.Balance", "ScaleIsSane", 999f), Is.False);
        }
    }
}
