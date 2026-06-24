namespace Elementborn.Core
{
    /// <summary>Pure helper for wall-climbing. A surface is climbable when it's too steep to walk up — judged by
    /// the upward component of its surface normal (1 = flat ground, 0 = a vertical wall, negative = an overhang).
    /// Engine-free and testable; the rig feeds in the normal's Y from its forward raycast and drives the rest of
    /// the climb (attach, ascend, mantle) from there.</summary>
    public static class ClimbMotion
    {
        public const float DefaultMaxWalkableNormalY = 0.6f; // steeper than ~53 degrees -> climb instead of walk

        /// <summary>True when the surface is too steep to walk (normal Y below the walkable cutoff) yet not a steep
        /// overhang/ceiling you couldn't cling to.</summary>
        public static bool IsClimbable(float surfaceNormalY, float maxWalkableNormalY)
            => surfaceNormalY < maxWalkableNormalY && surfaceNormalY > -0.5f;
    }
}
