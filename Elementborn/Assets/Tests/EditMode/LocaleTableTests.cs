using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class LocaleTableTests
    {
        [Test]
        public void New_DefaultsToBaseLocale()
        {
            var t = new LocaleTable("en");
            Assert.AreEqual("en", t.Current);
            Assert.AreEqual("en", t.BaseCode);
            Assert.IsTrue(t.HasLocale("en"));
        }

        [Test]
        public void Get_ReturnsValue_WhenPresent()
        {
            var t = new LocaleTable("en");
            t.Set("en", "menu.quit", "Quit");
            Assert.AreEqual("Quit", t.Get("menu.quit"));
        }

        [Test]
        public void Get_MissingKey_FallsBackToKey()
        {
            var t = new LocaleTable("en");
            Assert.AreEqual("menu.unknown", t.Get("menu.unknown"));
        }

        [Test]
        public void Get_CurrentMissing_FallsBackToBase()
        {
            var t = new LocaleTable("en");
            t.Set("en", "menu.quit", "Quit");
            t.Set("es", "menu.resume", "Reanudar"); // es has resume but not quit
            t.SetLocale("es");
            Assert.AreEqual("Reanudar", t.Get("menu.resume")); // from es
            Assert.AreEqual("Quit", t.Get("menu.quit"));       // falls back to en
        }

        [Test]
        public void SetLocale_UnknownDenied_KnownApplied()
        {
            var t = new LocaleTable("en");
            t.Set("es", "k", "v");
            Assert.IsFalse(t.SetLocale("fr"));
            Assert.AreEqual("en", t.Current);
            Assert.IsTrue(t.SetLocale("es"));
            Assert.AreEqual("es", t.Current);
        }

        [Test]
        public void Has_ReflectsCurrentOrBase()
        {
            var t = new LocaleTable("en");
            t.Set("en", "a", "A");
            t.Set("es", "b", "B");
            t.SetLocale("es");
            Assert.IsTrue(t.Has("b")); // in current
            Assert.IsTrue(t.Has("a")); // in base
            Assert.IsFalse(t.Has("c"));
        }
    }
}
