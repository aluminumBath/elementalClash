namespace Elementborn.Core
{
    /// <summary>Who a character belongs to. Drives default hostility (see <see cref="FactionRules"/>).</summary>
    public enum Faction
    {
        Player,    // the human player — never auto-aggros (attacks are manual)
        Ally,      // a tamed companion / mount fighting for the player
        Wild,      // element-aligned fighters: tolerant of their own element, hostile to other channelers
        Bandit,    // raiders / weapon thugs: hostile to all but their own kind (still spare peaceful NPCs)
        Civilian,  // peaceful townsfolk: never initiate; fight back only if attacked
        Neutral    // passive wildlife / props
    }

    public enum Disposition { Friendly, Neutral, Hostile }

    /// <summary>
    /// Pure relationship rules. Given a viewer and a target (faction + optional element) and whether the
    /// target has provoked the viewer, returns how the viewer feels. No Unity dependency, so it tests
    /// directly. Rules in plain terms:
    ///   • Anyone who attacks you becomes hostile to you, overriding everything else.
    ///   • Player / Civilian / Neutral never auto-aggro.
    ///   • Wild(element) is hostile to channelers of a *different* element, but tolerates its own element
    ///     and ignores non-channelers (weapon users) and peaceful NPCs.
    ///   • Bandits are hostile to everyone except other bandits and peaceful NPCs.
    ///   • Allies fight Wild/Bandit and are friendly to the player.
    /// </summary>
    public static class FactionRules
    {
        public static Disposition Resolve(
            Faction viewer, Element? viewerElement,
            Faction target, Element? targetElement,
            bool targetProvokedViewer)
        {
            if (targetProvokedViewer) return Disposition.Hostile;

            switch (viewer)
            {
                case Faction.Player:
                case Faction.Civilian:
                case Faction.Neutral:
                    return Disposition.Neutral;

                case Faction.Ally:
                    if (target == Faction.Player || target == Faction.Ally) return Disposition.Friendly;
                    return (target == Faction.Wild || target == Faction.Bandit)
                        ? Disposition.Hostile : Disposition.Neutral;

                case Faction.Bandit:
                    if (target == Faction.Bandit) return Disposition.Neutral;
                    if (target == Faction.Civilian || target == Faction.Neutral) return Disposition.Neutral;
                    return Disposition.Hostile;

                case Faction.Wild:
                    if (target == Faction.Civilian || target == Faction.Neutral) return Disposition.Neutral;
                    if (!targetElement.HasValue) return Disposition.Neutral;                 // non-channeler
                    if (viewerElement.HasValue && viewerElement.Value == targetElement.Value)
                        return Disposition.Neutral;                                          // same element
                    return Disposition.Hostile;                                              // other element

                default:
                    return Disposition.Neutral;
            }
        }

        public static bool IsHostile(
            Faction viewer, Element? viewerElement,
            Faction target, Element? targetElement,
            bool targetProvokedViewer)
            => Resolve(viewer, viewerElement, target, targetElement, targetProvokedViewer) == Disposition.Hostile;
    }
}
