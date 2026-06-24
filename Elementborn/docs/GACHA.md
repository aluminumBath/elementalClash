# Summon Beacon — the gacha loop

The Beacon turns the game's creature roster into a repeatable, currency-gated summon: spend a premium currency
to roll companions and mounts into your collection, with pity so a long dry streak still pays out. It sits on
top of the existing creatures — everything you summon is immediately usable (a companion in combat, a mount to
ride), so it needs no new art.

> This is separate from the one-time character-creation roll (`GachaRoller`/`GachaConfig`), which decides your
> starting element/loadout. The Beacon is the ongoing collection loop.

## The two currencies

- **Sigils** — the pull currency. You start a fresh game with **1600** (one ten-pull), and earn more from
  **levelling up** (+60 per level) and from **quest rewards** (several starter quests grant Sigils — the in-world
  faucet beyond level-ups). Other systems can grant more with `SummonController.AddSigils`.
- **Motes** — duplicate dust. A pull that lands a creature you already own refunds Motes instead of being wasted,
  and Motes can claim a banner's featured creature outright (the "spark" floor).

Both live on `SummonController` and persist in the save; neither touches the gameplay `Wallet`.

## Banners

A **banner** is a summon pool of three rarity tiers, optionally with one rate-up ("featured") Legendary.

- **Wild Beacon** (`standard`) — no rate-up; every Legendary is equally likely. The "collect anything" pool.
- **Featured Beacon** (`featured`) — a single rate-up slot whose creature **rotates** (see [Rotation](#rotation)).
  At period 0 it's the **Flamecaller Beacon** (Phoenix rate-up). The rate-up works via the featured 50/50 +
  guarantee.

Both banners draw from the same three pools (assigned in `SummonBannerCatalog`, the single rarity knob):

- **★★★★★ Legendary** — Phoenix, Fire Dragon, Water Dragon, Skytyrant, Tidewarden, Direstalker
- **★★★★ Epic** — Storm Cat (Ice), Storm Squirrel, Roc, Thunderbird, Ridgewing, Goldkoi, Gillcloak, Skimfin
- **★★★ Rare** — Web Spider, Water Cat, Earth Hound, Earth Mole, Dragonfly, Jellyfish, Horse, Rhino, Glidewisp

## Rotation

The featured slot rotates through one themed beacon per Legendary, so the rate-up creature changes on a cycle:

| Period | Beacon | Featured |
| --- | --- | --- |
| 0 | Flamecaller | Phoenix |
| 1 | Emberwyrm | Fire Dragon |
| 2 | Tidewyrm | Water Dragon |
| 3 | Stormcrown | Skytyrant |
| 4 | Deepward | Tidewarden |
| 5 | Nightprowl | Direstalker |

- The **period** is whole `rotationPeriodDays`-day windows (default **7**) since a fixed UTC epoch, so every client
  shows the same featured banner at the same time. The math (`SummonBannerCatalog.PeriodFor`) takes the time as an
  argument — no hidden clock — so it's unit-tested; the controller passes `DateTime.UtcNow` (overridable in tests
  via `SetClock`).
- The slot's **id is stable** (`featured`), so your **pity and the lost-50/50 guarantee carry across rotations** —
  only the name and rate-up creature change. The panel shows a "rotates in N days" countdown.
- `SummonController.CurrentFeatured()` / `Banners` give the live banner; `SummonBannerCatalog.FeaturedForPeriod(p)`
  is the pure source of truth.

## Rates & rules

Tunable on `SummonConfig` (`SummonConfig.Default` ships these):

- **Cost** — a single pull is **160** Sigils; a ten-pull is **1600**.
- **Base rates** — Legendary **0.6%**, Epic **5.1%**, Rare the remainder.
- **Hard pity** — a Legendary is guaranteed by the **80th** pull since your last one; the pity counter resets on
  any Legendary (forced or lucky).
- **Featured 50/50** — on a featured banner, a Legendary is the featured creature half the time. **Lose** the
  50/50 and the *next* Legendary is a **guaranteed** featured.
- **Ten-pull floor** — a ten-pull always contains **at least one Epic-or-better**.
- **Duplicate refunds** — a duplicate refunds Motes by tier: **Rare 1 / Epic 5 / Legendary 25**.
- **Spark exchange** — spend **100 Motes** to claim the active banner's featured creature outright. (The
  *Claim the Featured* onboarding quest teaches this, and `ClaimFeatured` is a quest objective kind.)

Pity and the featured guarantee are tracked **per banner** (`SummonState`) and persist.

