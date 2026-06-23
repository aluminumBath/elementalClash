# Evolution mode

An alternative way to play: instead of picking your power up front, you **start with one element** and, at a
milestone, **take a second**. The pair unlocks a **specialty**.

| Combination | Specialty |
| --- | --- |
| Water + Earth | Plant control (Verdancy) |
| Water + Air | Blood (Sanguine Grip) |
| Water + Fire | Steam & Healing (Steamcraft) |
| Earth + Air | Metal / Rust (Oreshaping) |
| Earth + Fire | Lava (Magmacraft) |
| Fire + Air | Flight |

Specialties reuse the existing `SubArt` layer, so an evolved character's plant/steam/etc. powers and gates work
exactly as elsewhere (a Water+Earth evolver *is* a plant user, a Water+Fire evolver is a steam/healer who can
tend Gleamlilies, and so on).

## How it works

- **`Specialties.For(a, b)`** (Core) — the order-independent combination table above; `NameOf` gives a label.
  Pure, unit-tested.
- **`ElementalEvolution`** (Core) — holds your primary, an optional second element, and the derived specialty.
  `Evolve(second)` takes the second element (once, and it must differ); `ToLoadout()` turns the current state
  into a `ChannelerLoadout` (both elements + the specialty sub-art).
- **`EvolutionController`** (Game, on the player) — `Begin(primary)` starts a run, `Evolve(second)` grants the
  specialty, and each change rebuilds and applies the loadout to `PlayerCombatController`, so it's immediately
  live in combat.

## Boundaries

The progression and its combat effect are fully wired and tested. Hooking it into a **mode picker** at character
creation (choose "Evolution" and your starting element) and pacing **when** the second element is offered (a
quest, a level, a shrine) are light follow-ups — `EvolutionController` is the mechanism they'd drive.
