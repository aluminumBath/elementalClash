namespace Elementborn.Core
{
    /// <summary>
    /// Builds a where-to-find hint for a creature (Willow's trade): its habitat, how you'd reach it given how it
    /// moves, and how rare/stubborn it is to tame (from its tame chance). Pure, so it's testable and any UI can
    /// reuse it.
    /// </summary>
    public static class CreatureHints
    {
        public static string WhereToFind(CreatureKind kind)
        {
            var info = CreatureCatalog.For(kind);
            return $"The {info.Name} {Habitat(kind)}. {Approach(Locomotion.For(kind))}{Rarity(info.TameChance)}";
        }

        private static string Habitat(CreatureKind kind)
        {
            switch (kind)
            {
                case CreatureKind.Ridgewing:
                case CreatureKind.Skytyrant:
                case CreatureKind.Roc:        return "haunts the high peaks";
                case CreatureKind.Thunderbird:
                case CreatureKind.AirDragonfly:
                case CreatureKind.AirJellyfish: return "drifts through the cloud reaches";
                case CreatureKind.Glidewisp:
                case CreatureKind.Monkey:
                case CreatureKind.Direstalker:
                case CreatureKind.Tiger:      return "prowls the deep forest";
                case CreatureKind.Goldkoi:
                case CreatureKind.Skimfin:
                case CreatureKind.Tidewarden:
                case CreatureKind.Crab:
                case CreatureKind.Eel:        return "lives along the coasts and isles";
                case CreatureKind.Gillcloak:
                case CreatureKind.Crocodile:
                case CreatureKind.Snake:      return "lurks in the marshes";
                case CreatureKind.FireDragon: return "nests near the volcano";
                case CreatureKind.Rhino:      return "ranges the open plains";
                case CreatureKind.Skyotter:   return "rides the storm-fronts where sea meets sky";
                default:                       return "roams the wilds";
            }
        }

        private static string Approach(LocomotionType locomotion)
        {
            switch (locomotion)
            {
                case LocomotionType.Flying: return "You'll need to fly or scale the heights to reach it.";
                case LocomotionType.Water:  return "You'll need to dive or travel by water to find it.";
                default:                     return "It's reachable on foot, if you can track it down.";
            }
        }

        private static string Rarity(float tameChance)
        {
            if (tameChance <= 0.08f) return " It's exceptionally rare and very hard to tame.";
            if (tameChance <= 0.16f) return " It's rare and stubborn to tame.";
            return "";
        }
    }
}
