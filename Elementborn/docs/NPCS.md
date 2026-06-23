# Guide NPCs

Three named guides help players, each data-driven (`NpcCatalog`) so more can be added later. Stand near one and
press Interact to talk (`GuideNpcController`); what they say depends on their role.

## The three

| NPC | Home | Element | Sidekick | Helps with |
| --- | --- | --- | --- | --- |
| **Willow M. Hyst** | a cabin by Mistmere Lake, beneath the floating peaks | Water | **Gunnar** (a rideable rock-channeling direwolf) + a menagerie | finding creatures — where they live and how to reach them |
| **Kiana Eclair** | a palace in the coastal capital **Tideholt** | Water + blood (Sanguine Grip) | **Crickets**, a massive blue fire salamander | taming, keeping, and training creatures — and enforcing their welfare |
| **Parfa Itchonga** | the forge-town **Cinderhold**, by the volcano | Fire + lava (Magmacraft) | **two bickering frogs** (air & water) that orbit his head | locating items and people; he also buys surplus goods |

Appearances are baked into the data (Willow's hair shifts blue/red/pink, Kiana has white hair and black brows,
Parfa's hair is green flecked with red).

## Willow — companions & hints

Willow rides **Gunnar**, a rock-channeling direwolf, and keeps an odd menagerie: a **parrot** that paints itself
black at 2:34am and swears it's a raven, a land-dwelling **blobfish**, a hypnotic anthropomorphic **mushroom**,
and a **chameleon** in the plant pot by her window. Each has its own food (`WillowSidekicks`). Feed every one of
them within a couple of days (`SidekickFeedingTracker`, wired through `SidekickFeedPoint` + `SidekickFeedingController`)
and she rewards the care with a **hint toward a hidden ability** (one of the signature moves).

## Willow — creature hints

`CreatureHints.WhereToFind(kind)` composes a habitat, how to reach it given how the creature moves (fly / dive /
on foot), and how rare or stubborn it is to tame. Talking to Willow surfaces one for a notable creature; the
helper is pure and reusable, so a proper "ask about X" UI can call it directly.

## Kiana — keeping, and her law

Kiana advises on care, and enforces it. `CreatureCareController` (on the player) holds a `CareTracker` score:
feeding a creature a **Heartfruit** raises it automatically, and `MistreatCreature()` lowers it. If it falls to
**Abused**, Kiana **confiscates** one of the player's creatures (`PlayerInventory.RemoveOwned` — it must be
tamed again from scratch) and **bars them from the capital for a day** (`IsBanned`). She blood-channels it away,
in keeping with her Sanguine Grip.

## Parfa — locating & buying

Parfa points you toward items and people, and buys surplus goods. The advice surfaces on talk; the buying plugs
into the currency wallet. His **two frogs** (air & water) bicker constantly — show up embodying *both* elements
(a loadout with Air and Water) and press Interact near them and they finally agree, earning you a **diamond**,
once (`ParfaFrogController` / `FrogAccord`).

## Where it lives

- **Core:** `Npcs.cs` (`GuideNpcId`, `NpcRole`, `NpcCatalog`), `CreatureHints.cs`, `CreatureCare.cs`.
- **Game:** `GuideNpcController` (talk + role service), `CreatureCareController` (Kiana's enforcement),
  `NpcSidekickOrbiter` (the frogs / hovering companions). `PlayerInventory` gained `RemoveOwned`.

## Boundaries

The guides talk and Kiana's enforcement (confiscate + ban + re-tame) is fully wired and tested; the **dialogue
surfaces via an event + log**, so a real dialogue/quest UI is a follow-up. The **vendor (Willow) and buying
(Parfa)** are surfaced as services and hook into the wallet, but a full shop UI/economy is later. The concrete
*abuse* triggers (striking your own creature, prolonged neglect) call `MistreatCreature()`; feeding is the one
wired so far. Location-gating Kiana's reach strictly to her city is a refinement. Sidekicks are flavor on
placeholder meshes — the real critters are an artist's job.

## Testing

`NpcCatalog` (every guide has a profile), `CreatureHints` (habitat/approach/rarity), and the `CareTracker`
verdict thresholds are unit-tested.
