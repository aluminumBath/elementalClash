# Weapons, stones & wands

A path to power for **non-Channelers** (and a top-up for anyone): elemental **stones** that a city **weaponsmith**
turns into an imbued weapon or a wand. The rules live in `Armory.cs` (Core, pure + unit-tested in `ArmoryTests`);
the in-world hooks are stubbed (see *Still to wire* below).

## Stones

An `ElementalStone` carries one element — or two, once **fused**.

- **Drops**: a defeated Channeler drops a stone of the element they channel (`StoneDrops.ForDefeatedChanneler`).
  Stones also sit in the **throne room of each capitol**.
- **Fusion**: `ElementalStone.TryFuse(a, b)` fuses two *single* stones of *different* elements into one dual stone.
  Only **two at a time** (a fused stone can't be fused again) and it **can't be undone**.

## At the weaponsmith (`WeaponSmith`)

A non-Channeler brings a stone to a capitol smith and chooses one of two services — both **irreversible**:

- **Imbue a weapon** — `WeaponSmith.ImbueInto(weaponType, stone)`. Picks one of the weapons the smith keeps on
  hand (`WeaponSmith.Offered`) and upgrades it: **+damage, +durability, and an elemental effect** keyed to the
  stone (Fire→burn, Water→slow, Earth→stagger, Air→pure knockback). A fused stone imbues harder.
- **Forge a wand** — `WeaponSmith.ForgeWand(stone)`.

## Wands

A `Wand` is built from a stone and, when equipped, grants **elemental spells** and **blocks every other item
slot** (`Wand.BlocksOtherItems`). The spell set comes from the stone's element(s), capped at
`WandCrafting.MaxSpells` = **6**:

- **single-element** stone → that element's **3** spells.
- **fused** stone → both elements' spells, **up to 6** (e.g. 3 Fire + 3 Water).

## Still to wire (the next slice)

The Core rules are complete and tested. What the in-world pass adds:

1. **Stone loot drops** — spawn the stone from `StoneDrops` when a Channeler enemy dies (hook in the enemy death
   path), as a pickup item.
2. **Throne-room stones** — place the capitol stones in the world.
3. **The smith NPC** — a `WeaponSmith` interaction (via the existing `InteractionArbiter`) with a small menu:
   choose imbue-target or wand, consuming the carried stone.
4. **Equip enforcement** — when a wand is equipped, hide/disable other item slots and surface its spells as the
   player's attacks; un-equipping restores items.
5. **Persistence** — save imbued weapons / forged wands / carried stones with the rest of the inventory.
