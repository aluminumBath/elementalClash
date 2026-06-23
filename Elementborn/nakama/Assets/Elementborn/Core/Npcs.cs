namespace Elementborn.Core
{
    public enum GuideNpcId { Willow, Kiana, Parfa }

    /// <summary>Willow's menagerie. Feeding each of them (its own food) over a couple of days earns a hidden-ability hint.</summary>
    public enum WillowSidekick { Gunnar, Parrot, Blobfish, Mushroom, Chameleon }

    public readonly struct SidekickInfo
    {
        public readonly string Name;
        public readonly string Description;
        public readonly string Food;

        public SidekickInfo(string name, string description, string food)
        {
            Name = name; Description = description; Food = food;
        }
    }

    public static class WillowSidekicks
    {
        public static readonly WillowSidekick[] All =
            { WillowSidekick.Gunnar, WillowSidekick.Parrot, WillowSidekick.Blobfish, WillowSidekick.Mushroom, WillowSidekick.Chameleon };

        public static SidekickInfo For(WillowSidekick s)
        {
            switch (s)
            {
                case WillowSidekick.Parrot:
                    return new SidekickInfo("the Parrot", "paints itself black every morning at 2:34am and insists it's a magical raven", "midnight sunflower seeds");
                case WillowSidekick.Blobfish:
                    return new SidekickInfo("the Blobfish", "a gross, gelatinous thing that somehow lives on land", "briny deep-jelly");
                case WillowSidekick.Mushroom:
                    return new SidekickInfo("the Mushroom", "an anthropomorphic toadstool that can hypnotize the unwary", "loamy compost truffles");
                case WillowSidekick.Chameleon:
                    return new SidekickInfo("the Chameleon", "sits in the plant pot by her window, changing color with the light", "iridescent beetles");
                default:
                    return new SidekickInfo("Gunnar", "her rock-channeling direwolf — big enough to ride", "ore-marrow bones");
            }
        }
    }

    /// <summary>What a guide NPC helps with.</summary>
    public enum NpcRole { CreatureFinder, CreatureKeeper, Locator }

    /// <summary>A guide NPC's identity, home, look, sidekick, and what they offer. Data-driven (fits the modding
    /// pass), so more guides can be added later.</summary>
    public readonly struct GuideNpcInfo
    {
        public readonly GuideNpcId Id;
        public readonly string Name;
        public readonly NpcRole Role;
        public readonly Element Element;
        public readonly SubArt SubArt;
        public readonly string Home;        // where they live
        public readonly BiomeType HomeBiome;
        public readonly string Appearance;
        public readonly string Sidekick;    // name + nature of their companion
        public readonly string Greeting;
        public readonly string ServiceLine; // their standing advice (CreatureFinder composes hints live instead)

        public GuideNpcInfo(GuideNpcId id, string name, NpcRole role, Element element, SubArt subArt,
            string home, BiomeType homeBiome, string appearance, string sidekick, string greeting, string serviceLine)
        {
            Id = id; Name = name; Role = role; Element = element; SubArt = subArt;
            Home = home; HomeBiome = homeBiome; Appearance = appearance; Sidekick = sidekick;
            Greeting = greeting; ServiceLine = serviceLine;
        }
    }

    public static class NpcCatalog
    {
        public static GuideNpcInfo For(GuideNpcId id)
        {
            switch (id)
            {
                case GuideNpcId.Kiana:
                    return new GuideNpcInfo(id, "Kiana Eclair", NpcRole.CreatureKeeper,
                        Element.Water, SubArt.SanguineGrip,
                        "a palace in the coastal capital of Tideholt", BiomeType.Beach,
                        "white hair, black eyebrows",
                        "Crickets, a massive blue fire salamander",
                        "Welcome. A creature is a bond, not a trophy.",
                        "Tame gently, keep them fed and rested, and train with patience. Mistreat a creature in my city and I will take it from you and bar you for a day — you'll start its taming over.");

                case GuideNpcId.Parfa:
                    return new GuideNpcInfo(id, "Parfa Itchonga", NpcRole.Locator,
                        Element.Fire, SubArt.Magmacraft,
                        "the forge-town of Cinderhold, by the volcano", BiomeType.Volcano,
                        "green hair flecked with red",
                        "two bickering frogs, one of air and one of water, that orbit his head",
                        "Looking for something — or someone? I know where most things are.",
                        "Tell me what you seek and I'll point you to it. I also buy surplus goods, if you're selling.");

                default:
                    return new GuideNpcInfo(GuideNpcId.Willow, "Willow M. Hyst", NpcRole.CreatureFinder,
                        Element.Water, SubArt.None,
                        "a cabin by Mistmere Lake, beneath the floating peaks", BiomeType.CloudTemple,
                        "hair that shifts between blue, red, and pink",
                        "Gunnar, a rock-channeling direwolf she rides — among a menagerie of odd companions",
                        "Hello, traveler. Hunting for a particular creature?",
                        "Ask me about a creature and I'll tell you where it lives and how to reach it. Tend my companions and I may share a secret, too.");
            }
        }
    }
}
