# Guide NPCs

Four named guides help players, each data-driven (`NpcCatalog`) so more can be added later. Stand near one and
press Interact to talk (`GuideNpcController`); what they say depends on their role. Beyond the guides, the world
also has the **royal houses** (`RoyalCatalog` + `RoyalNpcController`) — see *Royalty* below.

## The four

| NPC | Home | Element | Sidekick | Helps with |
| --- | --- | --- | --- | --- |
| **Willow M. Hyst** | a cabin by Mistmere Lake, beneath the floating peaks | Water | **Gunnar** (a rideable rock-channeling direwolf) + a menagerie | finding creatures — where they live and how to reach them |
| **Kiana Eclair** | a palace in the coastal capital **Tideholt** | Water + blood (Sanguine Grip) | **Crickets**, a massive blue fire salamander | taming, keeping, and training creatures — and enforcing their welfare |
| **Parfa Itchonga** | the forge-town **Cinderhold**, by the volcano | Fire + lava (Magmacraft) | **two bickering frogs** (air & water) that orbit his head | locating items and people; he also buys surplus goods |
| **Deb** (the Sphinx) | the **Crab-Sign Creature Orphanage** in Neritha Reefwood | Air (winged sphinx) | a rotating pride of orphaned creatures "hers now" | guarding the orphanage — sassy, kind, kooky, fiercely protective |

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

## Deb — the orphanage sphinx

**Deb** is a great winged sphinx — lioness body, folded eagle wings, moon-pale eyes — who fiercely guards the
**Crab-Sign Creature Orphanage** in Neritha Reefwood. Sassy, funny, kind, and a little kooky, she treats every
lost or hurt creature as hers to protect and never forgets a face (or a grudge, or where she left her
third-favourite riddle). She's a `GuideNpcId.Deb` (role `CreatureKeeper`); drop a `GuideNpcController` with
`id = Deb` at the orphanage to place her (model falls back to the primitive until sphinx art is bound).

## Royalty — the Crown and the elemental houses

The elemental capitals have their own rulers and the mythic **Creature Kings** sit above the world, but the
**central human Crown** — the Neutral Central City's monarchy — was the seat the roster was missing. It lives in
`RoyalCatalog` (`Royal` enum + `RoyalInfo`); place any of them with a **`RoyalNpcController`** (Interact →
"Speak"), exactly like the guides. "Metal" maps to Oreshaping, "Steam" to Steamcraft, "Plant" to Verdancy.

| Royal | House / seat | Discipline | Ties |
| --- | --- | --- | --- |
| **King Ronald** | the Crown (Neutral Central City) | Earth / Metal (Oreshaping) | aging monarch; father of Jaadeb, Samara, and Ella; keeps a great **statue of Deb** in his throne room |
| **Queen Renee** | the Crown | Air | prim, proper, forever joining conversations not her own; her own children arrive later |
| **Jaemys Windwyrm** | House Windwyrm (Metal Capital) | Steam (Steamcraft) | husband of Samara; father of Conrad |
| **Samara Windwyrm** | House Windwyrm | Metal (Oreshaping) | Ronald's daughter; wife of Jaemys; mother of Conrad |
| **Conrad Windwyrm** | House Windwyrm | strong Steam + Fire | son of Jaemys & Samara; all promise, no patience |
| **Kelly Flowers** | House Flowers (near the Earth Capital) | Earth + slight Plant (Verdancy) | wife of Jaadeb; mother of JB |
| **Jaadeb Flowers** | House Flowers | Metal (Oreshaping) | Ronald's son; husband of Kelly; father of JB |
| **JB** | House Flowers | medium Plant + medium Metal | son of Kelly & Jaadeb; cousin to Conrad |
| **Ella** | the Crab-Sign Creature Orphanage (estranged from court) | Water | Ronald's daughter; **eloped with Eloc**; keeps the orphanage Deb guards |
| **Eloc** | the orphanage (married in by elopement) | Earth (Verdancy) | the gardener Ella ran off with; tends the orphanage greens |

The family loops together: Ronald's daughter **Ella** eloped with the commoner **Eloc** to run the creature
orphanage; **Deb the Sphinx** guards it; and Ronald keeps a statue of Deb in his throne room and gazes at it
fondly. Queen Renee's own children are a later addition.

## Testing

`NpcCatalog` (every guide has a profile), `CreatureHints` (habitat/approach/rarity), and the `CareTracker`
verdict thresholds are unit-tested. The portal fast-travel routing the capitals use is covered by
`PortalNetworkTests`.
