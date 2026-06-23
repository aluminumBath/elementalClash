# The Grimoire — a book that fills itself in

A single in-world tome with three sections that **reveal themselves as the player discovers their contents** —
nothing is handed over up front. The discovery engine and the three section catalogs live in `Grimoire.cs` and
`Bloodlines.cs` (Core, pure, unit-tested in `GrimoireTests`); the book UI and the in-gameplay discovery hooks are
the wiring step (below).

## Sections

- **Bestiary** — every creature (composed from `CreatureCatalog` + `CreatureHints`, so it tracks the live data).
- **Attacks** — each element × the moveset (Cast, Sub-art, Guard, Heavy, Sweep, Signature).
- **Bloodlines** — the channeling lineages (`Bloodlines`): the four base lines, the sub-art lines, the Confluence,
  and the rare **Dragonthorn** line (Ash's), each with its elements, what the blood grants, and a known bearer.

## How "filling in" works

Each entry has four reveal layers, gated by a per-entry **discovery tier** (`DiscoveryTier`):

| Tier | Shows |
| ---- | ----- |
| **Unknown** | nothing — the entry reads `???` and is locked |
| **Glimpsed** | the name + a one-line glimpse |
| **Known** | + the main detail |
| **Mastered** | + the deepest secret |

`Grimoire.Redact(entry, tier)` returns exactly the lines a given tier may see (empty layers are skipped). The
player's state is `GrimoireProgress` — savable, and a tier **never downgrades**.

Discovery is driven by doing the thing the first time:

- Bestiary: `RecordSighting` → Glimpsed, `RecordDefeat` → Known, `RecordTame` → Mastered.
- Attacks: `RecordCast(element, intent)` → Known.
- Bloodlines: `RecordBloodlineSeen` → Glimpsed, `RecordBloodlineStudied` → Mastered.

## Still to wire (the UI + hooks)

The Core is complete and tested. What the in-world pass adds:

1. **Discovery hooks** — call the `Record…` methods from the real events: a creature entering view (sighting),
   the defeat path, the taming success, `PlayerCombatController` on a first cast of each element/intent, and an
   encounter with a bloodline bearer.
2. **The book UI** — a `GrimoireController` overlay (tabbed Bestiary / Attacks / Bloodlines) that lists each
   section's entries via `GrimoireCatalog.ForSection` redacted through the player's `GrimoireProgress`, with a
   discovered/total count per tab. (Note: it needs a VR opener too — see `VR_INPUT_MAP.md`.)
3. **Persistence** — fold `GrimoireProgress.ToSave`/`LoadFrom` into the save file alongside quests and items.
