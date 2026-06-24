using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class QuickTimeSequenceTests
    {
        private sealed class ScriptedRng : IRandomSource
        {
            private readonly double[] _v;
            private int _i;
            public ScriptedRng(params double[] v) { _v = v; }
            public double NextUnit() => _v[_i++ % _v.Length];
        }

        [Test]
        public void GenerateMapsUnitToActions()
        {
            var seq = QuickTimeSequence.Generate(4, 0.6f, new ScriptedRng(0.0, 0.3, 0.6, 0.9));
            Assert.AreEqual(4, seq.Length);
            Assert.AreEqual(QteAction.North, seq.Current);   // 0.0 -> 0
            seq.Press(QteAction.North);
            Assert.AreEqual(QteAction.South, seq.Current);   // 0.3 -> 1
            seq.Press(QteAction.South);
            Assert.AreEqual(QteAction.East, seq.Current);    // 0.6 -> 2
            seq.Press(QteAction.East);
            Assert.AreEqual(QteAction.West, seq.Current);    // 0.9 -> 3
        }

        [Test]
        public void GenerateAvoidsImmediateRepeats()
        {
            var seq = QuickTimeSequence.Generate(3, 0.6f, new ScriptedRng(0.0, 0.0, 0.0));
            Assert.AreEqual(QteAction.North, seq.Current); // 0 -> North
            seq.Press(QteAction.North);
            Assert.AreNotEqual(QteAction.North, seq.Current); // repeat dodged
        }

        [Test]
        public void CorrectPressesCompleteTheSequence()
        {
            var seq = new QuickTimeSequence(new[] { QteAction.North, QteAction.East }, 0.6f);
            Assert.AreEqual(QtePress.Correct, seq.Press(QteAction.North));
            Assert.AreEqual(QtePress.Completed, seq.Press(QteAction.East));
            Assert.IsTrue(seq.IsComplete);
            Assert.AreEqual(QtePress.Ignored, seq.Press(QteAction.West)); // after completion
        }

        [Test]
        public void WrongPressFails()
        {
            var seq = new QuickTimeSequence(new[] { QteAction.North, QteAction.East }, 0.6f);
            Assert.AreEqual(QtePress.Wrong, seq.Press(QteAction.South));
            Assert.IsTrue(seq.Failed);
            Assert.IsFalse(seq.IsComplete);
        }

        [Test]
        public void WindowExpiryFailsAndResetsPerStep()
        {
            var seq = new QuickTimeSequence(new[] { QteAction.North, QteAction.East }, 0.5f);
            Assert.IsFalse(seq.Tick(0.3f));
            Assert.AreEqual(QtePress.Correct, seq.Press(QteAction.North)); // resets the step clock
            Assert.IsFalse(seq.Tick(0.3f));                                // 0.3 < 0.5, still alive
            Assert.IsTrue(seq.Tick(0.3f));                                 // now 0.6 >= 0.5 -> expired
            Assert.IsTrue(seq.Failed);
        }

        [Test]
        public void ProgressAndTimeRemainingTrackTheWindow()
        {
            var seq = new QuickTimeSequence(new[] { QteAction.North }, 1.0f);
            seq.Tick(0.25f);
            Assert.AreEqual(0.25f, seq.Progress01, 1e-4f);
            Assert.AreEqual(0.75f, seq.TimeRemaining, 1e-4f);
        }
    }
}
