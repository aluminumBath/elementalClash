# Factions

**Status:** in, and data-driven so it folds into the modding pass. Four **joinable** social factions sit on top
of the base elements, each with a creed, a passive strength/weakness perk, and a stance on the **Confluence**
(all elements in one person) and on **mixed gifts** (the sub-arts: lava, blood, metal, flight). These are
separate from the combat `Faction` allegiance (Player/Wild/Bandit/…).

| Faction | In a line | Strength → Weakness (perk) | Confluence | Mixed gifts |
| --- | --- | --- | --- | --- |
| **Symbiasts** | All elements equal, in peace; convergence in one soul is holy. | Resilient, balanced (off ×1.05 / def ×1.15) | Reveres | Accepts |
| **Separatists** | Keep users apart, and apart by element — "safety," mostly envy. | Aggressive but brittle (off ×1.20 / def ×0.92) | Abhors | Dislikes |
| **Cleicists** | Harmony for all elements, yet convergence is an abomination. | Stalwart (off ×1.00 / def ×1.22) | Abhors | Dislikes |
| **Synodists** | Every voice counts, weighted by population; the synod governs. | Adaptable (off ×1.10 / def ×1.05) | Accepts | Accepts |

## Perks (strengths & weaknesses that "get added")

Joining applies the faction's `FactionPerk`: the **offense** multiplier scales your casts (wired into
`PlayerCombatController` next to the weather scaling), and the **defense** multiplier becomes a standing
damage modifier on your `Damageable` — above 1 cuts incoming damage, below 1 amplifies it (so a Separatist
genuinely hits harder but takes more). `FactionMembership` on the player holds the current faction and applies
the perk on `Join`/`Leave`.

## Attitudes

`FactionAttitudes.Toward(viewer, isConfluence, hasMixedGift)` is a pure function returning Revered → Friendly →
Neutral → Unfriendly → Hostile. It encodes the lore — Symbiasts revere a Confluence, Cleicists and Separatists
are hostile to one and the Cleicists distrust mixed gifts, Synodists judge by other things — so NPC AI and
dialogue can read one consistent source when reaction systems are wired in the NPC/modding pass.

## Moddable

Factions are now **registry-backed**: `FactionRegistry` seeds itself from the built-ins below and mods add more
by JSON (see `MODDING.md`). `FactionMembership` joins by id, so a modded faction is joinable with a working
perk and data-driven attitudes. The enum `FactionCatalog` remains the built-in source the registry seeds from.

## Extending

Add a faction by extending `FactionId` and the `FactionCatalog.For` switch (creed, perk, stances) and adding it
to `Joinable`; `FactionAttitudes` is the one place to teach who regards whom. This is intentionally table-shaped
for the modding work.

## Boundaries

The perks (offense/defense) are wired; broader strengths/weaknesses (element affinities, faction-specific moves)
and **NPC reaction/hostility** driven by `FactionAttitudes` are the next layer, alongside the modding pass.
Membership is in-memory for now (persisting it is a small follow-up).

## Testing

`FactionCatalog` (every joinable faction has a profile), the perk relationships, and `FactionAttitudes` are
unit-tested.
