# Elementborn — Story Bible

The canonical campaign. Lore lives in code as pure data — `Core/StoryLore.cs` (world + Kings + diplomat),
`Core/StoryArc.cs` (chapters + endings), and the story factions in `Core/Allegiances.cs` — so dialogue, codex,
and quests can all read one source of truth. This document is the human-readable companion.

## The world

Once the world was whole and the four elements ran as a single current — the **First Convergence**. The
**Sundering** broke that current and split the world into four elemental realms ringed around one neutral
city, **Concord**. Most believe the Sundering merely happened. It did not.

The realms do not run on faith or fortune. They run on the **four Creature Kings** — primordial titans bound
long ago and bled of their power to light the hearths, turn the tides, raise the stones, and stir the winds.
This is the **Great Betrayal**. The prisons are now failing: borders weaken, **hybrids and half-elements**
appear (the **Skyotter** is one such water/air storm-hybrid), and the stolen engine everyone depends on is
grinding toward collapse.

## The four Creature Kings  (`StoryLore.Kings`)

| King | Title | Element | Beast (CreatureKind) | Prison |
| --- | --- | --- | --- | --- |
| **Ignivar** | the Phoenix Emperor | Fire | `Phoenix` | the Emberheart, beneath the volcano's root |
| **Thalassa** | the Leviathan Queen | Water | `Tidewarden` | the Drowned Vault, below the tideless deep |
| **Terragor** | the World-Crown | Earth | `Rhino` | the Root Hollow, within the mountain's keel |
| **Zephyreon** | the Storm Roc | Air | `Roc` | the Hushed Eyrie, above the cloud reaches |

Per your call, **Terragor is the crowned Rhino** (`Boulder_Rhino`) — a crowned mountain given breath — rather
than a turtle. (Turtle models exist in the asset pool if that's ever reconsidered.)

## Factions  (`Core/Allegiances.cs`)

We keep the in-world faction names and add the two the storyline needs. The existing four remain the
**joinable** factions; the two new ones are **story factions** (not in the standard join list):

- **Symbiasts** — convergence in one soul is holy.  *(in-world face of the **Convergence Movement**)*
- **Separatists** — each kept apart by element.  *(in-world face of the **Purity League**)*
- **Cleicists** — coexist, but any convergence is an abomination.
- **Synodists** — rule by population and synod.
- **The Architects** *(new, hidden)* — engineered the Sundering and keep the Kings caged to hold the realms in
  order; cannot be seen to act, and crumble once exposed.
- **The Awakening** *(new, cult)* — the Kings are gods in chains; every hybrid is a sign, and their waking will
  remake the world.

## The campaign spine  (`StoryArc.Beats`)

A linear spine for now; full branching layers on later at the Reckoning.

1. **Arrival in Concord** — the neutral city alive with all four realms; you arrive as Ambassador Calderon
   prepares to seal a new accord.
2. **The Tower Blast** — the **Convergence Tower** erupts; **Ambassador Sera Calderon, Voice of Concord**
   (our beloved peace-broker) is killed and the accord turns to ash. *(inciting incident)*
3. **Ashes and Accusations** — chase the blast through a city turning on itself, courted and lied to by each
   faction.
4. **What the Realms Run On** — the truth: the imprisoned Kings, the Betrayal, the failing prisons, and the
   hand that arranged it all.
5. **The Fracturing** — borders buckle, hybrids spill through, and the factions go to war.
6. **The Reckoning** — at the failing heart of the world, choose the fate of the Kings and everyone who lives
   on their stolen power.

## The four endings  (`StoryArc.Endings`)

- **Restoration** — free the Kings; the First Convergence reborn.
- **Dominion** — seize the Kings' power and rule the reunited realms.
- **Human Supremacy** — forge new chains; people, not titans, command the elements.
- **Shared World** — refuse both cage and crown; broker a true peace between people and Kings.

## Built vs. next

**Built so far:** the lore (`StoryLore`), the spine and endings (`StoryArc`), the two story factions, the
**Skyotter** hybrid creature, the diplomat (named, with a Meshy prompt in `docs/MESHY_PROMPTS.md`), and a
**`StoryController`** — the live cursor through the campaign (current chapter + chosen ending) that persists with
the save and raises `ChapterChanged`/`EndingChosen` for the UI. Scenes and events drive it via `Advance` /
`SetChapter` / `ChooseEnding`.

**Next:** wiring the later chapters to gameplay (and full ending branching), then the **vertical world + cave /
underground-water systems** — tall, hard-to-reach places (climb/glide/jump/fly/creatures/spells) holding bosses and
rarities; caves leading to underground lakes/rivers that surface at the ocean, waterfalls and ponds; and the hidden
temples, underwater palaces, and boss arenas they open up. Two **environmental hazards** ship with that pass:
**altitude cold** (the higher you climb, the more health you lose — exempt: Air/Fire channelers, or a Fire chest
enchant) and **underwater pressure** (the deeper you dive — exempt: Water/Earth channelers, or a Water/Earth chest
*and* Air/Water helmet enchant). Both read the channeler element + the chest/helmet enchants already in place.

**Concord is built:** the capital city is Concord — `WorldSpawnPlacer` plants the **Convergence Tower** landmark
and **Ambassador Calderon** there, and walking up to the tower during `Arrival` triggers the **tower blast** that
kills her and flips the story to `TheTowerBlast` (`ConcordSite`).
The storyline's "civilization runs on the Kings' bled power" also dovetails with the backlogged creature-blood
summoning spellbook and the Summon Beacon.
