using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>The known channeling lineages. Base elemental lines, the sub-art lines, the Confluence, and the
    /// rare dragon line.</summary>
    public enum BloodlineId
    {
        Pyre, Tide, Stone, Gale,                  // the four base channeling lines
        Magmacraft, Sanguine, Oreshaping, Flight, // single sub-art lines
        Verdancy, Steamcraft,                     // dual sub-art lines
        Confluence,                               // all four
        Dragonthorn                               // the rare dragon line
    }

    /// <summary>A bloodline's details: the elements it carries, its sub-art, what the blood grants, a known bearer.</summary>
    public readonly struct BloodlineInfo
    {
        public readonly BloodlineId Id;
        public readonly string Name;
        public readonly IReadOnlyList<Element> Elements;
        public readonly SubArt SubArt;
        public readonly string Trait;   // what the blood grants
        public readonly string Notable; // a known bearer ("" if unrecorded)

        public BloodlineInfo(BloodlineId id, string name, IReadOnlyList<Element> elements, SubArt subArt,
            string trait, string notable)
        {
            Id = id; Name = name; Elements = elements; SubArt = subArt; Trait = trait; Notable = notable;
        }
    }

    public static class Bloodlines
    {
        public static IEnumerable<BloodlineId> AllIds => (BloodlineId[])Enum.GetValues(typeof(BloodlineId));

        /// <summary>The base channeling line for an element. Channeling that element implies you carry its blood,
        /// so the grimoire glimpses this line on a first cast.</summary>
        public static BloodlineId ForElement(Element element)
        {
            switch (element)
            {
                case Element.Fire:  return BloodlineId.Pyre;
                case Element.Water: return BloodlineId.Tide;
                case Element.Earth: return BloodlineId.Stone;
                default:            return BloodlineId.Gale; // Element.Air
            }
        }

        /// <summary>The lineage behind a sub-art, if any. Returns false for <see cref="SubArt.None"/>.</summary>
        public static bool TryForSubArt(SubArt subArt, out BloodlineId id)
        {
            switch (subArt)
            {
                case SubArt.Magmacraft:   id = BloodlineId.Magmacraft;  return true;
                case SubArt.SanguineGrip: id = BloodlineId.Sanguine;    return true;
                case SubArt.Oreshaping:   id = BloodlineId.Oreshaping;  return true;
                case SubArt.Flight:       id = BloodlineId.Flight;      return true;
                case SubArt.Verdancy:     id = BloodlineId.Verdancy;    return true;
                case SubArt.Steamcraft:   id = BloodlineId.Steamcraft;  return true;
                default:                  id = BloodlineId.Pyre;        return false; // SubArt.None
            }
        }

        public static BloodlineInfo For(BloodlineId id)
        {
            switch (id)
            {
                case BloodlineId.Pyre:
                    return new BloodlineInfo(id, "Pyre", new[] { Element.Fire }, SubArt.None,
                        "Fire channeling carried in the blood.", "Kram Itchonga");
                case BloodlineId.Tide:
                    return new BloodlineInfo(id, "Tide", new[] { Element.Water }, SubArt.None,
                        "Water channeling carried in the blood.", "");
                case BloodlineId.Stone:
                    return new BloodlineInfo(id, "Stone", new[] { Element.Earth }, SubArt.None,
                        "Earth channeling carried in the blood.", "");
                case BloodlineId.Gale:
                    return new BloodlineInfo(id, "Gale", new[] { Element.Air }, SubArt.None,
                        "Air channeling carried in the blood.", "");
                case BloodlineId.Magmacraft:
                    return new BloodlineInfo(id, "Magmacraft", new[] { Element.Fire }, SubArt.Magmacraft,
                        "Fire turned molten — lava and ember.", "");
                case BloodlineId.Sanguine:
                    return new BloodlineInfo(id, "Sanguine", new[] { Element.Water }, SubArt.SanguineGrip,
                        "Water turned to a blood-grip hold.", "Kiana Eclair");
                case BloodlineId.Oreshaping:
                    return new BloodlineInfo(id, "Oreshaping", new[] { Element.Earth }, SubArt.Oreshaping,
                        "Earth refined into worked metal.", "");
                case BloodlineId.Flight:
                    return new BloodlineInfo(id, "Flight", new[] { Element.Air }, SubArt.Flight,
                        "Air mastery raised into true flight.", "");
                case BloodlineId.Verdancy:
                    return new BloodlineInfo(id, "Verdancy", new[] { Element.Water, Element.Earth }, SubArt.Verdancy,
                        "Water and earth coaxed into living plants.", "");
                case BloodlineId.Steamcraft:
                    return new BloodlineInfo(id, "Steamcraft", new[] { Element.Water, Element.Fire }, SubArt.Steamcraft,
                        "Water and fire braided into healing steam.", "");
                case BloodlineId.Confluence:
                    return new BloodlineInfo(id, "Confluence", new[] { Element.Fire, Element.Water, Element.Earth, Element.Air },
                        SubArt.None, "All four elements answer to one — the Confluence.", "");
                case BloodlineId.Dragonthorn:
                    return new BloodlineInfo(id, "Dragonthorn", new[] { Element.Air, Element.Water, Element.Fire },
                        SubArt.None, "Three elements and a dragon's shape to take.", "Ash Shadowthorn");
                default:
                    return new BloodlineInfo(id, id.ToString(), new[] { Element.Fire }, SubArt.None, "", "");
            }
        }
    }
}
