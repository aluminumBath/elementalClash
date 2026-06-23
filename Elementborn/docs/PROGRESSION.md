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

## Persistence

Level and within-level XP save and load through the same path as quests and items (`SaveData` →
`PlayerInventory.ToSave`/`LoadFrom` → `ProgressionController.CaptureInto`/`RestoreFrom`). Quit at level 4 and
you're level 4 on return, max-health bonus and all.

## Tuning / extending

XP per defeat and per quest are serialized on `ProgressionController`. The bonus is currently max health only;
creature XP scales with difficulty and perks add ranked upgrades; the natural extensions are more perk effects
(damage, move speed, regen — which need hooks into those systems) and ability unlocks at level thresholds.
