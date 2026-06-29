using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ClimbMotionTests
    {
        private const float Max = ClimbMotion.DefaultMaxWalkableNormalY;

        [Test]
        public void FlatAndGentleGroundIsNotClimbable()
        {
            Assert.IsFalse(ClimbMotion.IsClimbable(1f, Max));   // flat ground
            Assert.IsFalse(ClimbMotion.IsClimbable(0.85f, Max)); // gentle slope, still walkable
        }

        [Test]
        public void SteepWallsAreClimbable()
        {
            Assert.IsTrue(ClimbMotion.IsClimbable(0f, Max));    // vertical wall
            Assert.IsTrue(ClimbMotion.IsClimbable(0.4f, Max));  // steep slope, too steep to walk
        }

        [Test]
        public void OverhangsAndCeilingsAreNotClimbable()
        {
            Assert.IsFalse(ClimbMotion.IsClimbable(-0.9f, Max)); // ceiling
            Assert.IsFalse(ClimbMotion.IsClimbable(-0.6f, Max)); // steep overhang
        }

        [Test]
        public void RespectsACustomWalkableCutoff()
        {
            // A more forgiving cutoff lets the player walk steeper slopes (so fewer surfaces count as climbable).
            Assert.IsFalse(ClimbMotion.IsClimbable(0.5f, 0.3f));
            Assert.IsTrue(ClimbMotion.IsClimbable(0.2f, 0.3f));
        }
    }
}
