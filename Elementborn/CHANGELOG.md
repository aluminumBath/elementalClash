# Changelog

All notable changes to Elementborn are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/); the project aims for [Semantic Versioning](https://semver.org/).
`tools/bump-version.sh` rolls the `[Unreleased]` section into a dated version on release.

## [Unreleased]

### Added
- **Traversal: jump + glide (`GlideMotion`).** The player rig can now **jump** (tap) and **glide** (hold the jump
  button while falling) — a slow, controlled descent with a little forward push, for riding down from heights and
  crossing gaps. The glide math is a pure, tested `GlideMotion` helper. Jump took the conventional **Space / A**
  slot, so **Dash moved to `Left Alt` / D-pad left** (rebindable as always; the VR jump/glide binding is TBD in the
  VR-moves pass). Pairs with the altitude-cold hazard — climb to a high point and glide back down.
- **Environmental hazards — altitude cold & underwater pressure (`EnvironmentHazards` + `EnvironmentHazardController`).**
  A pure, tested model: climb above a safe altitude and the cold drains health (ramping to a cap); dive below a safe
  depth and the pressure does the same. Exemptions reuse what's already in place — **cold**: Air/Fire channelers or a
  **Fire chest** enchant; **pressure**: Water/Earth channelers or a **Water/Earth chest _and_ Air/Water helmet**
  enchant. A bootstrap-spawned applier samples the player's altitude/depth each second and applies the rate as flat
  health loss (one-shot warning toast on entering each hazard). Dormant on the flat surface — it comes alive with the
  vertical heights and deep water in the cave pass.
- **Concord hub — the Convergence Tower & the inciting blast (`ConcordSite`).** The capital city (the world's
  `CapitalCity` region) is now Concord: `WorldSpawnPlacer` plants a `ConcordSite` there with a towering, four-
  realm **Convergence Tower** landmark (element-tinted converging orbs) and the diplomat **Ambassador Calderon**
  at its base. While the campaign sits at `Arrival`, approaching the tower triggers the inciting **tower blast**:
  it shakes the world, removes the diplomat, and advances the story to `TheTowerBlast`. A save loaded past Arrival
  builds the site in its aftermath state (no diplomat, no replayed blast). The tall spire also seeds the vertical
  world to come. Built from `ToonPalette`-tinted primitives — drop-in models can replace the parts later.
- **Story progression layer (`StoryController`).** A live, saved cursor through the campaign: it holds the current
  `StoryChapter` and the chosen `StoryEnding`, advances along the `StoryArc` spine (`Advance`/`SetChapter`) and
  records the Reckoning choice (`ChooseEnding`), raising `ChapterChanged`/`EndingChosen` for UI. It persists with
  the save (new `storyChapter`/`storyEnding` fields, captured/restored alongside the other controllers) and is
  spawned by the bootstrap scene. Scenes and scripted beats drive it — e.g. the coming tower-blast event will set
  `TheTowerBlast`.
- **Story-mode foundation.** Encoded the campaign as pure, tested data the dialogue/codex/quest layers can read:
  `Core/StoryLore` (the four imprisoned **Creature Kings** — Ignivar/Fire·Phoenix, Thalassa/Water·Tidewarden,
  **Terragor/Earth·crowned-Rhino**, Zephyreon/Air·Roc — plus the Sundering, the Great Betrayal, the neutral hub
  **Concord**, and the diplomat **Ambassador Sera Calderon**), and `Core/StoryArc` (the six-chapter linear spine
  from *Arrival in Concord* through the *Tower Blast* that kills the diplomat to the *Reckoning*, plus four ending
  hooks: Restoration / Dominion / Human Supremacy / Shared World). Added two **story factions** to keep our names
  while filling the storyline's gaps: **The Architects** (hidden) and **The Awakening** (King-cult); the joinable
  four are unchanged. A story bible lives in `docs/STORY.md`, and the diplomat has a Meshy prompt.
- **Skyotter** — a playful water/air **storm-hybrid** creature (rideable; swims and flies), added to the roster as
  the first emergent hybrid born of the weakening borders. Wired into the catalog, model aliases, and bestiary
  hints, with its prompt recorded in `docs/MESHY_PROMPTS.md`.
- **Fixed** a stale test that still expected `Tiger` to have no model (it does now); replaced it with an invariant
  that every `CreatureKind` resolves to a real model alias.
- **Enchantable armor with additive elemental resistance.** Armor is now four separate slots — **helmet, cloak,
  chest, boots** (plus charm and trinket) — and each worn piece can be **enchanted with an element** from the
  equipment panel (tap a slot's *Enchant* row to cycle Fire/Water/Earth/Air). A new pure, unit-tested
  `Core/ArmorResist` turns the equipped enchants into a per-incoming-element damage multiplier that **adds up
  across pieces**: each piece resists the element it dominates and is vulnerable to the element that dominates it
  (same cycle as the matchup), so four pieces of one element make a strong specialist with a matching glaring
  weakness, while a mixed set evens out. `Damageable` gained an `IncomingElementScale` hook that the player's
  `EquipmentController` binds to the live loadout. Base pieces (Iron Helm, Warding Cloak, Sturdy Boots; chest =
  Tough Leather) are craftable, and enchants persist across save/load.
  *Note:* expanding the armor slot enum shifts equipment save indices — pre-release saves should be re-made.
- **Mounted abilities + fighting from the saddle.** Every mount now grants its rider a **special move** on a
  dedicated, rebindable button (`Q` / right-stick press), keyed to how it travels: ground mounts **Charge**,
  water mounts **Surge**, flyers **Divebomb**. Charges and divebombs stagger foes the mount drives through, on a
  short cooldown — a pure, unit-tested `Core/MountAbilities` map drives it, executed by `MountController` with the
  burst clamped to the mount's locomotion (no charging into the terrain). Combat stays live while riding, so you
  keep fighting from the saddle — **channelers cast, non-channelers swing their equipped sword/spear/bow** with the
  normal cast button. (New `StaggerController.StaggerNearby` powers the impact.)
- **VR HUD placement pass.** The standing combat readouts — attunement, poise/stagger, and the guard/parry
  indicator — now render in the headset. A new `VrHudAnchor` (separate from the menu-oriented `VrCanvasAdapter`)
  converts each canvas to world space in VR and keeps it in the lower field of view, following the head with light
  smoothing (never dead-centre, never rigidly locked). Each readout has its own head-relative offset so they don't
  overlap (attunement lower-left, poise lower-centre, guard just above). Flat/desktop is untouched — the anchor
  no-ops. The stagger flash rides the poise canvas, so in VR it's a brief localized red tint rather than a comfort-
  unfriendly full-FOV flash. Offsets/sizes are starting values to tune on a headset. (`GameHud` currency/prompts
  and the VR move/guard *gestures* remain for their own passes — see `VR_INPUT_MAP.md`.)
- **Attunement HUD with discovery (fog-of-war matchups).** A compact left-side HUD readout shows the player's
  element and how they fare defensively against each element (RESIST / WEAK / neutral) — but every element stays
  **"???"** until the player has *encountered* it: by facing an opponent of that element (a periodic proximity
  check over live enemies, reusing `StaggerController`) or by being struck by a move of it (the player's
  `Damageable` damage feed). Your own element is known up front. Backed by a pure, unit-tested `Core/ElementDex`
  and **persisted** with the save (new `discoveredElements`, folded into `PlayerInventory` like the Grimoire).
- **Bestiary now reveals the taming item.** Each creature's Grimoire entry names the **lure** that tames it
  ("Tame it with a <name> lure…") at the **Known** tier — same unknown→discovered gating as the rest of the
  bestiary (creatures already revealed on sighting → defeat → tame). New `Damageable.Affinity` getter also lets
  the element HUD and the enemy health-bar pip read a target's element.
- **The matchup now cuts both ways + you can read it before committing.** Two follow-ups to elemental
  effectiveness: (1) the **player has a defensive affinity** — `PlayerCombatController` sets its `Damageable`
  affinity to the loadout's primary element (cleared for weapon users), so enemy attacks are scaled by the same
  chart: you resist your strong matchup and take extra from your weakness, with the existing "WEAK!"/"RESIST" cue
  firing over the player too. (2) Enemy floating health bars now carry a small **element pip** (an `ElementColor`-
  tinted square just left of the bar, built lazily from `Damageable.Affinity`) so you can see a foe's element and
  plan the matchup before swinging; non-elemental enemies show no pip. New `Damageable.Affinity` getter backs both.
- **Elemental effectiveness, wired into real fights**: the `Core/ElementMatchup` chart (Fire ▶ Earth ▶ Air ▶
  Water ▶ Fire; ×1.5 strong / ×0.6 resisted) was already applied by `Damageable` for any target with an affinity,
  but only tamed creatures and the Serpent boss set one — so it was dormant for ordinary foes. `EnemyController`
  now mirrors an element-typed enemy's element onto its `Damageable` affinity on `Configure`/`ConfigureById`
  (non-elemental bandits stay neutral), so the matchup finally bites across normal combat. Hits now show a player-
  facing cue — an orange **"WEAK!"** when the target is weak to your element, a cool **"RESIST"** when it shrugs part
  of it off — and a pure `Effectiveness` classifier (`ElementMatchup.Classify`) backs both the multiplier and the cue.
- **Guard & parry**: hold the guard input (Left-Ctrl / gamepad B, rebindable) to raise a guard driven by a pure,
  unit-tested `Core/GuardState`. The first ~0.2s is a **parry window** — a hit there is negated and
  counter-staggers the enemy in front of you (straight into a finisher), with a cyan flash + "PARRY!" cue; holding
  past the window **blocks** (70% damage cut, which also slows your poise loss since less lands). Composes cleanly
  with the existing faction damage-reduction via a new optional pre-mitigation `Damageable.IncomingModifier` hook;
  enemies gained an all-registry + `ForceStagger` so the counter can target the foe in your frontal cone. Flat/PC +
  gamepad now; VR guard gesture is part of the VR-moves pass.
- **Player poise/stagger**: the player now has the same `Core/Poise` meter (`PlayerStaggerController`, self-booting,
  found by tag). Incoming hits build a slim amber HUD bar; when it breaks the player is briefly stunned — which
  `PlayerCombatController` already honours, so the stagger interrupts your offense — and the screen flashes red with
  a "STAGGERED!" warning. Tankier than enemies and a shorter window; never a finisher target. Spacing/defense now
  matters under pressure.
- **Poise meter + stagger cue**: the floating enemy health bar now carries a thin poise sub-bar that fills (light
  blue) toward a break and reads full/gold while the enemy is staggered, so the break is telegraphed. The break
  itself fires a bright flash + a "STAGGER!" popup at the enemy.
- **Stagger & finishers**: real hits build a pure, unit-tested `Core/Poise` meter on every enemy
  (`Combat/StaggerController`, auto-required); on break the enemy is stunned (`EnemyController` already freezes on
  it) and flagged staggered. `Combat/FinisherController` shows a prompt near a staggered enemy and, on the finisher
  input (keyboard F / gamepad north), runs a `QuickTimeController` quick-time move — success executes the enemy
  (defeat poof + score). VR finisher gesture is part of the VR-moves pass (calls `QuickTimeController.SubmitAction`).
- **Elemental effectiveness**: `Core/ElementMatchup` (pure, unit-tested) scores attacker-vs-defender element on a
  Fire ▶ Earth ▶ Air ▶ Water ▶ Fire cycle (×1.5 strong / ×0.6 weak / ×1 neutral). `Damageable` gained an optional
  affinity (`SetAffinity`) and applies the multiplier in `Apply` before damage/feedback, so floating numbers show
  the effective hit. Creatures set their affinity from their element; the water serpent is Water (resists Water,
  weak to Air). Enemies have none unless assigned.
- **VR admin-log gesture**: `WristMenuGesture` (self-bootstrapping) toggles `AdminLogConsole` when you look at your
  left wrist and tap it with the right hand — HMD + controller poses from XR nodes through the unit-tested
  `Core/WristGesture`, gated by `AdminUnlocked`. Flat/PC keeps F2. (Approximates the tap; refine with hand-tracking.)
- **QTE VR input seam**: `QuickTimeController.SubmitAction(action)` lets a VR move/gesture detector feed quick-time
  presses (keyboard/gamepad still polled). The move → action binding is left to the VR-moves pass (VR_INPUT_MAP.md).
- **Quick-time complex moves**: `Core/QuickTimeSequence` (pure, unit-tested) generates a random series of
  direction/face-button presses, each with a reaction-time window; `Game/QuickTimeController` (self-bootstrapping)
  shows the prompts, reads keyboard/gamepad input, ticks the window, and fires success/failure callbacks. Optional
  `Game/QuickTimeMoveTrigger` is a designer-droppable hook (UnityEvents) to bind a complex move. (Trigger
  condition + VR button mapping left as design hooks — call `QuickTimeController.Begin` where the move should live.)
- **Admin live-log console**: `Game/AdminLogConsole` (self-bootstrapping) captures all Unity logs into a pure,
  unit-tested `Core/LogRing` and renders the recent lines in a screen-space panel, colour-coded by severity.
  Toggle with F2 (admin-gated via `AdminUnlocked`, on in editor/dev builds) or `AdminLogConsole.Toggle()`;
  `AdminLogConsole.Push` can surface other telemetry. (Headset toggle + gameplay-event feed are noted follow-ups.)
- **Boss health bar**: `Game/BossHealthBar` (self-bootstrapping, reusable) draws a screen-anchored name + wide
  drain bar for a boss, reusing `Core/HealthBarState`; the water serpent engages it on first hit and hides it on
  defeat. Any boss calls `BossHealthBar.Engage(damageable, name)`.
- **Hit-direction knockback**: unified through `Game/Combat/KnockbackImpulse` + the unit-tested
  `Core/KnockbackForce` — projectile, melee, and heavy hits now flatten knockback to the ground plane and add a
  small upward pop (clamped to a sane ceiling), so shoves read clearly and no longer inherit the caster's aim pitch.
- **Enemy health bars**: `Combat/EnemyHealthBar` (auto-required on every `EnemyController`) floats a billboarded
  bar over each enemy from two `ToonPalette`-tinted quads — appears on damage, retracts after a lull, drains from
  the left, and steps green → yellow → red (fill + fade from the unit-tested `Core/HealthBarState`). It's a
  detached object so `HitReaction`'s squash never catches it, and it's destroyed with the enemy.
- **Defeat effect**: on `CombatFeedback.Defeated`, `Feel/DeathPoof` bursts element-tinted `ToonPalette` shards
  (`Feel/Shard`) along the deterministic, unit-tested `Core/ShardBurst` spread — they scatter, tumble, and shrink
  away — and enemies pop a gold "+score" number via the shared `Feel/FloatingText`. See `docs/MODELS.md`.
- **Floating damage numbers**: `Feel/DamageNumbers` (self-bootstrapping) spawns a world-space `Feel/FloatingNumber`
  over every real hit off `CombatFeedback.Hit` — a TextMeshPro that billboards, climbs, pops, and fades, then
  self-destroys, with motion from the unit-tested `Core/DamagePopup` and element tint from the shared
  `Game/ElementColor`. Size grows with hit strength; uses TMP's default font and stays silent if none is set up.
  Also gated `Combat/HitReaction` to hits ≥ 1 so burn/DoT ticks no longer flicker the target. See `docs/MODELS.md`.
- **Combat impact feedback**: real damage now drives the game-feel layer. `Damageable` broadcasts a global
  `CombatFeedback.Hit`/`Defeated` (world position + amount + element); `Combat/HitReaction` (auto-required on every
  `EnemyController`) squash-pops the struck model and flashes it white via a MaterialPropertyBlock (originals
  restored, no allocations, CharacterController untouched); `Feel/FlashFeedback` pops an element-tinted spark per
  hit and a burst on defeat; `Feel/CameraShaker` shakes for nearby hits and `Feel/HitStop` punches time for heavy
  nearby hits — both gated by distance + strength (`Core/HitFeedback`, unit-tested) so distant/light hits stay
  calm. See `docs/MODELS.md`.
- **Game-feel subscribers**: concrete, asset-free reactions to the animation events — `Feel/CameraShaker`
  (decaying screen shake via unit-tested `Core/ShakeOffset`; third-person camera only, post-rig execution order,
  unscaled), `Feel/HitStop` (brief `Time.timeScale` dip on impact), and `Feel/FlashFeedback` (fading point-light
  spark on impact / cyan burst on cast, via `Feel/TransientLight` + `Feel/LightFade`). All listen on new static
  broadcast events on `AnimationEventReceiver` (`AnyImpacted`/`AnyLanded`/`AnyCastReleased`/`AnyWasHurt`), so they
  need no reference to the runtime-spawned player. See `docs/MODELS.md`.
- **Meshy prompt sheet** (`docs/MESHY_PROMPTS.md`) expanded from the 13 missing creatures to **everything still
  without a model**: 5 sidekick pets (incl. the unmatched Chameleon), 3 weapons (Dagger, Sai, a real War Hammer),
  3 consumable potions, 2 materials, 5 foods, and a rig-ready player hero — each with a paste-ready prompt and a
  suggested alias name, organized under a section index.
- **Animation event hooks**: `Game/AnimationEventReceiver` exposes 14 hooks for Unity Animation Events on the
  player's rigged clips — footsteps (incl. left/right/water), jump, land, attack windup/swing/impact, cast
  charge/release, dodge, hurt, vocalize — each playing the right sound (new `footstep`/`jump`/`land` SFX added)
  and raising a positioned C# event for VFX, camera shake, hit-stop, or damage timing. `Game/ProceduralFootsteps`
  drives footsteps from distance travelled (`Core/FootstepCadence`, unit-tested) when clips have no authored
  events, and the binder adds it to the static-mesh fallback automatically. See `docs/MODELS.md`. the third-person rig now adds a `PlayerModelBinder` that prefers a rigged humanoid
  prefab (skinned mesh + Animator) at `Models/Characters/PlayerRigged`, driving its idle/walk/run from the body's
  measured speed via a `Speed` blend param (`Core/LocomotionAnimation`, unit-tested), and falls back to the
  static mesh when none exists.
- **Items in the world**: `Core/ItemModelNames` maps catalog items to gem/relic models and `Game/WorldItemPickup`
  shows an item's 3D model and grants it on touch (ember_shard, river_pearl, old_relic, elemental_charm mapped;
  others keep a primitive and stay icon-only in the UI). See `docs/MODELS.md`. `Core/ModelBindings.cs` adds `NpcModelNames` (Willow/Kiana/Parfa → humanoids),
  `SidekickModelNames` (Willow's pets → loose stand-ins), `WeaponModelNames` (weapon pickups → gear),
  `PlayerModelNames` (third-person player model), and a `PropCatalog` reference registry for structures/VFX. A generic
  `Game/ModelLibrary.Attach(path, host)` loads them (primitive fallback), wired into `GuideNpcController`,
  `WeaponPickup`, `ThirdPersonRig`, and `NpcSidekickOrbiter` (opt-in). The import script now covers every
  category. Items (consumables/materials) stay icon-only; the player model is a static mesh (no skeletal anim
  yet). See `docs/MODELS.md`.
- **Meshy prompt sheet** (`docs/MESHY_PROMPTS.md`): ready-to-paste text-to-3D prompts for the 13 creature kinds
  with no model (FireDragon, Mermaid, Tiger, …), element-themed and matched to the cel-shaded look. `CreatureModelNames.Aliases` now maps 18 `CreatureKind`s to the Meshy AI models
  in `Assets/generated_assets` (e.g. WaterDragon→Azure_Wave_Dragon, Phoenix→Fire_Phoenix, Direstalker→Shadow_Wolf).
  `tools/import-creature-models.sh` extracts the mapped models into `Resources/Models/Creatures/<alias>/`. Kinds
  with no close model (FireDragon, Mermaid, Tiger, …) keep the primitive fallback. See `docs/MODELS.md`.
- **Event logs → Neon via Nakama RPC**: the `events_ingest` server RPC (`nakama/src/main.ts`) creates the tables
  on startup and inserts each event with `nk.sqlExec` (writes to whatever DB Nakama uses — point it at Neon via
  `NAKAMA_DB_ADDRESS`). The client `NeonEventSink` (behind `ELEMENTBORN_NAKAMA`) ships batches and is installed by
  `NakamaSocialInstaller`. The DB credential stays server-side only. See `docs/EVENT_LOGGING.md`.
- **Per-session event logging**: every play session now opens a uniquely-identified, append-only event log
  (`Core/SessionEventLog`, pure + unit-tested) fed by a self-bootstrapping `GameEventLogger`. It auto-captures
  gameplay off the `QuestEvents` bus (casts, defeats, tames, summons, crafts, equips, currency, items, NPC talk,
  quests) and offers typed `Log*` calls for logins (id + name, **never passwords**), action errors, move math
  totals, statuses, spawn/respawn points, and the final leaderboard. Events buffer and flush in JSON batches
  through an `IEventSink` (console by default; a Neon-Postgres sink routes through the server so the DB
  connection string never ships in the client). Wired: session start/end, the bus, login (`SocialHub`), respawn
  and respawn-anchor (`RespawnController` / `CheckpointObject`), move-damage math (`PlayerCombatController`), and
  health/level status. Includes the Neon schema. See
  `docs/EVENT_LOGGING.md`.
- **Login-streak bonus**: claiming the daily summon now advances a consecutive-day streak (continues on the next
  UTC day, resets if a day is skipped) and pays escalating Sigils on a 7-day cycle (`40/40/60/60/80/100/200`, day
  7 a milestone) plus a capped per-week loyalty bonus (`+10`/week, max `+100`). The Beacon panel shows the streak
  day and the next bonus; rules are a pure, unit-tested `LoginStreak`; the streak persists. See `docs/GACHA.md`.

### Changed
- **Category-aware shop sell prices**: buy-back is no longer a flat 50% of value. `Shop.SellFractionFor` now
  scales by item category — sell-only **treasure** ~90%, **tools**/**consumables** ~40%, **materials**/**food**
  the base 50% — all below full price so there's no buy-and-resell arbitrage. See `docs/ITEMS.md`.
- **Daily free summon**: a once-per-day no-cost pull on the standard Wild Beacon, surfaced at the top of the
  Beacon panel (with a countdown when not yet available). Refreshes at UTC midnight via a pure, unit-tested
  `DailySummon` helper; the controller persists the last-claim time. The free pull runs full resolution (rates,
  pity, refunds, history, stats) and fires the `SummonPerformed` quest hook, so it can complete *Answer the
  Beacon*. The paid and free paths now share one `ResolvePull` routine. See `docs/GACHA.md`.
- **Per-creature model binding (with placeholder fallback)**: creatures can now show real models instead of one
  shared primitive prefab. Drop a prefab/`.fbx` at `Resources/Models/Creatures/<CreatureKind>` (or add a
  `CreatureModelNames.Aliases` entry if your file is named differently) and `CreatureModelLibrary` loads it and
  attaches it to spawned creatures, hiding the placeholder. If no model is present the placeholder stays, so the
  game is unchanged until art is added — bind creatures one at a time. Hooked into `CreatureController` (wild /
  tamed), `MountSummoner`, and `CompanionSummoner`; pure path resolver is unit-tested. See `docs/MODELS.md`.
- **Quest chaining (prerequisites)**: a `QuestDefinition` can now list prerequisite quest ids; a quest is only
  offered and only startable once every prerequisite is *Completed* (`QuestLog.PrerequisitesMet`, enforced in
  `AvailableFrom` and `Start`). The onboarding quests now form two short chains — Willow: **First Channeling →
  Answer the Beacon → Claim the Featured**; Parfa: **First Craft → Gear Up**. See `docs/QUESTS.md`.
- **"Claim the Featured" objective (teaches the Motes exchange)**: a new `ObjectiveKind.ClaimFeatured` and a
  `QuestEvents.FeaturedClaimed` event (raised by `SummonController.ClaimFeatured`), wired through `QuestController`.
  The new **Claim the Featured** quest (50 Silver + 100 Sigils) asks you to spend Motes on a banner's featured
  creature, closing the summon-economy loop. See `docs/GACHA.md`.
- **Onboarding quests for the core procedures**: a short starter line that teaches each basic action by doing it
  once — **Answer the Beacon** (summon), **First Channeling** (channel an element), **Gear Up** (equip gear), and
  **First Craft** (craft an item). Backed by four new `ObjectiveKind`s (`SummonCreature`, `CastAbility`,
  `EquipItem`, `CraftItem`) and the events that feed them: a new `QuestEvents.SummonPerformed` (raised once per
  Beacon roll), `ItemEquipped` (raised by `EquipmentController.Equip`), `ItemCrafted` (raised by the crafting
  flow), and the existing `AbilityCast` now also forwarded into the `QuestLog`. See `docs/QUESTS.md`.
- **Recent-pulls history**: the Summon Beacon panel now shows a **Recent pulls** log — your last few notable
  summons (Epic or better) with tier, creature, banner, featured marker, and a relative age ("2h ago"). It's a
  pure, unit-tested newest-first ring buffer (`SummonHistory`, capacity 8) that `SummonController` records each
  notable pull into (timestamped from its clock seam) and persists via `SaveData`. See `docs/GACHA.md`.
- **Summon history / stats**: the Summon Beacon now keeps a lifetime, all-banner tally — total pulls, counts by
  tier, featured wins, and Sigils spent / Motes earned — surfaced as a "Summon history" section in the panel
  (alongside per-banner pulls and the observed Legendary rate). It's a pure, unit-tested `SummonStats` that
  `SummonController` feeds on every roll and persists via `SaveData`. See `docs/GACHA.md`.
- **Quests can reward Sigils**: `QuestReward` gained an optional `Sigils` amount (0 = none), granted on turn-in
  through `SummonController.AddSigils` and shown in the dialogue reward line and the completion toast. Several
  starter quests now grant Sigils (A Wild Start +120, A Gentle Hand +200, Pelts for the Tanner +160), giving the
  Beacon an in-world faucet beyond level-ups. See `docs/QUESTS.md` and `docs/GACHA.md`.
- **Summon Beacon (gacha loop)**: a repeatable, currency-gated summon over the existing creature roster, so it
  needs no new art — everything pulled drops into the roster usable by the companion/mount summoners. `Summoning`
  (Core, pure, unit-tested) defines the rarity tiers, banners, tunable `SummonConfig`, per-banner `SummonState`,
  and the seedable resolver `SummonRoller` (tier roll honouring **hard pity**, the featured **50/50 + guarantee**,
  and a ten-pull **Epic floor**); `SummonBannerCatalog` holds the two banners (no-rate-up **Wild Beacon** and
  Phoenix-rate-up **Flamecaller Beacon**) and is the single rarity knob. `SummonController` owns two new
  currencies — **Sigils** (pulls; 1600 to start, +60 per level via `Progression.LeveledUp`) and **Motes**
  (duplicate dust) — spends and rolls, grants new creatures via the new `PlayerInventory.GrantOwned` or refunds
  Motes for duplicates, and lets Motes claim a banner's featured creature outright; state persists through
  `SaveData`. The `SummonViewer` (key **U**, and on the VR hub) shows balances, the banner selector, pity,
  ×1/×10 pulls, the Motes claim, the last results, and a collection roster. See `docs/GACHA.md`.
- **Featured-banner rotation**: the Summon Beacon's featured slot now **rotates** — one themed beacon per
  Legendary (Flamecaller/Phoenix, Emberwyrm/Fire Dragon, Tidewyrm/Water Dragon, Stormcrown/Skytyrant,
  Deepward/Tidewarden, Nightprowl/Direstalker). `SummonBannerCatalog.FeaturedForPeriod` builds the active banner
  under a **stable slot id** so pity and the lost-50/50 guarantee carry across rotations; the period is whole
  `rotationPeriodDays`-day windows (default 7) since a fixed UTC epoch via the pure, unit-tested
  `SummonBannerCatalog.PeriodFor` (the controller passes `DateTime.UtcNow`, overridable with `SetClock`). The
  `SummonViewer` shows the current featured banner and a "rotates in N days" countdown. See `docs/GACHA.md`.
- **Summon sounds**: the Beacon has its own placeholder SFX (4 new, synthesized by `make_sfx.py`) — a "cast"
  whoosh (`SummonPull`) on a roll, then a per-tier **reveal sting** (`SummonRare` / `SummonEpic` /
  `SummonLegendary`) chosen by the pure `AudioController.SfxForSummon`; a Motes claim plays the Legendary sting.
  Replaces the Beacon's earlier reuse of the generic level-up/confirm sounds. See `docs/AUDIO.md`.
- **Equippable gear**: crafted/looted items can now be **worn** for passive bonuses. `EquipLoadout` (Core, pure,
  unit-tested) holds one item per slot (Armor / Charm / Trinket) and sums `MaxHealthBonus` + `OffenseMultiplier`
  from `GearCatalog` (Hide / Tough Leather armor, the Elemental Charm, the Old Relic trinket). `EquipmentController`
  folds max-health into `ProgressionController` (stacking with level + perks) and the power multiplier into
  `PlayerCombatController` (alongside weather/faction), persisting worn slots via `PlayerInventory`. The
  `EquipmentViewer` (key **V**) shows slots, totals, and equip/unequip. See `docs/PROGRESSION.md`. (Not on the VR
  hub yet — that list is full pending a scroll.)
- **Usable consumables**: consumables can now be **used from the inventory** (key I) — each shows a Use button.
  `Consumables` (Core, pure, unit-tested) maps an item to a `ConsumableEffect` (heal amount + stamina refill); the
  inventory applies it to the player's `Health.Heal` / `StaminaController.Refill` and spends one. Healing Tonic
  heals, Stamina Draught refills stamina, Elixir of Vigor does both. See `docs/ITEMS.md`.
- **Crafting**: a `RecipeBook` (Core) of recipes turning loot materials into gear/consumables, with pure
  unit-tested `Crafting.CanCraft` / `Crafting.Missing` checks. The `CraftingViewer` (key **B**, and on the VR hub)
  lists each recipe with held counts and crafts on click — consuming inputs via `Items.Remove` and granting the
  output via `PlayerInventory.AddItem` (which counts toward the Collector achievement). Recipes upgrade raw drops
  into the consumables and three new craft-only items (Tough Leather, Elixir of Vigor, the four-element Elemental
  Charm) plus a Reforged Relic. See `docs/ITEMS.md`.
- **Achievements / milestones**: a pure, unit-tested `AchievementProgress` tracker (counts per metric + qualifier,
  so "any" and targeted achievements share one feed) over an `AchievementCatalog` of 13 across eight metrics.
  `AchievementController` drives it from the `QuestEvents` bus (defeats, tames, sightings, casts, items, currency,
  quests, NPCs), toasts + plays the level-up fanfare on each unlock (`Record` returns only the ones that just
  crossed), and persists via `PlayerInventory`. An `AchievementsViewer` (key **K**, and on the VR hub) shows the
  checklist with progress. See `docs/PROGRESSION.md`.
- **Loot drops**: defeated creatures now roll a **weighted `LootTable`** (Core) instead of a hardcoded `hide` —
  `Rolls` weighted picks with quantity ranges and a "nothing" slot, seeded through `IRandomSource` so it's
  deterministic and unit-tested. `LootTables.For(kind)` themes drops by element/habitat (fire/water/earth/air/apex)
  over a beast baseline, every entry a real `ItemCatalog` id. `CreatureController.OnDefeated` grants the rolls and
  toasts the haul. See `docs/ITEMS.md`.
- **Interact works in VR**: `VrInteractInput` reads the right-hand grip (legacy XR `CommonUsages.gripButton`) and
  signals the shared `InteractionArbiter`, firing the current best interaction through the same path the desktop
  Interact key uses — so NPCs, pickups/activation, mounting, taming, and the rift/checkpoint prompts are reachable
  from the headset. This closes `VR_INPUT_MAP.md`'s highest-impact gap; the remaining VR gap is the menu/overlay UI
  (screen-space today — needs the world-space + XRI ray-interaction pass documented there).
- **VR menu overlays**: a `VrOverlayHub` (left-hand menu button in a headset, Tab on desktop) opens a panel that
  opens each overlay — Quests / Inventory / Grimoire / Map / Social / Character / Settings — through a new public
  `Open()` on each. Overlay canvases switch to World Space in VR via `VrCanvasAdapter` (gated by a shared
  `XrState.Active`), placed in front of the player on open. This closes the menu half of the VR gap above; the
  controller-ray *click* still needs the in-editor XRI `TrackedDeviceGraphicRaycaster` + `XRUIInputModule` the
  creation UI documents.
- **VR hub covers the action verbs**: Save / Load, Element Travel, Summon Mount, and Summon Companion are now
  entries on `VrOverlayHub` (an "Actions" group), each calling the system's existing public method — so a headset
  player can reach them without the keyboard. The only remaining VR input gap is the in-editor XRI ray-click
  raycaster; dedicated controller gestures for the verbs (vs. menu entries) would be later polish.
- **Checkpoints (respawn shrines)**: a new `Checkpoint` type + canonical `WorldMap.Checkpoints` (cardinal
  waystones), a pure unit-tested `CheckpointLog` (activated set + active anchor), and the runtime `CheckpointState`
  / `CheckpointObject` / `CheckpointSpawner` (amber obelisks with an Interact to set your respawn). `RespawnController`
  now revives at the active checkpoint → house → spawn. They draw on the map viewer and minimap (finally using
  `MapMarkerKind.Checkpoint`), persist via `SaveData.activatedCheckpoints`/`activeCheckpoint`, and play a `UiConfirm`
  on activation. The bootstrap sandbox spawns the shrines.
- **Map systems (Core)**: `MapNavigation` — **leyline-rift fast travel** (`FastTravelNetwork`: warp only to
  discovered rifts, nearest-rift, savable), **locate self** always (`Locator.Self`) and **locate friends** only
  with explicit opt-in (`LocationSharing`, off by default; `Locator.VisibleFriends` is consent-gated), plus
  **minimap math** (`Minimap.WorldToNormalized` / `WithinRange`) and `MapMarker`s. Pure rules + tests. **Now wired
  in-world**: `MapState` (runtime owner, seeded from a canonical `WorldMap` of seven rifts; persisted via
  `SaveData.discoveredRifts`/`shareLocation` through `PlayerInventory`), an always-on `MinimapHud`, a full
  `MapViewerController` (key **M**, also opened from a rift) drawing the overworld backdrop with tap-to-fast-travel,
  `LeylineRiftObject`/`LeylineRiftSpawner` (discover-on-approach crystals via the `InteractionArbiter`), and a
  shared `RigTeleporter` (the safe respawn-style warp). The overworld key-art is installed at
  `Resources/ElementbornUI/worldmap`. The friend-position feed is now wired too: a pure `PresenceRegistry` (Core,
  TTL'd, tested), an `IFriendPresence` producer seam, `MapState` polling with a consent-gated refresh, and friend
  markers on both the viewer and the minimap — exercised offline by a `SimulatedFriendPresence` demo (orbits a
  seeded ally). The online producer is implemented too: `NakamaFriendPresence` (behind `ELEMENTBORN_NAKAMA`)
  broadcasts position as the player's Nakama status via the pure, tested `PresenceCodec` and follows friends to
  receive theirs, registered by `NakamaSocialInstaller` after connect — verified against a live server, like the
  rest of the Nakama layer.
- **The Grimoire** — a discovery-driven tome with three sections that fill in as the player uncovers them:
  **Bestiary** (every creature, composed from `CreatureCatalog`/`CreatureHints`), **Attacks** (each element ×
  the moveset), and **Bloodlines** (a new `Bloodlines` catalog: the four base lines, the sub-art lines, the
  Confluence, and the rare **Dragonthorn** line — Ash's). Each entry reveals in tiers (`DiscoveryTier`:
  Unknown → Glimpsed → Known → Mastered) via `Grimoire.Redact`; the player's `GrimoireProgress` is savable and
  never downgrades. Core engine + catalogs are complete and unit-tested. **Now wired in-world**: a
  `GrimoireController` overlay (default key **G**, maroon-and-gold tome, three tabs with discovered/total counts,
  redacted entries), discovery hooks via the `QuestEvents` bus (a new `CreatureSighted` proximity event →
  Glimpsed, `CreatureDefeated` → Known, `CreatureTamed` → Mastered; a new `AbilityCast` event from
  `PlayerCombatController` → the matching Attacks entry + a glimpse of that element's base bloodline; carried
  lines/sub-arts/Confluence glimpsed on open), and save persistence folded into `PlayerInventory` (`SaveData`
  `grimoireKeys`/`grimoireTiers`). Remaining: bloodline **Mastered** via meeting a notable bearer; a shared VR
  opener (see `GRIMOIRE.md` / `VR_INPUT_MAP.md`).
- **Owner admin + signature hero (Ash Shadowthorn)**: `SignatureCharacter` (Core) defines the owner's hero — a
  three-element Channeler (Air/Water/Fire with Flight/Sanguine Grip/Magmacraft), a `DragonForm`, teal eyes /
  reddish-brown hair / Texan accent, and three named companions: **Apollo** (kitsune), **Artemis** (shadowhound,
  shadow-teleport via blink), **Iago** (iridescent phoenix that mirrors the hero's elements and is reborn once).
  `Social.AdminAccounts` treats any Gmail alias of the owner's address as a global `UserRole.Admin` (dots and
  +suffixes normalised), and can provision the owner into a `UserDirectory`. Unit-tested.
- **Weapons / stones / wands (Core stub)**: `Armory` (Core) — elemental **stones** (dropped by defeated
  Channelers and in each capitol throne room), irreversible **two-at-a-time fusion**, a capitol **weaponsmith**
  that either **imbues** a weapon (+damage/+durability + an element effect; fused imbues harder) or **forges a
  wand**, and **wands** that grant elemental spells, block other item slots, and cap at 6 spells (3 per element).
  Pure rules + tests; in-world wiring (loot drops, throne-room stones, smith NPC, equip enforcement) is staged in
  `WEAPONS.md`.
- **In-App Purchasing** (`com.unity.purchasing` 5.1.1 — the version Unity ships for 6000.5) added to the package
  manifest so real-money purchasing is available for future monetization. Not yet wired into any game system; the
  economy still runs entirely on soft currency (Silver) + gacha. IAP v5 is a breaking API change from v4, so any
  previously-imported 4.15.x sample scripts must be removed (or re-imported for v5).
- **Ability ladder** (`AbilityUnlocks`, Core): the combat kit unlocks with player level — Primary + Defend from
  the start, Sweep at 2, Heavy at 4, and the Secondary/Signature casts at 6. The combat controller gates each
  intent (a locked intent no-ops with a toast); Channelers and weapon users share one intent-keyed table, so the
  ladder applies to both kits on the same levels. A level-up announces new unlocks. Gating is skipped where no
  `ProgressionController` is present (e.g. the Arena). All numbers live in one table; unit-tested
  (`AbilityUnlocksTests`, +1 → 56 EditMode test files).
- **Per-element Sweep arc**: Sweep is now a wide, multi-target arc (`OutcomeKind.Sweep`) instead of a single
  projectile — every enemy in a 120° / 3.5 m fan in front is hit at once. Each element carries a distinct rider:
  Fire burns, Water shoves + slows, Earth shoves + briefly staggers (control), Air is pure max-knockback
  displacement. The cone math is the pure `SweepArc`; `SweepController` (added to both rigs in the bootstrap) is
  the presentation shell that overlaps, filters to the cone, dedupes, and hits everyone caught. Unit-tested
  (`SweepArcTests`, `SweepMovesetTests`, +2 → 58 EditMode test files). Documented in `VR_COMBAT.md`.
- **Per-element Heavy impact**: Heavy is now a committed **impact zone at range** (`OutcomeKind.Heavy`) — it
  lands 3 m in front of the caster and hits everything within a 2 m blast, knocking targets outward. A distinct
  third shape alongside Sweep (wide/near) and Primary (single travelling shot). Riders preserved per element:
  Fire meteor (burn on Magmacraft), Water geyser (slow), Earth ground slam (metal on Oreshaping), Air updraft
  (knock-up). Blast math is the pure `HeavyStrike`; `HeavyController` is the presentation shell. Unit-tested
  (`HeavyStrikeTests`, `HeavyMovesetTests`, +2 → 60 EditMode test files). Documented in `VR_COMBAT.md`.
- **Sweep/Heavy VFX + Heavy telegraph + charge scaling**:
  - **Sweep** now throws a fast forward fan of particles on cast (`AbilityFx.SpawnSweepFan`); still instant.
  - **Heavy** is now **telegraphed** — it marks a ground ring at a fixed impact point, waits
    `HeavyStrike.TelegraphSeconds` (0.5 s), then drops the blast, so it's dodgeable (step out of the ring and
    you're spared). The hit is resolved *after* the wind-up.
  - **Charge scales Heavy's blast**: holding the cast grows both the damage (already) and the impact radius
    (`HeavyStrike.RadiusForCharge`), and the telegraph ring grows to match. `AbilityOutcome` now carries an
    optional `Charge` (source-compatible; preserved through `Scaled`).
  - New `AbilityPalette` (Core) gives the shared colour language — element primary, sub-art as the
    secondary/accent — and `AbilityFx` (Game) builds the procedural, self-cleaning fan / ring / impact burst
    (no prefab assets needed; appearance is a placeholder pending a hand-made VFX pass).
  - +1 test (`AbilityPaletteTests`) and charge/telegraph cases added to the Heavy tests → 61 EditMode test files.
    Documented in `VR_COMBAT.md`.
- **Heavy now arcs in with a filling telegraph ring** (replaces land-after-delay): on cast the strike launches on
  a parabolic arc (`HeavyStrike.ArcPoint`) toward the fixed impact point while a ground ring fills like a clock
  over the flight, and the blast resolves on landing — still dodgeable, now legible. `AbilityFx` gained the
  filling ring (`SpawnTelegraphRing` + `SetRingFill`) and the travelling projectile (`SpawnHeavyTravel`); the arc
  path is unit-tested (`HeavyStrikeTests`).
- **`docs/VR_INPUT_MAP.md`** — an audit of VR vs desktop input. All combat + locomotion is bound in VR; the
  unmapped controls (Interact, the Quest/Inventory/Social/Character/Settings/Slots overlays, Element travel,
  Mount, Companion) are listed, along with a real binding **conflict** (right **A** drives both Dash and
  Recenter). Indexed in `INDEX.md`.
- **Dialogue & action coverage tests** (`DialogueAndActionCoverageTests`): every guide has a full spoken profile
  (greeting + service line + home/sidekick/appearance + valid element), every `NpcRole` is represented, every
  creature has a finding hint, and every quest is fully authored (title/summary/giver/objectives/reward). +1 →
  62 EditMode test files. (Enemy stats/actions were already covered by `EnemyArchetypesTests`.)
- **Placeholder/bug sweep**: no `TODO`/`FIXME`/`NotImplementedException`/empty-catch/stubbed logic anywhere; the
  only "placeholders" are intentional procedural meshes/VFX awaiting an art pass.

### Fixed
- **VR input conflict**: right controller **A** drove both Dash (combat) and Recenter (locomotion), so a press
  fired both. Recenter now lives on the **right thumbstick-click** (`primary2DAxisClick`) in `VrComfortLocomotion`,
  leaving A as Dash-only. `VR_INPUT_MAP.md` updated.
- Compile errors surfaced on import (Unity 6000.5.0f1):
  - `ScoreController` now exposes `EnsureInstance()` (mirrors `StaminaController`) — fixes `ArenaController`.
  - `IceTrap` imports `Elementborn.Core`, resolving `StatusEffect` / `StatusKind`.
  - `FactionMember`'s element field initializer is namespace-qualified so it binds to the enum, not the
    same-named `Element` property.
  - `GuideNpcController` uses a struct-safe sentinel (`string.IsNullOrEmpty(_info.Name)`) instead of comparing
    the `GuideNpcInfo` value type to null.
  - `SwimLocomotion` qualifies the XR `CommonUsages`, resolving the InputSystem/XR ambiguity.
  - `UiTheme` fully-qualifies `UnityEngine.UI.Slider` so the type isn't shadowed by the `Slider(...)` factory.
  - `UnderwaterTests` adds `using UnityEngine;` for `Vector3`.
  - `HealthTests` passes a concrete `Element` (neutral `Earth`) to `DamageInfo` instead of `null` — the
    element parameter is a non-nullable enum.
- `QuestController.Start(string)` → `StartQuest`: on a MonoBehaviour, `Start` is a reserved Unity lifecycle name
  and can't take parameters (Unity logged "Start() can not take parameters"). Caller in `DialogueController`
  updated; the pure `QuestLog.Start` is unaffected.
- Removed `com.unity.modules.vr` from the manifest: the legacy built-in VR module is deprecated and unavailable
  in Unity 6000.5 ("Package [com.unity.modules.vr@1.0.0] cannot be found"). The project's VR uses `UnityEngine.XR`
  (XRNode / InputDevices / CommonUsages) from `com.unity.modules.xr`, not legacy VR, so nothing depends on it. The
  doctor's XR-module invariant now requires only `com.unity.modules.xr`.
- **Combat presentation wiring**: the generated player rigs were missing every `OutcomeReady` listener except
  the one just added for Sweep, so most casts resolved but presented nothing. The bootstrap now adds the full
  set to both the flat and VR rigs — `AbilityVfxBinder` (Projectile: procedural visuals + audio + a damaging
  projectile), `MeleeController` (Melee), `SweepController` (Sweep), `HeavyController` (Heavy),
  `BarrierResponder` (Defend shield), `DashResponder` (Dash/Flight), `SanguineGripController` (Control). Each
  self-wires, so no manual references are needed; visuals are procedural placeholders pending an art pass.

### Changed
- **2D art reproducibility (`make_ui_sprites.py`)**: the flat UI sprite set in `Assets/Elementborn/Art/UI/` now has
  a generator (parity with `make_sfx.py` / `make_glyphs.py`), regenerating panels, button states, HUD frames, gems,
  and map bits to the `UI_SPRITES.md` spec so the 2D look is restyleable in code. `UiTheme` already 9-slices these
  once they're imported as Sprites under `Resources/ElementbornUI/` (a one-time in-editor step). The visual art
  pass proper — wiring those sprites in-editor, plus 3D meshes and VFX in Blender — stays an editor/Blender task.
- **Audio pass completed**: the controller, the 17 synthesized clips (regenerable via `make_sfx.py`), the
  settings-driven volumes, and every trigger in `docs/AUDIO.md` are wired — and the systems added since (the map)
  now sound too: attuning a leyline rift plays `UiConfirm`, a fast-travel warp plays `WhooshShort`.
- **Doctor: new `imports` invariant** (`tools/check-imports.sh`) — fails CI if any `using` directive references a
  namespace that doesn't exist (Elementborn namespaces must be declared; others must be a known external root).
  Catches a class of break that previously only surfaced in a Unity compile. The tree currently passes clean.
- **Package upgrades** (Unity 6000.5): Input System 1.11.2 → 1.19.0, XR Interaction Toolkit 3.0.7 → 3.5.1,
  XR Management 4.5.0 → 4.5.4, OpenXR 1.13.0 → 1.17.1.
- IP guard hardened: `ip-guard.sh` and the doctor's standing grep now catch the bare verb "bend"/"bends",
  closing the gap that let Channel-convention stragglers slip past. Fixed three: a player-facing
  "Bend an Element" button (→ "Channel an Element") and two doc comments.

## [0.2.0] - 2026-06-23

### Added
- **Online social layer** (backend-agnostic): identity + roles, notifications, feedback-to-admin, friends,
  session invites, direct/session messaging, and role-enforced moderation — all behind seven Core seams with an
  in-memory `LocalSocialBackend`, so it runs and unit-tests offline. `SocialHub` is the single access point.
- **Social UI** (`SocialMenuController`): a toggled in-game overlay (key J) for notifications, friends + invites,
  session chat, a feedback form, and a moderation panel, plus a reusable `UiTheme.Input` text field. Now added to
  the generated bootstrap scene.
- **Interaction arbiter** (`InteractionArbiter` + `IInteractable`): one owner of the Interact button and prompt;
  the five former pollers (world interactor, NPCs, sidekicks, plant control, the frogs) became offerers, ending
  the shared-button conflict.
- **Editor bootstrap generator**: one-click build of a playable scene + first-person / third-person / VR rig
  prefabs (Elementborn ▸ Bootstrap).
- **Material generator**: cel-shaded `.mat` assets from the toon shaders (Elementborn ▸ Materials), auto-applied
  to the generated rigs and ground, with a `SceneStyleController` for sky/ambient/fog in the scene.
- **Netcode (Nakama)**: client adapters for all seven seams behind the `ELEMENTBORN_NAKAMA` define, a server
  runtime module (feedback-to-admin, trusted notify, authoritative sessions, and a ban-gated match join), a local
  `docker-compose`, and the build/load setup.
- **VR comfort locomotion** (`VrComfortLocomotion`): snap/smooth turn, stick movement, recenter and height reset,
  driving the existing comfort vignette; the generated VR rig is now an XR-origin shape.
- **Project doctor** (`tools/doctor.sh`): one command asserting every invariant — gates, IP, `#if` balance,
  asmdef validity, the Element enum, retired tokens, and doc/test-count sync.
- Release pipeline: the `VERSION` file, this changelog, `tools/bump-version.sh`, `tools/validate.sh`, and the
  `release` and `docs` GitHub Actions workflows; `tools/ip-guard.sh` + `tools/validate.sh` wired into CI.
- Docs: `BOOTSTRAP.md`, `NETCODE.md`, `VR_SETUP.md`, `SOCIAL.md`, `LIMITATIONS.md`.

### Changed
- `SocialHub` builds its services from a swappable `ISocialBackend` (local by default, Nakama under the define).
- `Health.SetMax` resizes in place instead of replacing the object, so external Damaged/Died subscribers survive.
- Faction "Republicers" renamed to "Synodists"; a trademark sweep across shaders and docs; `Notification.MarkRead()`
  made public so backend stores can mark read.
- EditMode test files: 44 → 50 (social phases, the backend swap, the arbiter, cross-service hardening).

## [0.1.0] - 2026-06-22
### Added
- Initial cross-platform scaffold from one Unity 6 (URP) codebase: VR / flat / third-person elemental combat,
  character creation with a gacha roll, and the cel-shaded look.
- A seeded open world — regions, biomes, low-poly terrain, and POI-driven structures.
- Bestiary and rare companions, mounts and vehicles, economy / taming, weather and day-night, the HUD,
  interaction prompts, and slot-aware saving.
- The arena and underwater layers, VR motion combat with a stance layer, and four hidden signature moves.
- Joinable factions, interactive plants, exotic apex creatures with two-element mix attacks, the three guide
  NPCs with Willow's sidekick menagerie, the evolution mode, and data-driven modding.
- 44 EditMode test files plus PlayMode tests, a GameCI workflow, the cel-shaded shader suite, and the full
  documentation set under `docs/`.
