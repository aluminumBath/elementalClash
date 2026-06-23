using System.Collections.Generic;

namespace Elementborn.Core
{
    // ---------- Elemental stones ----------

    /// <summary>
    /// An elemental stone: dropped by a defeated Channeler (their element) and found in each capitol's throne
    /// room. A non-Channeler can carry one to a city weaponsmith to imbue a weapon or have a wand forged. Two
    /// single stones fuse into one dual-element stone — two at a time, and never undone.
    /// </summary>
    public readonly struct ElementalStone
    {
        public readonly Element Primary;
        public readonly Element? Secondary; // set only on a fused (dual) stone

        public ElementalStone(Element primary, Element? secondary = null)
        {
            Primary = primary; Secondary = secondary;
        }

        public bool IsFused => Secondary.HasValue;

        public static ElementalStone Of(Element element) => new ElementalStone(element);

        /// <summary>Fuse two single stones into one dual-element stone. Irreversible; only two at a time.</summary>
        public static bool TryFuse(ElementalStone a, ElementalStone b, out ElementalStone fused)
        {
            fused = default;
            if (a.IsFused || b.IsFused) return false; // already dual — can't fuse further (two at a time)
            if (a.Primary == b.Primary) return false; // need two different elements
            fused = new ElementalStone(a.Primary, b.Primary);
            return true;
        }
    }

    /// <summary>Where elemental stones come from.</summary>
    public static class StoneDrops
    {
        /// <summary>A defeated Channeler drops a stone of the element they channel.</summary>
        public static ElementalStone ForDefeatedChanneler(Element element) => ElementalStone.Of(element);
    }

    // ---------- Imbuing an existing weapon ----------

    /// <summary>A weapon a smith upgraded by imbuing a stone — more damage, more durability, an elemental effect.</summary>
    public readonly struct ImbuedWeapon
    {
        public readonly WeaponType BaseType;
        public readonly ElementalStone Stone;
        public readonly float DamageBonus;
        public readonly float DurabilityBonus;
        public readonly StatusKind Effect; // the rider added by the stone's primary element

        public ImbuedWeapon(WeaponType baseType, ElementalStone stone, float damageBonus,
            float durabilityBonus, StatusKind effect)
        {
            BaseType = baseType; Stone = stone; DamageBonus = damageBonus;
            DurabilityBonus = durabilityBonus; Effect = effect;
        }
    }

    public static class Imbuement
    {
        public const float DamageBonusPerStone = 8f;
        public const float DamageBonusFused = 14f;
        public const float DurabilityBonusAmount = 25f;

        /// <summary>Imbue a stone into one of the smith's weapons. Irreversible (no un-imbue).</summary>
        public static ImbuedWeapon Imbue(WeaponType weapon, ElementalStone stone)
        {
            float dmg = stone.IsFused ? DamageBonusFused : DamageBonusPerStone;
            return new ImbuedWeapon(weapon, stone, dmg, DurabilityBonusAmount, EffectFor(stone.Primary));
        }

        /// <summary>The elemental rider an element grants when imbued.</summary>
        public static StatusKind EffectFor(Element element)
        {
            switch (element)
            {
                case Element.Fire:  return StatusKind.Burn;
                case Element.Water: return StatusKind.Slow;
                case Element.Earth: return StatusKind.Control; // stagger
                case Element.Air:   return StatusKind.None;    // pure-knockback weapon, no status
                default:            return StatusKind.None;
            }
        }
    }

    // ---------- Wands ----------

    /// <summary>One elemental spell a wand grants.</summary>
    public readonly struct WandSpell
    {
        public readonly Element Element;
        public readonly string Name;
        public WandSpell(Element element, string name) { Element = element; Name = name; }
    }

    /// <summary>
    /// A wand forged from an elemental stone. When equipped it grants elemental spells and blocks every other item
    /// slot. A single-element stone yields that element's three spells; a fused stone yields both sets, capped at
    /// <see cref="WandCrafting.MaxSpells"/> (6).
    /// </summary>
    public readonly struct Wand
    {
        public readonly ElementalStone Stone;
        public readonly IReadOnlyList<WandSpell> Spells;

        public Wand(ElementalStone stone, IReadOnlyList<WandSpell> spells) { Stone = stone; Spells = spells; }

        /// <summary>Equipping a wand locks out other items.</summary>
        public bool BlocksOtherItems => true;
    }

    public static class WandCrafting
    {
        public const int MaxSpells = 6;

        /// <summary>Forge a wand from a stone: its element(s) decide the spell set, capped at <see cref="MaxSpells"/>.</summary>
        public static Wand Make(ElementalStone stone)
        {
            var spells = new List<WandSpell>();
            AddSpells(spells, stone.Primary);
            if (stone.Secondary.HasValue) AddSpells(spells, stone.Secondary.Value);
            if (spells.Count > MaxSpells) spells.RemoveRange(MaxSpells, spells.Count - MaxSpells);
            return new Wand(stone, spells);
        }

        private static void AddSpells(List<WandSpell> into, Element e)
        {
            switch (e)
            {
                case Element.Fire:
                    into.Add(new WandSpell(e, "Fire Bolt"));
                    into.Add(new WandSpell(e, "Cinder Ward"));
                    into.Add(new WandSpell(e, "Ember Nova"));
                    break;
                case Element.Water:
                    into.Add(new WandSpell(e, "Water Jet"));
                    into.Add(new WandSpell(e, "Frost Veil"));
                    into.Add(new WandSpell(e, "Tide Surge"));
                    break;
                case Element.Earth:
                    into.Add(new WandSpell(e, "Stone Shard"));
                    into.Add(new WandSpell(e, "Bulwark"));
                    into.Add(new WandSpell(e, "Quake Step"));
                    break;
                case Element.Air:
                    into.Add(new WandSpell(e, "Gale Dart"));
                    into.Add(new WandSpell(e, "Updraft"));
                    into.Add(new WandSpell(e, "Cyclone"));
                    break;
            }
        }
    }

    // ---------- The capitol weaponsmith ----------

    /// <summary>
    /// A capitol weaponsmith's services for a non-Channeler carrying a stone: imbue the stone into one of the
    /// smith's weapons, or forge a wand. Pure rules — the in-world smith NPC, its menu, stone loot drops, and the
    /// throne-room stones are wired separately (see WEAPONS.md).
    /// </summary>
    public static class WeaponSmith
    {
        /// <summary>The weapon types a smith keeps on hand to imbue.</summary>
        public static readonly WeaponType[] Offered =
            { WeaponType.Sword, WeaponType.Hammer, WeaponType.Dagger, WeaponType.LongBow, WeaponType.Shield, WeaponType.Sai };

        public static ImbuedWeapon ImbueInto(WeaponType chosen, ElementalStone stone) => Imbuement.Imbue(chosen, stone);

        public static Wand ForgeWand(ElementalStone stone) => WandCrafting.Make(stone);
    }
}
