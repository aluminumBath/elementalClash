namespace Elementborn.Core
{
    /// <summary>The linear spine of the campaign, opening in Concord and ending at the choice that decides the
    /// world's fate. Branching plays out at <see cref="StoryChapter.Reckoning"/> via <see cref="StoryEnding"/>.</summary>
    public enum StoryChapter
    {
        Arrival,         // a vibrant Concord; the player arrives as the realms treat
        TheTowerBlast,   // the Convergence Tower explodes; the diplomat is killed
        Investigation,   // chasing the blast through a fracturing city and its factions
        Revelation,      // the truth: the Creature Kings, the Betrayal, the failing prisons
        FracturedRealms, // open conflict — factions take sides as the borders give way
        Reckoning        // the final choice that sets the ending
    }

    /// <summary>The four fates the campaign can resolve to (full branching layered on later).</summary>
    public enum StoryEnding { None, Restoration, Dominion, HumanSupremacy, SharedWorld }

    public readonly struct StoryBeat
    {
        public readonly StoryChapter Chapter;
        public readonly string Title;
        public readonly string Summary;
        public StoryBeat(StoryChapter chapter, string title, string summary)
        { Chapter = chapter; Title = title; Summary = summary; }
    }

    public readonly struct EndingPath
    {
        public readonly StoryEnding Ending;
        public readonly string Title;
        public readonly string Summary;
        public EndingPath(StoryEnding ending, string title, string summary)
        { Ending = ending; Title = title; Summary = summary; }
    }

    /// <summary>The campaign structure as pure data: chapters in order and the four ending hooks. A future
    /// StoryController drives progress through these and records the chosen ending.</summary>
    public static class StoryArc
    {
        public static readonly StoryBeat[] Beats =
        {
            new StoryBeat(StoryChapter.Arrival, "Arrival in Concord",
                "The neutral city is alive with all four realms in uneasy festival. You arrive as Ambassador Calderon prepares to seal a new accord."),
            new StoryBeat(StoryChapter.TheTowerBlast, "The Tower Blast",
                "The Convergence Tower erupts. The Voice of Concord is dead, the accord in ashes, and every realm blames another."),
            new StoryBeat(StoryChapter.Investigation, "Ashes and Accusations",
                "You follow the blast's trail through a city turning on itself, courted and lied to by each faction in turn."),
            new StoryBeat(StoryChapter.Revelation, "What the Realms Run On",
                "The truth surfaces: civilization is powered by four imprisoned Creature Kings, their prisons are failing, and someone arranged it all long ago."),
            new StoryBeat(StoryChapter.FracturedRealms, "The Fracturing",
                "Borders buckle and hybrids spill through as the factions go to war over what to do with the dying Kings."),
            new StoryBeat(StoryChapter.Reckoning, "The Reckoning",
                "At the failing heart of the world, you choose what becomes of the Kings — and of everyone who lives on their stolen power."),
        };

        public static readonly EndingPath[] Endings =
        {
            new EndingPath(StoryEnding.Restoration, "Restoration",
                "Free the Kings and let the elements run together again — the First Convergence reborn, whatever the cost to the world built on their chains."),
            new EndingPath(StoryEnding.Dominion, "Dominion",
                "Seize the Kings' power for yourself and rule the reunited realms as the one who holds the current."),
            new EndingPath(StoryEnding.HumanSupremacy, "Human Supremacy",
                "Forge new chains and keep the Kings bound forever, so that people — not titans — command the elements on their own terms."),
            new EndingPath(StoryEnding.SharedWorld, "Shared World",
                "Refuse both cage and crown: broker a true peace between people and Kings, and build a world neither rules alone."),
        };

        /// <summary>The next chapter in the spine; the last chapter returns itself.</summary>
        public static StoryChapter Next(StoryChapter chapter)
        {
            int i = (int)chapter;
            int last = Beats.Length - 1;
            return i >= last ? (StoryChapter)last : (StoryChapter)(i + 1);
        }

        public static StoryBeat BeatFor(StoryChapter chapter)
        {
            foreach (var b in Beats) if (b.Chapter == chapter) return b;
            return Beats[0];
        }

        public static EndingPath PathFor(StoryEnding ending)
        {
            foreach (var e in Endings) if (e.Ending == ending) return e;
            return Endings[0];
        }
    }
}