## Daily free summon

A once-per-day **free** pull on the standard Wild Beacon, on top of paid pulls. The button sits at the top of the
Beacon panel when available; otherwise it shows a countdown ("Next free summon in 3h 20m"). It refreshes at **UTC
midnight** (a calendar-day reset, not a 24h cooldown), so the rules are a tiny pure helper (`Core/DailySummon`,
unit-tested in `DailySummonTests`); the controller holds the last-claim timestamp and persists it.

The free pull runs the same resolution as a paid one — full rates, hard pity, duplicate refunds, history and
lifetime stats — just at zero Sigil cost, and it raises the same `SummonPerformed` quest hook, so a free pull can
satisfy the *Answer the Beacon* onboarding quest.

Claiming also advances a **login streak**: claim on the next UTC day and the streak grows; skip a day and it
resets to 1. Each claim pays escalating **Sigils** on a 7-day cycle (`40, 40, 60, 60, 80, 100, 200` — day 7 is a
milestone) plus a small loyalty bonus that grows `+10` per completed week, capped at `+100`. The panel shows the
streak day and the bonus you'd earn next. Rules are pure (`Core/LoginStreak`, unit-tested in `LoginStreakTests`);
the streak count persists.

## Code

- **`Summoning.cs`** (Core, pure) — `SummonRarity`, `SummonResult`, `SummonBanner`, `SummonConfig`, `SummonState`,
  and the seedable resolver `SummonRoller` (`Pull` / `PullMany`). No Unity, no I/O, no currency — it just rolls a
  tier (honouring hard pity) then a creature (honouring the featured 50/50). Unit-tested in `SummonRollerTests`
  and `SummonConfigTests`.
- **`SummonBannerCatalog.cs`** (Core, pure) — the banners, `All`, `ById`, `RarityOf`, and the **rotation**
  (`FeaturedForPeriod`, `FeaturedRotationLength`, and the pure `PeriodFor` date math). Tested in
  `SummonBannerCatalogTests` and `SummonRotationTests` (every pooled/featured creature is usable; tiers are
  disjoint; the rotation cycles through distinct Legendaries under one stable slot id).
- **`SummonController.cs`** (Game) — owns the Sigils/Motes balances and per-banner state, seeds the starter,
  tops up on level-up, spends + rolls, grants new creatures into `PlayerInventory` (so the companion/mount
  summoners can use them) or refunds Motes for duplicates, and persists via the usual `CaptureInto`/`RestoreFrom`.
- **`SummonViewer.cs`** (Game) — the overlay panel (see below): balances, banner selector, pity readout, ×1/×10
  pull buttons, the Motes claim button on a featured banner, the last-summon results, and a collection roster
  (owned vs. locked, grouped by tier).

## History

The panel shows a **summon history**: pulls on the current banner (`SummonState.TotalPulls`) plus a lifetime,
all-banner tally — total pulls, counts by tier, featured pulled, the observed Legendary rate, and lifetime Sigils
spent / Motes earned. It's a pure, unit-tested `SummonStats` (`SummonStatsTests`) that the controller feeds every
pull and persists in the save.

Below that, a **Recent pulls** log shows your last few notable summons (Epic or better) — tier, creature, the
banner it came from, whether it was a featured win, and how long ago ("2h ago"). It's a small newest-first ring
buffer (`SummonHistory`, capacity 8, unit-tested in `SummonHistoryTests`); the controller records each notable
pull with the time from its clock seam and persists the log.

## Sound

The Beacon has its own placeholder SFX (synthesized by `make_sfx.py`): a **cast whoosh** (`SummonPull`) when you
roll, then a **reveal sting** for the batch's best tier (`SummonRare` / `SummonEpic` / `SummonLegendary`, via the
pure `AudioController.SfxForSummon`). Claiming the featured creature with Motes plays the Legendary sting. See
`docs/AUDIO.md`.

## Opening it

- **Desktop:** press **U**. (`Esc` closes it.)
- **VR:** the **Summon Beacon** entry on the wrist hub (`Tab`).

The bootstrap scene (`Elementborn ▸ Bootstrap`) already adds a `Summon` object with both components.

## Persistence

`SummonController` round-trips through `SaveData`: `summonSigils`, `summonMotes`, `summonSeeded` (so the starter
grant is one-time), and parallel `summonBannerIds` / `summonPity` / `summonGuaranteed` / `summonTotalPulls` lists
for per-banner state. A loaded game arrives already seeded, so the starter grant is a no-op.
