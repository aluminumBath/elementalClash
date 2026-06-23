# Modding & extensibility

**Status:** foundation in, with **factions** as the first fully-moddable content. The aim is that factions,
NPCs, attacks, and fighting styles can be added without recompiling — drop a JSON file and the game picks it up.

## How it works

Content lives in **registries** keyed by a string `id`. Built-in content is seeded in code; mods register more
through the same door, and a later registration of the same id **overrides** the built-in (so a mod can retune a
faction). The pieces:

- **`ContentRegistry<T>`** (Core) — a generic, case-insensitive id→def registry. Pure C#, unit-tested.
- **`IContentDef`** — anything registrable; just exposes `Id`.
- **`ModContent.Parse(json)`** (Core) — turns a mod file into strongly-typed defs. Defensive: bad or missing
  JSON yields nothing rather than throwing; missing numbers default sensibly; unknown enum strings fall back.
- **`ModLoader`** (Game) — at boot (`GameFlowController`), scans two folders for `*.json` and registers what it
  finds, skipping (and logging) any one bad file.

## Where mods live

| Folder | For |
| --- | --- |
| `StreamingAssets/Mods/` | mods shipped with the game (there's an `example_faction_mod.json` here) |
| `<persistentDataPath>/Mods/` | mods a player drops in (the same folder the saves/settings use) |

## Mod file format

A mod file is one JSON object with a section per content type. Today that's `factions` and `enemies`:

```json
{
  "factions": [
    {
      "id": "Emberwrights",
      "name": "The Emberwrights",
      "creed": "Fire is craft, not wrath — forge the world, never burn it.",
      "strength": "Hard-hitting artisan-warriors.",
      "weakness": "Few, and slow to trust outsiders.",
      "offenseMultiplier": 1.18,
      "defenseMultiplier": 1.0,
      "onConfluence": "Accepts",
      "onMixedGifts": "Reveres"
    }
  ]
}
```

- `id` is required and unique; everything else is optional (`name` defaults to the id, multipliers to 1,
  doctrines to `Accepts`).
- `offenseMultiplier` / `defenseMultiplier` are the perk a member gains — above 1 a strength, below 1 a
  weakness (defense below 1 means the member takes *more* damage).
- `onConfluence` / `onMixedGifts` are one of `Reveres`, `Accepts`, `Dislikes`, `Abhors` and drive how the
  faction greets a Confluence or a mixed-gift person (via `FactionAttitudes`).

A modded faction is a first-class citizen: it shows up in `FactionRegistry.All`, is joinable through
`FactionMembership.JoinById("Emberwrights")`, and its perk and attitudes come straight from its data.

Enemies look like this (under an `"enemies"` array), and modded ones spawn through
`EnemyController.ConfigureById("AshHound", …)`:

```json
{
  "enemies": [
    { "id": "AshHound", "behavior": "Runner", "element": "Fire",
      "maxHealth": 38, "moveSpeed": 6.0, "damage": 11, "ranged": false }
  ]
}
```

`behavior` is which built-in archetype to mimic (`Grunt`/`Brute`/`Runner`/`Archer`/`Elementalist`); the rest
default sensibly. `EnemyRegistry` seeds from the built-in `EnemyArchetypes`, so the five archetypes are
fetchable by id too.

## Adding a new moddable content type (the pattern)

1. A serializable `XxxDef : IContentDef` in Core (the moddable record), plus an `XxxRegistry` that seeds itself
   from the existing built-in catalog.
2. A DTO + a branch in `ModContent.Parse` (and a matching `xxx` array in the JSON).
3. One line in `ModLoader` to register the parsed defs.
4. Point the consuming system at the registry (by id) so modded entries actually appear in play.

## Roadmap (next)

Factions and **enemy archetypes** are done. Remaining, in order: **attacks** (ability outcomes) and **fighting
styles** (per-element/sub-art move mappings). The deeper part for those is letting modded entries drive the live
`AbilitySystem`/gesture resolution. Also pending for enemies: letting the random arena waves *roll* a modded
enemy (today modded enemies are registered and spawnable by id via `ConfigureById`; wiring them into the
`EnemyComposition` roll is the last step).

## Boundaries

This is **JSON/data** modding, which needs no Unity editor and ships in the build. Modded *behaviour* is limited
to what the data exposes (stats, mappings, flags) — new C# logic still means a code change. ScriptableObject
authoring (designer-side, in-editor) is a possible parallel path but isn't what this system does.

## Testing

`ContentRegistry`, `ModContent.Parse` (including bad input and defaults), and `FactionRegistry` (built-ins +
registering a modded faction) are unit-tested.
