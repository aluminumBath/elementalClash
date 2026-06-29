using System;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class GrimoireTests
    {
        // ---- discovery progress ----
        [Test]
        public void UnknownByDefaultThenDiscoveryRaisesTier()
        {
            var g = new GrimoireProgress();
            Assert.AreEqual(DiscoveryTier.Unknown, g.TierOf(GrimoireSection.Bestiary, "FireDragon"));
            Assert.IsFalse(g.IsDiscovered(GrimoireSection.Bestiary, "FireDragon"));
            Assert.IsTrue(g.RecordSighting(CreatureKind.FireDragon));
            Assert.AreEqual(DiscoveryTier.Glimpsed, g.TierOf(GrimoireSection.Bestiary, "FireDragon"));
            Assert.IsTrue(g.IsDiscovered(GrimoireSection.Bestiary, "FireDragon"));
        }

        [Test]
        public void SightDefeatTameRaiseProgressively()
        {
            var g = new GrimoireProgress();
            g.RecordSighting(CreatureKind.Tiger);
            Assert.AreEqual(DiscoveryTier.Glimpsed, g.TierOf(GrimoireSection.Bestiary, "Tiger"));
            g.RecordDefeat(CreatureKind.Tiger);
            Assert.AreEqual(DiscoveryTier.Known, g.TierOf(GrimoireSection.Bestiary, "Tiger"));
            g.RecordTame(CreatureKind.Tiger);
            Assert.AreEqual(DiscoveryTier.Mastered, g.TierOf(GrimoireSection.Bestiary, "Tiger"));
        }

        [Test]
        public void TierNeverDowngrades()
        {
            var g = new GrimoireProgress();
            g.RecordTame(CreatureKind.Horse);                      // Mastered
            Assert.IsFalse(g.RecordSighting(CreatureKind.Horse));  // no downgrade, no change
            Assert.AreEqual(DiscoveryTier.Mastered, g.TierOf(GrimoireSection.Bestiary, "Horse"));
        }

        [Test]
        public void CountsTrackDiscovery()
        {
            var g = new GrimoireProgress();
            Assert.AreEqual(0, g.CountDiscovered(GrimoireSection.Bestiary));
            g.RecordSighting(CreatureKind.Crab);
            g.RecordDefeat(CreatureKind.Snake);
            Assert.AreEqual(2, g.CountDiscovered(GrimoireSection.Bestiary));
            Assert.AreEqual(1, g.CountAtLeast(GrimoireSection.Bestiary, DiscoveryTier.Known)); // only the snake
        }

        [Test]
        public void ProgressSavesAndLoads()
        {
            var g = new GrimoireProgress();
            g.RecordTame(CreatureKind.Phoenix);
            g.RecordCast(Element.Fire, IntentType.Heavy);
            var reloaded = GrimoireProgress.LoadFrom(g.ToSave());
            Assert.AreEqual(DiscoveryTier.Mastered, reloaded.TierOf(GrimoireSection.Bestiary, "Phoenix"));
            Assert.AreEqual(DiscoveryTier.Known,
                reloaded.TierOf(GrimoireSection.Attacks, GrimoireCatalog.AttackId(Element.Fire, IntentType.Heavy)));
        }

        // ---- redaction ----
        [Test]
        public void UnknownEntryIsHidden()
        {
            var e = new GrimoireEntry(GrimoireSection.Bloodlines, "X", "Name", "g", "d", "m");
            var r = Grimoire.Redact(e, DiscoveryTier.Unknown);
            Assert.IsTrue(r.IsLocked);
            Assert.AreEqual(Grimoire.Hidden, r.Name);
            Assert.AreEqual(0, r.Lines.Count);
        }

        [Test]
        public void HigherTiersRevealMoreLines()
        {
            var e = new GrimoireEntry(GrimoireSection.Bloodlines, "X", "Name", "glimpse", "detail", "mastery");
            Assert.AreEqual(1, Grimoire.Redact(e, DiscoveryTier.Glimpsed).Lines.Count);
            Assert.AreEqual(2, Grimoire.Redact(e, DiscoveryTier.Known).Lines.Count);
            Assert.AreEqual(3, Grimoire.Redact(e, DiscoveryTier.Mastered).Lines.Count);
            Assert.AreEqual("Name", Grimoire.Redact(e, DiscoveryTier.Glimpsed).Name);
        }

        [Test]
        public void EmptyMasteryLineIsSkipped()
        {
            var e = new GrimoireEntry(GrimoireSection.Attacks, "Fire/Heavy", "Fire Heavy", "glimpse", "detail", "");
            Assert.AreEqual(2, Grimoire.Redact(e, DiscoveryTier.Mastered).Lines.Count); // mastery empty -> 2 lines
        }

        // ---- catalog completeness ----
        [Test]
        public void EverySectionHasEntries()
        {
            Assert.AreEqual(Enum.GetValues(typeof(CreatureKind)).Length, GrimoireCatalog.Bestiary().Count);
            Assert.AreEqual(4 * GrimoireCatalog.MovesetIntents.Length, GrimoireCatalog.Attacks().Count);
            Assert.AreEqual(Enum.GetValues(typeof(BloodlineId)).Length, GrimoireCatalog.BloodlineEntries().Count);
        }

        [Test]
        public void EveryEntryHasANameAndGlimpse()
        {
            foreach (GrimoireSection s in Enum.GetValues(typeof(GrimoireSection)))
                foreach (var e in GrimoireCatalog.ForSection(s))
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(e.Name), s + " name");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(e.Glimpse), s + ":" + e.Id + " glimpse");
                }
        }

        // ---- bloodlines ----
        [Test]
        public void DragonthornIsAshAndSanguineIsTheWaterSubArt()
        {
            StringAssert.Contains("Ash", Bloodlines.For(BloodlineId.Dragonthorn).Notable);
            Assert.AreEqual(SubArt.SanguineGrip, Bloodlines.For(BloodlineId.Sanguine).SubArt);
        }

        // ---- bloodline mapping (feeds the grimoire's discovery hooks) ----
        [Test]
        public void ForElementMapsToBaseLine()
        {
            Assert.AreEqual(BloodlineId.Pyre, Bloodlines.ForElement(Element.Fire));
            Assert.AreEqual(BloodlineId.Tide, Bloodlines.ForElement(Element.Water));
            Assert.AreEqual(BloodlineId.Stone, Bloodlines.ForElement(Element.Earth));
            Assert.AreEqual(BloodlineId.Gale, Bloodlines.ForElement(Element.Air));
        }

        [Test]
        public void TryForSubArtMapsKnownLinesAndRejectsNone()
        {
            Assert.IsTrue(Bloodlines.TryForSubArt(SubArt.Magmacraft, out var magma));
            Assert.AreEqual(BloodlineId.Magmacraft, magma);
            Assert.IsTrue(Bloodlines.TryForSubArt(SubArt.SanguineGrip, out var sang));
            Assert.AreEqual(BloodlineId.Sanguine, sang);
            Assert.IsTrue(Bloodlines.TryForSubArt(SubArt.Verdancy, out var verd));
            Assert.AreEqual(BloodlineId.Verdancy, verd);
            Assert.IsFalse(Bloodlines.TryForSubArt(SubArt.None, out _));
        }

        [Test]
        public void CastingAnElementGlimpsesItsBaseBloodline()
        {
            // Mirrors GrimoireController.OnCast: a channeler cast reveals the Attacks entry and glimpses the line.
            var g = new GrimoireProgress();
            g.RecordCast(Element.Fire, IntentType.PrimaryCast);
            g.RecordBloodlineSeen(Bloodlines.ForElement(Element.Fire));
            Assert.AreEqual(DiscoveryTier.Known,
                g.TierOf(GrimoireSection.Attacks, GrimoireCatalog.AttackId(Element.Fire, IntentType.PrimaryCast)));
            Assert.AreEqual(DiscoveryTier.Glimpsed, g.TierOf(GrimoireSection.Bloodlines, BloodlineId.Pyre.ToString()));
        }
    }
}
