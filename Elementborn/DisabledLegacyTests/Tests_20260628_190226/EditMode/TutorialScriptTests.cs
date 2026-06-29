using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class TutorialScriptTests
    {
        [Test]
        public void Default_HasSteps_StartsAtFirst()
        {
            var t = TutorialScript.Default();
            Assert.Greater(t.Count, 0);
            Assert.AreEqual(0, t.Index);
            Assert.IsFalse(t.IsComplete);
            Assert.IsTrue(t.TryGetCurrent(out var step));
            Assert.AreEqual("wake", step.Id);
        }

        [Test]
        public void Advance_WalksThrough_ThenCompletes()
        {
            var t = TutorialScript.Default();
            int n = t.Count;
            for (int i = 0; i < n; i++) t.Advance();
            Assert.IsTrue(t.IsComplete);
            Assert.IsFalse(t.TryGetCurrent(out _));
        }

        [Test]
        public void IsLast_TrueOnFinalStep()
        {
            var t = TutorialScript.Default();
            for (int i = 0; i < t.Count - 1; i++) t.Advance();
            Assert.IsTrue(t.IsLast);
            Assert.IsFalse(t.IsComplete);
        }

        [Test]
        public void Skip_JumpsToEnd_RestartResets()
        {
            var t = TutorialScript.Default();
            t.Skip();
            Assert.IsTrue(t.IsComplete);
            t.Restart();
            Assert.AreEqual(0, t.Index);
            Assert.IsFalse(t.IsComplete);
        }
    }
}
