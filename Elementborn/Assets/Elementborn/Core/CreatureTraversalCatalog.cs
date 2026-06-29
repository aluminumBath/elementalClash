using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    public static class CreatureTraversalCatalog
    {
        private static readonly Dictionary<string, CreatureTraversalType> Explicit =
            new Dictionary<string, CreatureTraversalType>(StringComparer.OrdinalIgnoreCase)
            {
                { "Abyss_Angler", CreatureTraversalType.Swimming },
                { "AirDragonfly_Gale_Dragonfly", CreatureTraversalType.Flying },
                { "AirJellyfish_Sky_Jellyfish", CreatureTraversalType.Flying },
                { "Antler_Spider_Creature", CreatureTraversalType.Land },
                { "Azure_Wave_Dragon", CreatureTraversalType.Swimming },
                { "Blue_Dino_Mount", CreatureTraversalType.Land },
                { "Blue_Fantasy_Bird", CreatureTraversalType.Flying },
                { "Blue_Gold_Tuna", CreatureTraversalType.Swimming },
                { "Boulder_Rhino", CreatureTraversalType.Land },
                { "Canopy_Monkey", CreatureTraversalType.Land },
                { "Coral_Crab_Spider", CreatureTraversalType.Swimming },
                { "Current_Eel", CreatureTraversalType.Swimming },
                { "EarthMole_Stone_Mole", CreatureTraversalType.Burrowing },
                { "Ember_Dragon", CreatureTraversalType.Flying },
                { "Fawn_Sprite", CreatureTraversalType.Land },
                { "Fire_Phoenix", CreatureTraversalType.Flying },
                { "Giant_Eagle", CreatureTraversalType.Flying },
                { "IceCat", CreatureTraversalType.Land },
                { "Leaf_Cub", CreatureTraversalType.Land },
                { "Marsh_Crocodile", CreatureTraversalType.Amphibious },
                { "Moss_Wolf", CreatureTraversalType.Land },
                { "Patchwork_Pup", CreatureTraversalType.Land },
                { "Prism_Chameleon", CreatureTraversalType.Land },
                { "Purple_Kraken", CreatureTraversalType.Swimming },
                { "Raven_Parrot", CreatureTraversalType.Flying },
                { "Shadow_Wolf", CreatureTraversalType.Land },
                { "Skyotter", CreatureTraversalType.Amphibious },
                { "Spark_Squirrel", CreatureTraversalType.Land },
                { "Storm_Wyvern", CreatureTraversalType.Flying },
                { "Teal_Fantasy_Fish", CreatureTraversalType.Swimming },
                { "Teal_Serpent", CreatureTraversalType.Swimming },
                { "Thunderbird", CreatureTraversalType.Flying },
                { "Tide_Mermaid", CreatureTraversalType.Swimming },
                { "WaterCat_Wave_Cat", CreatureTraversalType.Amphibious }
            };

        public static CreatureTraversalType GetTraversalType(string creatureName)
        {
            if (string.IsNullOrWhiteSpace(creatureName))
            {
                return CreatureTraversalType.Unknown;
            }

            string trimmed = creatureName.Trim();
            if (Explicit.TryGetValue(trimmed, out var explicitType))
            {
                return explicitType;
            }

            string key = Normalize(trimmed);

            if (ContainsAny(key, "skyotter", "otter", "frog", "turtle", "salamander", "watercat", "crocodile"))
                return CreatureTraversalType.Amphibious;

            if (ContainsAny(key, "bird", "eagle", "phoenix", "thunderbird", "dragonfly", "wyvern", "roc", "raven", "parrot", "banshee", "airjellyfish", "skyjellyfish", "dragon"))
                return CreatureTraversalType.Flying;

            if (ContainsAny(key, "fish", "eel", "angler", "tuna", "kraken", "serpent", "mermaid", "jellyfish", "crab", "reef", "coral"))
                return CreatureTraversalType.Swimming;

            if (ContainsAny(key, "mole", "worm", "beetle", "badger", "burrow"))
                return CreatureTraversalType.Burrowing;

            if (ContainsAny(key, "wolf", "rhino", "cat", "cub", "monkey", "squirrel", "hound", "dino", "prowler", "sprite", "spider", "pup", "chameleon"))
                return CreatureTraversalType.Land;

            return CreatureTraversalType.Unknown;
        }

        public static string GetDefaultMarkerLabel(CreatureTraversalType type)
        {
            return type switch
            {
                CreatureTraversalType.Land => "Land Mount",
                CreatureTraversalType.Flying => "Flying Mount",
                CreatureTraversalType.Swimming => "Swimming Mount",
                CreatureTraversalType.Amphibious => "Amphibious Mount",
                CreatureTraversalType.Burrowing => "Burrowing Mount",
                _ => "Last Ridden Creature"
            };
        }

        private static string Normalize(string value)
        {
            return value
                .ToLowerInvariant()
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty);
        }

        private static bool ContainsAny(string value, params string[] terms)
        {
            foreach (string term in terms)
            {
                if (value.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
