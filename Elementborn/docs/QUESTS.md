# Quests — the NPC → dialogue → quest → reward loop

A small, complete quest loop runs on the existing systems. Talk to a guide NPC, accept a quest, advance its
objectives by playing, and turn it in for a reward.

## How it flows

1. **Talk.** Walking up to Willow, Kiana, or Parfa and pressing Interact opens a **dialogue panel**
   (`DialogueController`). The panel shows the NPC's line and quest-aware choices.
2. **Accept.** If the NPC has a quest you haven't started, the panel shows its summary and objectives with an
   **Accept** button (`QuestController.Start`).
3. **Progress.** Objectives advance from real play. The gameplay raises `QuestEvents` at its success points —
   defeating a creature, taming one, gaining currency, talking to an NPC — and `QuestController` forwards them
   into the `QuestLog`, which advances any matching active objective.
4. **Turn in.** Once every objective is met the quest flips to *ready*; the next time you talk to the giver the
   panel shows a **Turn in** button that grants the reward — currency via `PlayerInventory`, plus **Sigils** for
   the Summon Beacon via `SummonController` when the quest offers them — and toasts the result.

Press **L** for the **quest log** (`QuestLogController`) — a live list of tracked quests with per-objective
progress and a "ready to turn in" marker.

## The starter quests

| Quest | Giver | Objectives | Reward |
| --- | --- | --- | --- |
| A Wild Start | Willow | Defeat 3 wild creatures | 50 Silver + 120 Sigils |
| A Gentle Hand | Kiana | Tame any creature | 1 Ruby + 200 Sigils |
| Word to Kiana | Parfa | Talk to Kiana · gather 30 Silver | 1 Emerald |
| Pelts for the Tanner | Willow | Gather 3 hides | 1 Ruby + 160 Sigils |

## Onboarding quests

A short, **chained** line teaching the core procedures — one quest per basic action, each satisfied just by
doing it once. Each unlocks the next when turned in (a quest is only offered once its prerequisites are
*Completed*):

| Quest | Giver | Teaches | Unlocks after | Reward |
| --- | --- | --- | --- | --- |
| First Channeling | Willow | Channel your element | — | 50 Silver |
| Answer the Beacon | Willow | Summon a creature (press U) | First Channeling | 40 Silver + 80 Sigils |
| Claim the Featured | Willow | Claim a featured creature with Motes | Answer the Beacon | 50 Silver + 100 Sigils |
| First Craft | Parfa | Craft any item (press B) | — | 60 Silver |
| Gear Up | Parfa | Equip a piece of gear (press V) | First Craft | 60 Silver |

So Willow walks you channel → summon → spark, and Parfa walks you craft → equip.

These advance off `ObjectiveKind`s — `SummonCreature`, `CastAbility`, `EquipItem`, `CraftItem`, `ClaimFeatured` —
wired to the matching gameplay events (`SummonPerformed` / the existing `AbilityCast` / `ItemEquipped` /
`ItemCrafted` / `FeaturedClaimed`).

## The pieces

- **Core (pure, unit-tested):** `Quests.cs` — `QuestObjective` / `QuestDefinition` / `QuestState` / `QuestReward`
  and `QuestLog` (start, record events, complete, turn in). `QuestEvents.cs` — the static event bus.
  `QuestCatalog.cs` — the authored quests. `QuestLogTests` covers start/progress/targeting/multi-objective and
  turn-in-once.
- **Game:** `QuestController` (owns the log, forwards events, grants rewards), `DialogueController` and
  `QuestLogController` (the panels, built through `UiTheme` / `OverlayUi`).

## Adding a quest

Add a `QuestDefinition` to `QuestCatalog` with a `GiverNpcId`, objectives, and a reward. New objective kinds go in
`ObjectiveKind` plus a matching `QuestEvents` raise at the gameplay success point and a forward in
`QuestController`. To **chain** quests, pass a list of prerequisite quest ids as the last argument: the quest is
only offered (and only startable) once every prerequisite is *Completed* (`QuestLog.PrerequisitesMet`, enforced in
`AvailableFrom` and `Start`). No UI changes needed — the dialogue and log read the catalog.

## What's next for the loop

Objectives cover defeat / tame / currency / talk / collect-item, plus the procedure verbs summon / channel /
equip / craft / claim-featured. Quests can now **chain** via prerequisites (quest B unlocks after A). Natural
extensions: reach-region objectives, branching chains. Quest status and progress persist through the save system.
