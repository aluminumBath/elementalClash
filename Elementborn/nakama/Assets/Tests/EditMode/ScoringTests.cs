using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ScoringTests
    {
        [Test]
        public void FirstKillScoresBasePointsAtSingleMultiplier()
        {
            var s = new ScoreSystem();
            int gained = s.AddKill(100);
            Assert.AreEqual(100, gained);
            Assert.AreEqual(100, s.Score);
            Assert.AreEqual(1, s.Combo);
            Assert.AreEqual(1, s.Multiplier);
        }

        [Test]
        public void ComboRampsTheMultiplier()
        {
            var s = new ScoreSystem();
            s.AddKill(100); // x1 -> 100
            s.AddKill(100); // x2 -> 200
            s.AddKill(100); // x3 -> 300
            Assert.AreEqual(3, s.Combo);
            Assert.AreEqual(600, s.Score);
        }

        [Test]
        public void ComboIsCappedAtMaxCombo()
        {
            var s = new ScoreSystem { MaxCombo = 3 };
            for (int i = 0; i < 10; i++) s.AddKill(10);
            Assert.AreEqual(3, s.Combo);
            Assert.AreEqual(3, s.Multiplier);
        }

        [Test]
        public void ComboDecaysAfterWindow()
        {
            var s = new ScoreSystem { ComboWindow = 2f };
            s.AddKill(100);
            s.Tick(1f);
            Assert.AreEqual(1, s.Combo);
            s.Tick(1.5f); // crosses the 2s window
            Assert.AreEqual(0, s.Combo);
        }

        [Test]
        public void HighScoreTracksBestAndSurvivesReset()
        {
            var s = new ScoreSystem();
            s.AddKill(500);
            int high = s.HighScore;
            s.Reset();
            Assert.AreEqual(0, s.Score);
            Assert.AreEqual(0, s.Combo);
            Assert.AreEqual(high, s.HighScore);
            Assert.GreaterOrEqual(s.HighScore, 500);
        }
    }
}
