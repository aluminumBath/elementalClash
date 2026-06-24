using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class StoryArcTests
    {
        [Test]
        public void EveryChapterHasABeatInOrder()
        {
            var chapters = (StoryChapter[])System.Enum.GetValues(typeof(StoryChapter));
            Assert.AreEqual(chapters.Length, StoryArc.Beats.Length);
            for (int i = 0; i < chapters.Length; i++)
                Assert.AreEqual(chapters[i], StoryArc.Beats[i].Chapter, "beats should be in chapter order");
        }

        [Test]
        public void NextAdvancesAndClampsAtTheEnd()
        {
            Assert.AreEqual(StoryChapter.TheTowerBlast, StoryArc.Next(StoryChapter.Arrival));
            Assert.AreEqual(StoryChapter.Reckoning, StoryArc.Next(StoryChapter.FracturedRealms));
            Assert.AreEqual(StoryChapter.Reckoning, StoryArc.Next(StoryChapter.Reckoning)); // clamps
        }

        [Test]
        public void FourDistinctEndingsNoneOfWhichIsNone()
        {
            Assert.AreEqual(4, StoryArc.Endings.Length);
            var seen = new System.Collections.Generic.HashSet<StoryEnding>();
            foreach (var e in StoryArc.Endings)
            {
                Assert.AreNotEqual(StoryEnding.None, e.Ending);
                Assert.IsFalse(string.IsNullOrEmpty(e.Title));
                seen.Add(e.Ending);
            }
            Assert.AreEqual(4, seen.Count);
        }

        [Test]
        public void LookupsResolve()
        {
            Assert.AreEqual(StoryChapter.Revelation, StoryArc.BeatFor(StoryChapter.Revelation).Chapter);
            Assert.AreEqual(StoryEnding.SharedWorld, StoryArc.PathFor(StoryEnding.SharedWorld).Ending);
        }
    }
}
