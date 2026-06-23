# Plants & plant control

Interactive flora, most of it the domain of **plant users** — Channelers with the **Verdancy** specialty (the
Water + Earth combination from the evolution mode). `PlantControl.IsPlantUser(loadout)` is the single pure check
everything uses.

## The flora

| Plant | What it does | Who controls it |
| --- | --- | --- |
| **Maw Snaptrap** | Rooted carnivore — grabs (Control) and bites anyone who strays in range. Killable. | acts on its own |
| **Grasping Vine** | Trips non-plant-users who blunder through (stumble + slow); can be climbed (hold the climb/Dash input). A plant user can lash it to seize and hold nearby foes. | plant user (lash) / anyone (climb) |
| **Hazecap Spores** | A plant user puffs them to disorient foes in a radius; the plant user is immune. A non-plant-user who brushes the cap sets it off on themselves. | plant user |
| **Gleamlily** | Rare gold-and-pink sparkly bloom; a plant user harvests a healing fruit from it (long cooldown). | plant user |
| **Heartfruit** | The lily's fruit — silver skin, blood-red flesh. Heals and cures whoever (person or creature) it's used on. | anyone benefits |
| **Venom Lily** | A near-identical look-alike of the Gleamlily, but purple-sparkled and **poisonous to touch** (contact damage + a sickly slow). No fruit — a trap for the careless. | hazard |
| **Willow Gate** | A weeping-willow curtain that parts only for a plant user nearby and closes behind them. The marshes are full of them. | plant user |

## How control works

- **Plant user** = `loadout.HasSubArt(SubArt.Verdancy)`. They walk through vines without tripping, open willow
  gates, and shrug off spore haze.
- **Steam/healer** = `loadout.HasSubArt(SubArt.Steamcraft)` (the Water+Fire specialty). They can also make a
  Gleamlily bloom and take its Heartfruit (`PlantControl.CanTendLily`), but the vines, spores, and gates stay
  plant-user business. The Venom Lily is poisonous to everyone who touches it.
- **`PlantControlController`** on the player turns the Interact press into commands when you're a plant user:
  nearby vines lash (`Snare`), spore caps puff (`Puff`), and lilies harvest (`Harvest`).
- Trip/grab/hold reuse the existing **Stun/Slow/Control** statuses; disorient reuses **Control** (a brief loss
  of control) for now. The Heartfruit heals via `Health.Heal` and cures via the new `StatusController.Clear`.

## Where it lives

- **Core:** `Plants.cs` — `PlantKind`, `PlantCatalog`, the `PlantControl` gate, and `PlantTuning`. `SubArt`
  gained `Verdancy`.
- **Game:** `PlantFlytrap` (Combat), and in World: `VinePatch`, `MushroomCluster`, `GleamLily`, `Heartfruit`,
  `WillowGate`; plus `PlantControlController` on the player.

## Boundaries

The plants drive their gameplay (grab, trip, disorient, gate, heal); the **meshes are placeholders** (the
Heartfruit is a tinted sphere) — organic plant models are an artist's job. Climbing is a simple ascend assist,
not full hand-over-hand / VR climbing (a locomotion follow-up). "Moving" vines bodily around the world (vs.
lashing them in place) and a deliberate "use Heartfruit on a chosen creature" inventory flow are future hooks
(`Heartfruit.ApplyTo` is already there for the latter). The Verdancy specialty itself is granted by the
**evolution game mode** (queued); until then a loadout carries it as data.

## Testing

`PlantControl` (who's a plant user, who opens gates / resists spores), `PlantCatalog` flags, and the new
`StatusController.Clear` cure are unit-tested.
