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
| **Known** | + the main detail (where to find it, and the lure that tames it) |
| **Mastered** | + the deepest secret |

`Grimoire.Redact(entry, tier)` returns exactly the lines a given tier may see (empty layers are skipped). The
player's state is `GrimoireProgress` — savable, and a tier **never downgrades**.

Discovery is driven by doing the thing the first time:

- Bestiary: `RecordSighting` → Glimpsed, `RecordDefeat` → Known, `RecordTame` → Mastered.
- Attacks: `RecordCast(element, intent)` → Known.
- Bloodlines: `RecordBloodlineSeen` → Glimpsed, `RecordBloodlineStudied` → Mastered.

## The UI + hooks (built)

`GrimoireController` (added by the bootstrap scene, default key **G**) is the in-world tome. It owns the live
`GrimoireProgress`, builds a maroon-and-gold book panel (echoing the reference art) with three tabs, and lists each
section's entries via `GrimoireCatalog.ForSection` redacted through the player's tiers, with a discovered/total
count per tab. Undiscovered entries read `???`; revealed ones show their name, a tier tag, and the layered lines.

Discovery is wired to real gameplay through the `QuestEvents` bus (so the grimoire stays decoupled, like quests):

| Reveal | Source |
| ------ | ------ |
| Bestiary → **Glimpsed** | `CreatureController` raises `CreatureSighted` once the player first comes within its vision range |
| Bestiary → **Known** | the existing `CreatureDefeated` event (creature death) |
| Bestiary → **Mastered** | the existing `CreatureTamed` event (taming success) |
| Attacks → **Known** | `PlayerCombatController` raises `AbilityCast(element, intent)` on each resolved channeler cast (incl. the signature gesture) |
| Bloodlines → **Glimpsed** | casting an element glimpses its base line (`Bloodlines.ForElement`); on opening the book, the player's own carried lines + sub-arts + Confluence are glimpsed (`Bloodlines.TryForSubArt`) |

**Persistence** — `GrimoireController.CaptureInto`/`RestoreFrom` mirror `GrimoireProgress.ToSave`/`LoadFrom` into
`SaveData.grimoireKeys` / `grimoireTiers`, folded into `PlayerInventory.ToSave`/`LoadFrom` next to quests and items.

### Not yet wired
- **Bloodlines → Mastered** (`RecordBloodlineStudied`) is reserved for *meeting a line's notable bearer* (e.g. Ash
  → Dragonthorn). The Core call exists; the bearer→line NPC mapping is the remaining hook.
- **VR opener** — like the other overlays (J/L/I/C), the book opens on a keyboard key only. A shared world-space
  VR menu that opens all overlays is tracked in `VR_INPUT_MAP.md`.
