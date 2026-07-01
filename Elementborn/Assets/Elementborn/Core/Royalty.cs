namespace Elementborn.Core
{
    /// <summary>The realm's human Crown and the elemental houses married into it. The elemental capitals have their
    /// own rulers and the mythic Creature Kings sit above the world, but the <b>central human monarchy</b> — the
    /// Neutral Central City's Crown — was the missing seat. King Ronald and Queen Renee fill it; their metal-blooded
    /// children married into the steam house of <b>Windwyrm</b> and the earth/plant house of <b>Flowers</b>, binding
    /// the courts together. Pure data (fits the modding pass); place one with a RoyalNpcController to put them in-world.</summary>
    public enum Royal
    {
        KingRonald, QueenRenee,
        JaemysWindwyrm, SamaraWindwyrm, ConradWindwyrm,
        KellyFlowers, JaadebFlowers, JB,
        Ella, Eloc
    }

    /// <summary>A royal / noble's identity, house, home, mapped discipline, family ties, and voice. "Metal" maps to
    /// <see cref="SubArt.Oreshaping"/> (ore/metal), "Steam" to <see cref="SubArt.Steamcraft"/>, "Plant" to
    /// <see cref="SubArt.Verdancy"/>.</summary>
    public readonly struct RoyalInfo
    {
        public readonly Royal Id;
        public readonly string Name;
        public readonly string Title;
        public readonly string House;
        public readonly string Home;
        public readonly Element Element;
        public readonly SubArt SubArt;
        public readonly string Relations;
        public readonly string Personality;
        public readonly string Greeting;

        public RoyalInfo(Royal id, string name, string title, string house, string home, Element element,
                         SubArt subArt, string relations, string personality, string greeting)
        {
            Id = id; Name = name; Title = title; House = house; Home = home; Element = element;
            SubArt = subArt; Relations = relations; Personality = personality; Greeting = greeting;
        }
    }

    public static class RoyalCatalog
    {
        public static Royal[] All => (Royal[])System.Enum.GetValues(typeof(Royal));

        public static RoyalInfo For(Royal id)
        {
            switch (id)
            {
                case Royal.QueenRenee:
                    return new RoyalInfo(id, "Queen Renee", "Queen of the Neutral Crown", "the Crown",
                        "the Crown Court of the Neutral Central City", Element.Air, SubArt.None,
                        "Wife to King Ronald; mother of her own children (yet to be presented at court).",
                        "Prim, proper, and endlessly certain her opinion was invited. Drifts into conversations that were doing perfectly well without her.",
                        "Yes, yes — I couldn't HELP but overhear, and let me just say: you're holding it wrong. Whatever it is. Carry on.");

                case Royal.JaemysWindwyrm:
                    return new RoyalInfo(id, "Jaemys Windwyrm", "Steamwright of House Windwyrm", "Windwyrm",
                        "the steam-wreathed foundries of the Metal Capital", Element.Water, SubArt.Steamcraft,
                        "Husband of Samara Windwyrm; father of Conrad.",
                        "An easy, warm steam-channeler who fogs up every room he loves being in. Married up and never lets anyone forget how lucky he feels.",
                        "Mind the vents — this whole hall runs on my breath and Samara's iron. Tea? I can have it steaming before you finish the word.");

                case Royal.SamaraWindwyrm:
                    return new RoyalInfo(id, "Samara Windwyrm", "Iron Duchess of House Windwyrm", "Windwyrm",
                        "the Metal Capital, in the forges of House Windwyrm", Element.Earth, SubArt.Oreshaping,
                        "Daughter of King Ronald; wife of Jaemys; mother of Conrad; sister of Jaadeb Flowers and Ella the orphanage keeper.",
                        "A metal-shaper with a spine of the stuff. Father's daughter — dutiful, exacting, secretly soft on the creatures her sister keeps.",
                        "House Windwyrm forged half the rails you walked in on. Speak plainly and don't waste my heat.");

                case Royal.ConradWindwyrm:
                    return new RoyalInfo(id, "Conrad Windwyrm", "Young Lord of House Windwyrm", "Windwyrm",
                        "the Metal Capital, forever underfoot in the foundries", Element.Fire, SubArt.Steamcraft,
                        "Son of Jaemys and Samara Windwyrm; grandson of King Ronald.",
                        "Strong steam AND strong fire, and about as good at holding both as you'd expect from a boy who thinks a kettle is a weapon. All promise, no patience.",
                        "Watch this — no wait, back up. BACK UP. Okay, watch this.");

                case Royal.KellyFlowers:
                    return new RoyalInfo(id, "Kelly Flowers", "Lady of House Flowers", "Flowers",
                        "the terraced flower-gardens near the Earth Capital", Element.Earth, SubArt.Verdancy,
                        "Wife of Jaadeb Flowers; mother of JB.",
                        "An earth-channeler with a green thumb she's still growing into — coaxes stone to bloom, mostly on purpose. Kind, grounded, quietly stubborn.",
                        "Careful where you tread — half of what's underfoot here is trying its best to grow. Sit. Have you eaten? You look like you haven't eaten.");

                case Royal.JaadebFlowers:
                    return new RoyalInfo(id, "Jaadeb Flowers", "Lord of House Flowers", "Flowers",
                        "House Flowers, between the Earth Capital and the Metal forges", Element.Earth, SubArt.Oreshaping,
                        "Son of King Ronald; husband of Kelly; father of JB; brother of Samara Windwyrm and Ella the orphanage keeper.",
                        "A steady metal-shaper who married into gardens and learned to like getting soil under his iron nails. Level-headed where his siblings run hot.",
                        "Metal and marigolds — Kelly's idea, and she was right, as usual. What brings you to House Flowers?");

                case Royal.JB:
                    return new RoyalInfo(id, "JB", "Heir of House Flowers", "Flowers",
                        "House Flowers, usually up a tree he grew ten minutes ago", Element.Earth, SubArt.Verdancy,
                        "Son of Kelly and Jaadeb Flowers; grandson of King Ronald.",
                        "Middling plant, middling metal, boundless nerve — sprouts iron-veined vines and is very proud of each one. Cousin to Conrad, and just as much of a handful.",
                        "I grew a vine with METAL in it. Wanna see it lift a rock? It can almost lift a rock.");

                case Royal.Ella:
                    return new RoyalInfo(id, "Ella", "Keeper of the Crab-Sign Creature Orphanage", "the Crown (estranged)",
                        "the Crab-Sign Creature Orphanage in Neritha Reefwood", Element.Water, SubArt.None,
                        "Daughter of King Ronald; eloped with Eloc; sister of Samara Windwyrm and Jaadeb Flowers; keeps the orphanage that Deb the Sphinx guards.",
                        "A princess who traded a court for a creature orphanage and never once looked back. As fierce about her strays as Deb is — the two of them are a matched set. Ran off with Eloc for love and dares her father to stay cross about it (he can't).",
                        "Wipe your feet and mind the hatchlings. If Deb likes you, I like you — and she's watching, so behave.");

                case Royal.Eloc:
                    return new RoyalInfo(id, "Eloc", "Orphanage Hand, and Ella's Own", "the Crown (by elopement)",
                        "the Crab-Sign Creature Orphanage in Neritha Reefwood", Element.Earth, SubArt.Verdancy,
                        "Husband of Ella (they eloped); son-in-law of King Ronald.",
                        "The commoner gardener who ran off with a princess and spends his days mucking out orphaned creatures beside her, blissfully. Unbothered by his royal father-in-law's frostiness — Ella's smile settled that argument years ago.",
                        "You're just in time — grab a pail. Ella runs the place, Deb guards it, and I keep the greens growing. Everyone's got their post.");

                default: // Royal.KingRonald
                    return new RoyalInfo(Royal.KingRonald, "King Ronald", "King of the Neutral Crown", "the Crown",
                        "the Crown Court of the Neutral Central City", Element.Earth, SubArt.Oreshaping,
                        "Husband of Queen Renee; father of Jaadeb Flowers, Samara Windwyrm, and Ella (who eloped with Eloc to keep the Crab-Sign Creature Orphanage); grandfather of Conrad and JB.",
                        "An aging, warm-hearted monarch who has married off most of his children and misses them plainly. Pretends to still be cross that his Ella eloped, but keeps a great statue of Deb the Sphinx — who guards Ella's orphanage — in his throne room, and gazes at it fondly, mid-sentence.",
                        "Ah — forgive me, I was… admiring the sphinx again. Deb guards my Ella's orphanage, you know. My daughter ran off with a gardener to raise strays — and I have never seen her happier, curse the girl. Sit with an old king a while.");
            }
        }
    }
}
