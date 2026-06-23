# Dedicated combat mode (arena)

**Status:** shipped. A sparring arena tuned for motion combat — escalating waves, dodgeable enemies, a combo
multiplier, and a stamina pool that paces the fight. It reuses the existing enemy stack rather than a parallel
one, and it's where each element's **fuller move kit** comes into play.

## The loop

`ArenaController` runs a simple state machine: **Ready → Active → Cleared / Defeated**. While Active it spawns
waves on a ring around the player. `ArenaProgression` (pure, tested) decides each wave: enemy count climbs
(wave 1 = 4, wave 8 = 11) and the danger level ramps 1→5, which the existing `EnemyComposition`/`EnemySelector`
use to roll tougher archetypes. The run clears after `ArenaProgression.TotalWaves` (8) or ends on a player
death. Enter restarts a finished run (a flat-play convenience; a VR button can be bound later).

## Telegraphed enemies + dodging

Arena enemies get a **windup** before they strike (`EnemyController.SetTelegraph`). During the windup the enemy
commits — it holds position — and the hit only lands if the player is still in range when it resolves. Step or
lean out of range and it **whiffs**: no damage, plus a dodge that feeds your combo. `EnemyController` raises
`AttackTelegraphed` / `AttackLanded` / `AttackDodged` so a VFX/audio layer can flash the tell. World enemies
elsewhere are unchanged — telegraph defaults to 0 (instant), so only the arena opts in.

## Combo scoring

Reuses the existing `ScoreSystem`: each kill (and each clean dodge) bumps a multiplier that decays after a few
seconds without action, so aggressive, mobile play scores best. The score/combo HUD is the one
`ScoreController` already draws; the arena adds a wave/enemies line, a centre banner, and a stamina bar.

## Stamina pacing

`StaminaModel` (pure, tested) is a regenerating pool. Every cast spends stamina by `StaminaCost` — basics are
cheap, **Heavy** costs the most, **Defend** is free — and when you're too winded the cast fizzles. This keeps
motion combat a rhythm rather than a flail. `PlayerCombatController` only consults stamina when a
`StaminaController` exists (the arena spawns one), so normal play is unaffected.

## The fuller move kit

The dedicated mode is where the extra moves matter. `AbilitySystem` now implements two more intents —
**Heavy** (a committed power attack) and **Sweep** (a wide crowd-control arc) — for every element, and the VR
gesture table maps each element's four single-hand motions to a distinct move:

| Element | Thrust | Whip | Uppercut | Slam |
| --- | --- | --- | --- | --- |
| **Fire** | blast (Primary) | fire sweep (Sweep) | lightning (Secondary) | meteor lob (Heavy) |
| **Water** | water shove (Sweep) | jet (Primary) | ice geyser (Heavy) | ice (Secondary) |
| **Earth** | ground slam (Heavy) | rock wall (Sweep) | boulder (Secondary) | rock hurl (Primary) |
| **Air** | — | scythe (Secondary) | updraft (Heavy) | downburst (Sweep) |

Air's Primary is the two-hand **Push** (gust); all elements still have the **Guard** block, a **dash**, and
Water keeps the fist+paddle **ice flow** channel. Note: the flat/gamepad scheme still emits only the original
four actions, so Heavy/Sweep are VR-gesture moves for now — adding flat bindings is a small follow-up.

## Setup (editor)

1. Drop `ArenaController` on an empty GameObject and position it where the ring should centre.
2. Assign an **enemy prefab** — the same kind a `WorldSpawnPlacer` uses (EnemyController + Damageable +
   FactionMember + CharacterController).
3. Tag the player rig **"Player"** (already used by respawn/companions). The HUD and stamina pool build
   themselves.
4. Tune `spawnRadius`, `telegraphTime`, `biome`, and the seed to taste.

## Honest boundaries

As before, there's no shipped scene or prefab — this is a component plus documentation, since I can't author
Unity scenes/prefabs here. The new moves reuse the element projectile visuals (placeholder art), and the
telegraph/stamina/spawn numbers want on-device tuning in a headset. The pieces that are pure logic
(`ArenaProgression`, `StaminaModel`, the Heavy/Sweep outcomes) are unit-tested; the AI, telegraph timing, and
HUD are runtime behaviour.
