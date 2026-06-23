using NUnit.Framework;
using System.Linq;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ModdingTests
    {
        private sealed class Dummy : IContentDef
        {
            public string Id { get; }
            public Dummy(string id) { Id = id; }
        }

        [Test]
        public void RegistryRegistersGetsAndOverrides()
        {
            var reg = new ContentRegistry<Dummy>();
            reg.Register(new Dummy("alpha"));
            reg.Register(new Dummy("beta"));
            Assert.AreEqual(2, reg.Count);
            Assert.IsTrue(reg.Contains("ALPHA")); // case-insensitive
            Assert.IsNotNull(reg.Get("beta"));

            var replacement = new Dummy("alpha");
            reg.Register(replacement);
            Assert.AreEqual(2, reg.Count);                 // same id -> no new entry
            Assert.AreSame(replacement, reg.Get("alpha")); // last write wins
        }

        [Test]
        public void RegistryIgnoresNullOrEmptyIds()
        {
            var reg = new ContentRegistry<Dummy>();
            reg.Register(null);
            reg.Register(new Dummy(""));
            Assert.AreEqual(0, reg.Count);
            Assert.IsNull(reg.Get("missing"));
        }

        [Test]
        public void ModContentParsesAFaction()
        {
            const string json = "{\"factions\":[{\"id\":\"Tideborn\",\"name\":\"Tideborn\"," +
                                 "\"offenseMultiplier\":1.1,\"defenseMultiplier\":1.2," +
                                 "\"onConfluence\":\"Reveres\",\"onMixedGifts\":\"Dislikes\"}]}";
            var parsed = ModContent.Parse(json);
            Assert.AreEqual(1, parsed.Factions.Count);
            var f = parsed.Factions[0];
            Assert.AreEqual("Tideborn", f.Id);
            Assert.AreEqual(Doctrine.Reveres, f.OnConfluence);
            Assert.AreEqual(Doctrine.Dislikes, f.OnMixedGifts);
            Assert.AreEqual(1.2f, f.Perk.DefenseMultiplier, 0.001f);
        }

        [Test]
        public void ModContentIsDefensiveAboutBadInput()
        {
            Assert.IsTrue(ModContent.Parse(null).IsEmpty);
            Assert.IsTrue(ModContent.Parse("").IsEmpty);
            Assert.IsTrue(ModContent.Parse("not json at all").IsEmpty);
        }

        [Test]
        public void ModContentDefaultsMissingFields()
        {
            var parsed = ModContent.Parse("{\"factions\":[{\"id\":\"Bare\"}]}");
            var f = parsed.Factions[0];
            Assert.AreEqual("Bare", f.Name);                       // name defaults to id
            Assert.AreEqual(1f, f.Perk.OffenseMultiplier, 0.001f); // multipliers default to 1
            Assert.AreEqual(Doctrine.Accepts, f.OnConfluence);     // doctrine defaults to Accepts
        }

        [Test]
        public void FactionRegistryHasBuiltInsAndAcceptsMods()
        {
            Assert.GreaterOrEqual(FactionRegistry.Count, 4);
            Assert.IsNotNull(FactionRegistry.Get("Symbiasts")); // a built-in, by id

            var modded = new FactionDef("Skywrights", "Skywrights", "creed", "str", "weak",
                1.2f, 0.95f, Doctrine.Accepts, Doctrine.Reveres);
            FactionRegistry.Register(modded);
            Assert.AreSame(modded, FactionRegistry.Get("Skywrights"));
            Assert.IsTrue(FactionRegistry.All.Any(f => f.Id == "Skywrights"));
            // the modded faction's own data drives its perk and attitude
            Assert.AreEqual(Attitude.Revered, modded.AttitudeToward(false, true));
        }

        [Test]
        public void ModContentParsesAnEnemy()
        {
            const string json = "{\"enemies\":[{\"id\":\"AshHound\",\"behavior\":\"Runner\",\"element\":\"Fire\"," +
                                 "\"maxHealth\":40,\"damage\":11,\"ranged\":false}]}";
            var parsed = ModContent.Parse(json);
            Assert.AreEqual(1, parsed.Enemies.Count);
            var e = parsed.Enemies[0];
            Assert.AreEqual("AshHound", e.Id);
            Assert.AreEqual(EnemyKind.Runner, e.BaseKind);
            Assert.AreEqual(Element.Fire, e.Element);
            Assert.AreEqual(40f, e.Stats.MaxHealth, 0.001f);
        }

        [Test]
        public void EnemyRegistryHasBuiltInsAndAcceptsMods()
        {
            Assert.GreaterOrEqual(EnemyRegistry.Count, 5);
            Assert.IsNotNull(EnemyRegistry.Get("Brute")); // a built-in archetype, by id

            var stats = new EnemyStats(200f, 2f, 30f, 2.5f, 2f, 500, false);
            var modded = new EnemyDef("StoneTitan", "Stone Titan", stats, Element.Earth, EnemyKind.Brute);
            EnemyRegistry.Register(modded);
            Assert.AreSame(modded, EnemyRegistry.Get("StoneTitan"));
            Assert.IsTrue(EnemyRegistry.All.Any(x => x.Id == "StoneTitan"));
        }
    }
}
