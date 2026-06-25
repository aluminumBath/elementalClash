namespace Elementborn.Core
{
    /// <summary>Place-setting inscriptions found on the way into a site — a line of lore the player reads as they
    /// approach whatever the site holds, so the walk in has a sense of place. Engine-free + testable;
    /// <c>SiteInteriorController</c> shows the line as the player passes the entrance-hall stele.</summary>
    public static class ApproachLore
    {
        public static string Line(SiteKind kind)
        {
            switch (kind)
            {
                case SiteKind.SunkenEntrance:
                    return "Etched in the drowned stone: \"Beyond lies the Guardian. The tide remembers those who do not return.\"";
                case SiteKind.Aerie:
                    return "Wind-scoured runes warn: \"Only the storm-touched climb past this perch. The rest are carried away.\"";
                case SiteKind.CaveMouth:
                    return "A miner's scrawl: \"Deeper still the seams glitter — and deeper still, something guards them.\"";
                case SiteKind.TempleDoor:
                    return "The lintel reads: \"Speak softly here. The old powers sleep lightly.\"";
                case SiteKind.Spring:
                    return "Mossed letters by the water: \"Drink, rest, and the wild ones may show themselves.\"";
                default:
                    return "Worn inscriptions hint that others passed this way long ago.";
            }
        }
    }
}
