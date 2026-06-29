# Elementborn Boat + Ocean Gameplay Patch

This patch expands the previous boat mechanics with:

- Wind-Waker-like boat sailing and paddle control
- small boat jump
- sail raise/lower
- wind that affects boats only, not flying creatures
- wave rocking while still or moving
- procedural wake particles while moving
- boat/player hit knockoff: player is launched into nearby water and starts swimming
- rare boat creature encounter spawner
- enemy death: turn to stone, crumble to dust, blow away, respawn at original location
  - typical enemies: 60 seconds
  - rare enemies: 10 minutes
- user respawn delay changed to 10 seconds
- NPC damage immunity component
- save-slot button on the main menu, using the existing SaveSystem/SaveSlotController
- displayed Parfa name changed to Kram while leaving the internal GuideNpcId.Parfa enum alone for save/test compatibility
- new Coral Reef Forest island biome: Neritha Reefwood

## Install

1. Close Unity.
2. Back up or commit your repo.
3. Copy the `Elementborn` folder from this patch over your repo's `Elementborn` folder.
4. Reopen Unity and let it compile.
5. Add the boat components to a test boat prefab or scene object.

## Important notes

The patch overwrites several existing files to add the new biome/menu/name changes. If you have local edits in those files, compare first.

Files overwritten:
- Assets/Elementborn/Game/Boats/BoatController.cs
- Assets/Elementborn/Game/Boats/BoatBoardingStation.cs
- Assets/Elementborn/Game/Boats/BoatCreatureEncounterDirector.cs
- Assets/Elementborn/Game/MainMenuController.cs
- Assets/Elementborn/Game/RespawnController.cs
- Assets/Elementborn/Game/ParfaFrogController.cs
- Assets/Elementborn/Game/QuestLogController.cs
- Assets/Elementborn/Core/Npcs.cs
- Assets/Elementborn/Core/QuestCatalog.cs
- Assets/Elementborn/Core/Bloodlines.cs
- Assets/Elementborn/Core/World/World.cs
- Assets/Elementborn/Core/World/WorldGen.cs
- Assets/Elementborn/Core/World/TerrainGen.cs
- Assets/Elementborn/Core/World/MeshTerrainGen.cs
- Assets/Elementborn/Core/Wildlife.cs
- Assets/Elementborn/Core/Companions.cs
- Assets/Elementborn/Core/Enemies.cs
- Assets/Elementborn/Core/Weather.cs
- Assets/Elementborn/Game/World/WorldMapView.cs

New files:
- Assets/Elementborn/Game/Boats/BoatWaveVisuals.cs
- Assets/Elementborn/Game/Boats/BoatWakeController.cs
- Assets/Elementborn/Game/Boats/BoatKnockoffOnHit.cs
- Assets/Elementborn/Game/Boats/BoatSwimEntry.cs
- Assets/Elementborn/Game/Combat/EnemyRespawnOnDeath.cs
- Assets/Elementborn/Game/Combat/NpcDamageImmunity.cs

## Quick scene test

Create a temporary scene with:
- Plane or cube as water visual at Y=0
- A large trigger box with WaterVolume, top at water surface
- TestBoat cube with Rigidbody, BoatController, BoatWaveVisuals, BoatWakeController, BoatRangedCombat, BoatKnockoffOnHit, Damageable
- Child PilotSeat
- Child BoardingTrigger with BoxCollider Is Trigger and BoatBoardingStation
- Empty WorldWind with WorldWindController
- Player capsule with CharacterController, Damageable, FirstPersonRig, PlayerCombatController, UnderwaterController, SwimLocomotion, tag Player

Controls:
- E: board / disembark
- Q or MountSkill: raise/lower sail
- WASD: steer / paddle
- Space: jump while sail down on small boat
- Left click: arrow shot
- Right click: elemental shot
