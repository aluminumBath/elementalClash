# Creatures (underwater)

**Status:** the underwater creatures are in — decorative fish, the octopus ambusher, and the water-serpent
boss — built on the existing enemy/status/underwater systems. Their **behaviour** is here; the **organic
models** (and coral) are placeholders for an artist to replace (see boundaries).

## Decorative fish

`FishWanderer` drifts with gentle randomised heading changes (via the pure, tested `AmbientWander`), stays in
a home radius, keeps mostly horizontal, and darts away when the player gets close. `FishSpawner` scatters a
small school around itself (inside a `WaterVolume`, say); assign a fish prefab for real art, or it spawns flat
placeholder cubes so the water still feels alive.

## Octopus

`OctopusController` lurks, closes on the player, and in range **grabs**: a little damage, a **Control** status
that interrupts casting and pins movement, and a forced hold-breath (`UnderwaterController.Suffocate`) so even
a water user starts to **drown** in its grip. It has its own `Damageable`, so you can kill it — and it can be
frozen or stunned itself.

Because of the grab, casting now respects being incapacitated: `PlayerCombatController` checks the player's
own `Damageable` and **drops any cast while stunned, controlled, or frozen** — so the octopus grip, the ice
trap, and lightning stuns all genuinely interrupt you.

## Water serpent (boss)

`SerpentBossController` is a unique boss that **guards something good**. It pursues to a preferred range and
fights with telegraphed **water** and **ice** (ice slows) attacks plus a close **tail swipe** you can dodge by
backing off during the windup. It hardens through phases as its health falls (the pure `BossPhases`): calmer
water pressure → water + slowing ice → a fast frenzy with quicker, harder hits. On death it **reveals its
guarded reward** (a GameObject that starts disabled), pays out premium currency, and awards a big score. It
reuses `Damageable`, so the player's slows/freezes/knockback affect it like anything else.

## Setup (editor)

1. Tag the player rig **"Player"** (already used elsewhere).
2. Put `OctopusController` / `SerpentBossController` on a placeholder mesh with a `Damageable` (and a
   `FactionMember` if you want faction reactions); give the boss a `rewardOnDefeat` object (starts inactive).
3. Drop a `FishSpawner` in a water area for ambient life. Tune ranges/damage/phase numbers in the inspector.

## Wildlife by habitat

Beyond the bestiary, these roam their biomes (peaceful until provoked, via the existing `CreatureController`
and `Wildlife` biome rolls): **eels** underwater, **crabs** on beaches, **monkeys** and **tigers** in forests,
**crocodiles** and **snakes** in swamps/marshes, **rocs** over mountains, **thunderbirds** in the cloud
reaches, and **rhinos** and **tigers** on the plains. Eels swim, rocs and thunderbirds fly, the rest walk; the
big ones (roc, rhino) can be ridden once tamed. Each has its own combat stats (`CreatureCombat`). The
predators (crocodile, snake, tiger, rhino) currently use the same fight-back-when-attacked behaviour — making
them actively hunt is a small follow-up flag on `CreatureController`.

## Exotic apex creatures

Eight rare, original creatures inhabit the toughest corners of the world. They're **tameable but stubborn**
(very low tame chance) and turn up only as uncommon-to-rare spawns in their habitats:

| Creature | Niche | Lives | Locomotion |
| --- | --- | --- | --- |
| **Ridgewing** | cliff-soaring flying mount | mountains (rare) | flies |
| **Glidewisp** | small forest flyer | forests (rare) | flies |
| **Skytyrant** | immense apex flyer | mountains (very rare) | flies |
| **Goldkoi** | gold-green aquatic glider | coasts/isles (rare) | swims |
| **Direstalker** | land apex predator | forests (very rare) | walks |
| **Skimfin** | fast aquatic skimmer mount | coasts/isles (rare) | swims |
| **Gillcloak** | mantled aquatic creature | marshes (rare) | swims |
| **Tidewarden** | colossal sentient sea-beast | coasts/isles (very rare) | swims |

These are original designs (named and statted from scratch), not tied to any existing IP.

## Two-element mix attacks

Each exotic above — plus our **rocs** and **thunderbirds** — has a signature attack that blends two elements
(`CreatureMixAttacks`, e.g. the Skytyrant's *Cinderstorm* = Fire+Air, the Gillcloak's *Scaldveil* = Water+Fire
steam, the Thunderbird's *Thunderhead* = Fire+Air lightning). `MixAttack.ToOutcome` resolves it into a normal
`AbilityOutcome`, and `CreatureMixAttackController` makes the beast unleash it at the player in range. Wiring
these into a full creature AI loop (telegraphs, phases) and letting a *rider* fire their mount's mix attack are
follow-ups; the data and a working strike are in.

## Boundaries

The behaviour is real; the **art is placeholder**. The organic models — fish, **coral** decoration, the
octopus, and the serpent — are exactly the detailed organic meshes I can't author here, so they ship as blocky
stand-ins (coral is just static art to drop in — no script needed). Real **water rendering** and the
**scene/prefab** wiring are yours, and the combat/spawn numbers want an on-device tuning pass.

## Testing

`AmbientWander` and `BossPhases` are unit-tested. The fish drift, the octopus grab, and the boss phase/attack
loop are runtime behaviour.
