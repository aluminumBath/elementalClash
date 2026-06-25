namespace Elementborn.Core
{
    /// <summary>The named boss that guards a boss-bearing site: a display name, the uploaded model to wear
    /// (<c>Resources/Models/Bosses/&lt;ModelName&gt;</c>), its element, how much to scale its size and health, and
    /// the reward it drops when defeated (silver + premium gems).</summary>
    public struct BossInfo
    {
        public readonly string Name;
        public readonly string ModelName;
        public readonly Element Element;
        public readonly float Scale;
        public readonly float HealthMultiplier;
        public readonly int SilverReward;
        public readonly int GemReward;

        public BossInfo(string name, string modelName, Element element, float scale, float healthMultiplier,
                        int silverReward, int gemReward)
        {
            Name = name;
            ModelName = modelName;
            Element = element;
            Scale = scale;
            HealthMultiplier = healthMultiplier;
            SilverReward = silverReward;
            GemReward = gemReward;
        }
    }

    /// <summary>Maps a site to its boss. Engine-free + testable; the Game layer (<c>SiteInteriorController</c> /
    /// <c>BossController</c>) spawns a strong enemy archetype for real stats/behaviour, then names it, dresses it
    /// in the boss model (graceful fallback to the placeholder), scales it up, buffs its health, and pays out the
    /// reward on defeat. Names and models intentionally tie to the uploaded boss assets.</summary>
    public static class BossCatalog
    {
        public static BossInfo For(SiteKind kind)
        {
            switch (kind)
            {
                case SiteKind.Aerie:
                    return new BossInfo("Stormwing Phoenix", "Prismatic_Phoenix", Element.Air, 2.3f, 6f, 220, 2);
                case SiteKind.SunkenEntrance:
                    return new BossInfo("Azure Arbor Guardian", "Azure_Arbor_Guardian", Element.Water, 2.4f, 6.5f, 260, 3);
                default:
                    return new BossInfo("Ironhorn Warden", "Ironhorn_Warden", Element.Earth, 2.2f, 5.5f, 180, 1);
            }
        }
    }
}
