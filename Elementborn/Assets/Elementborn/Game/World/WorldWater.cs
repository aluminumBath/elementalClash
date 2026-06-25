namespace Elementborn.Game
{
    /// <summary>The one authoritative sea-level Y for the world. <see cref="TerrainBuilder"/> sets it from
    /// <c>seaLevel × heightScale</c> the moment the terrain is built; everything that rides the water line —
    /// water creatures (<c>CreatureController</c>), water companions (<c>CompanionController</c>), summoned
    /// mounts and craft (<c>MountController</c> via <c>MountSummoner</c> / <c>ElementTravelController</c>) —
    /// reads it, so they all sit on the actual ocean surface instead of the old hard-coded y=0.
    ///
    /// The default matches TerrainBuilder's defaults (0.12 × 120 = 14.4), so pre-build queries and EditMode
    /// tests get the correct value even before a terrain has been generated.</summary>
    public static class WorldWater
    {
        public static float SeaLevelY { get; set; } = 14.4f;
    }
}
