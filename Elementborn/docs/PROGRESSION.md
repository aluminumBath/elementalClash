# Progression — XP, levels, and growth

Everything you do feeds a simple level curve, and leveling makes you tougher.

## How it works

- **`Progression`** (Core, pure) — tracks level and within-level XP. The XP to reach the next level scales with
  your level (`100 × level`), so each level is a bigger step; a big XP gain can cross several levels at once. It
  exposes `BonusMaxHealth` (`+10` per level) for other systems to apply. Unit-tested in `ProgressionTests`.
- **`ProgressionController`** (Game) — awards XP, applies the bonus, and persists. It listens on the existing
  `QuestEvents`:
  - **+25 XP** for defeating a creature,
  - **+75 XP** for completing a quest.
  On level-up it toasts and raises the player's `Damageable` max health by the new bonus (capturing your base
  health the first time, so loading a high level applies correctly).
- **`CharacterScreenController`** (Game) — a toggled overlay (key **C**) showing level, XP toward next, the
  health bonus, and your element. Refreshes live.

## Persistence

Level and within-level XP save and load through the same path as quests and items (`SaveData` →
`PlayerInventory.ToSave`/`LoadFrom` → `ProgressionController.CaptureInto`/`RestoreFrom`). Quit at level 4 and
you're level 4 on return, max-health bonus and all.

## Tuning / extending

XP per defeat and per quest are serialized on `ProgressionController`. The bonus is currently max health only;
the natural extensions are more per-level stats (damage, stamina), unlocking abilities at thresholds, and
scaling creature XP by the creature's difficulty rather than a flat amount.
