# Progression — XP, levels, and growth

Everything you do feeds a simple level curve, and leveling makes you tougher.

## How it works

- **`Progression`** (Core, pure) — tracks level and within-level XP. The XP to reach the next level scales with
  your level (`100 × level`), so each level is a bigger step; a big XP gain can cross several levels at once. It
  exposes `BonusMaxHealth` (`+10` per level) for other systems to apply. Unit-tested in `ProgressionTests`.
- **`ProgressionController`** (Game) — awards XP, applies the bonus, and persists. It listens on the existing
  `QuestEvents`:
  - **XP scaled by creature difficulty** for a defeat (a dragon is worth far more than a barn cat —
    `Experience.ForCreature` from the creature's health and damage; a flat fallback for unknown kinds),
  - **+75 XP** for completing a quest.
  On level-up it grants a scaling **Silver reward**, toasts, and raises the player's `Damageable` max health by
  the new bonus (capturing your base health the first time, so loading a high level applies correctly).
- **`ProgressionHud`** (Game) — an always-on bottom-left bar showing your level and a fill toward the next level,
  so progression is visible without opening the character screen.
- **Perks** (`Perks.cs`, Core, pure) — you earn **1 perk point per level** and spend points to rank up perks
  (each capped). The starting perks: **Toughness** (+20 max health/rank), **Scholar** (+10% XP/rank), and
  **Fortune** (+15% reward currency/rank). `PerkState` aggregates the effects; `ProgressionController` applies
  them (health folds into the bonus, XP gains and level-up rewards are multiplied). Unit-tested in
  `PerkStateTests`.
- **`CharacterScreenController`** (Game) — a toggled overlay (key **C**) showing level, XP toward next, the
  health bonus, your element, and the **perks** with their ranks and a spend button per perk. Refreshes live.

## Ability ladder

Your combat kit opens up as you level. Each input intent has a required level — `AbilityUnlocks` in Core, the
sibling of `StaminaCost`. The combat controller checks it before firing; a locked intent no-ops with a toast
("Sweep unlocks at Lv 2"). Because Channelers and weapon users both dispatch through the same intents, one table
gates both kits on the same levels.

Default ladder:

| Level | Unlocks |
| ----- | ------- |
| 1 | Primary cast + Defend (Dash is never gated) |
| 2 | Sweep — a wide horizontal crowd-control arc |
| 4 | Heavy — a committed power attack |
| 6 | Secondary (the charged sub-art, e.g. Fire → Lightning) and the hidden Signature move |

A level-up announces anything newly unlocked in the same toast, and handles multi-level jumps. When no
`ProgressionController` is present in a scene (e.g. a standalone Arena), gating is disabled and the full kit is
available, so combat-test scenes aren't crippled. All the numbers live in `AbilityUnlocks.Ladder`, so tuning the
pace is a one-line change. Unit-tested in `AbilityUnlocksTests`.

## Persistence

Level and within-level XP save and load through the same path as quests and items (`SaveData` →
`PlayerInventory.ToSave`/`LoadFrom` → `ProgressionController.CaptureInto`/`RestoreFrom`). Quit at level 4 and
you're level 4 on return, max-health bonus and all.

## Achievements

A milestone layer on top of progression. `AchievementProgress` (Core, pure, unit-tested) counts events per
(metric, qualifier): each event bumps both an "any" bucket and — when it carries a qualifier — a specific one, so
broad ("defeat 25 creatures") and targeted ("earn 1000 **silver**") achievements both advance off the same feed.
`AchievementCatalog` defines them across eight metrics (creatures defeated/tamed/sighted, abilities cast, items
collected, currency earned, quests completed, NPCs met). `AchievementController` (added by the bootstrap) drives it
from the `QuestEvents` bus, and on each unlock it toasts and plays the level-up fanfare; `Record` returns exactly
the achievements that crossed their target so nothing fires twice. Counts persist through `PlayerInventory`
(`SaveData.achievementKeys` / `achievementCounts`). The `AchievementsViewer` (key **K**, and on the VR hub) lists
every achievement with its progress and a `[x]` when done. To add or retune one, edit `AchievementCatalog` in
`Achievements.cs`.

## Equipment

Worn gear is another source of the same bonuses. `EquipLoadout` (Core, pure, unit-tested) tracks one item per
slot (**Armor / Charm / Trinket**) and aggregates `MaxHealthBonus` and `OffenseMultiplier` from `GearCatalog` —
which maps a handful of items to gear (Hide and Tough Leather as armor, the Elemental Charm, the Old Relic as a
trinket). `EquipmentController` (added by the bootstrap) holds the loadout: equipping requires owning the item,
its max-health bonus is folded into `ProgressionController.ApplyBonus` (so HP totals stack level + perks + gear),
and its `OffenseMultiplier` scales outgoing ability power in `PlayerCombatController` alongside the weather/faction
multipliers. Worn slots persist through `PlayerInventory`. The `EquipmentViewer` (key **V**) shows the slots, the
running totals, and Equip/Unequip buttons for the gear in your bag. To add gear, extend `GearCatalog` in
`Equipment.cs`. (It isn't on the VR hub yet — the hub list is full pending a scroll; key V works in flat play.)

## Tuning / extending

XP per defeat and per quest are serialized on `ProgressionController`. The bonus is currently max health only;
creature XP scales with difficulty, perks add ranked upgrades, and the kit unlocks across levels (see the
Ability ladder above). The natural remaining extensions are more perk effects (damage, move speed, regen —
which need hooks into those systems).
