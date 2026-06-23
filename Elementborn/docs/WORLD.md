# The world — generation, terrain & population

The open world is data-driven: a `WorldMap` of regions and points of interest, sculpted into terrain and filled
with life. The bootstrap scene wires the whole chain so it runs on Play.

## The chain

1. **Generate.** `GameFlowController` builds a `WorldMap` from a seed (`worldSeed`, `regionCount` — 14 by
   default): regions with a biome, danger level, radius, and POIs (cities, villages, markets, shrines, camps,
   weapon caches…).
2. **Pick a region.** After character creation, the world map (`WorldMapView`) lets you choose where to enter.
3. **Sculpt.** On entering, `MeshTerrainBuilder` builds the terrain mesh from the world data (biome-blended
   per-vertex colours, with a `MeshCollider`).
4. **Populate.** `WorldSpawnPlacer.Place(World)` scatters content across the regions and POIs:
   - **creatures** per region (wild, tameable — `Wildlife.Pick` by biome) and a chance of a rare companion,
   - **enemies** at each POI's suggested count (`EnemyComposition.Pick` by biome + danger),
   - **civilians** in towns,
   - **weapon caches** and **lures** at the POIs that have them.
   Spawns snap to the ground via `TerrainHeight.Sample`, which reads the mesh terrain — so everything sits on the
   sculpted surface.

## What the bootstrap wires

`Elementborn ▸ Bootstrap ▸ Build Playable Scene` builds three primitive entity prefabs — **WildCreature**
(`CreatureController` + `Tameable`), **Enemy** (`EnemyController`, with its AI/combat), and **Civilian**
(`FactionMember`) — assigns them to a `WorldSpawnPlacer`, and points `GameFlowController.spawnPlacer` at it. So
entering the world fills it with roaming wildlife, hostile enemies, and townsfolk, on top of the demo plaza near
spawn (which holds the quest-giving guide NPCs and a merchant).

## Tuning

On `WorldSpawnPlacer`: `creaturesPerRegion`, `civiliansPerTown`, `companionSpawnChance`, `mapToWorldScale`,
`snapToTerrain`. On `GameFlowController`: `worldSeed`, `regionCount`. Re-roll the seed for a different world.

## What's still placeholder

The entity meshes are capsules (real models replace them — the prefabs already carry the right components and
materials). Weapon-cache and lure pickups aren't wired into the generated scene yet (they need `WeaponPickup` /
`LurePickup` prefabs — a small follow-up). Guide NPCs are placed as fixed demo content rather than spawned by the
placer, since they anchor the quest loop.
