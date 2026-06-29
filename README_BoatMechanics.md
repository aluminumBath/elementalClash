# Elementborn boat mechanics patch

Copy the `Elementborn/Assets/Elementborn/Game/Boats` folder into the same path in your Unity project.

## Fast setup in Unity

1. Create a boat GameObject. A cube is fine for now.
2. Add a `Rigidbody`.
3. Add `BoatController`.
4. Add `BoatRangedCombat`.
5. Add `BoatCreatureEncounterDirector` if you have a water creature prefab.
6. Add a child empty named `PilotSeat` and assign it on `BoatController`.
7. Add a child trigger collider near the boat, add `BoatBoardingStation`, and assign the boat.
8. Add a `WorldWindController` to any scene object.
9. Press Play, walk into the boat trigger, press Interact/E to board.

## Controls

- WASD / left stick: steer and paddle.
- E / Interact: board or leave through `BoatBoardingStation`.
- Q / MountSkill: raise/lower sail.
- Space / Jump: small-boat jump when the sail is down and the boat is on the water.
- Left click / PrimaryCast: boat arrow shot.
- Right click / SecondaryCast: ranged elemental shot.

## Design behavior

- Sail raised: the boat accelerates most when its forward direction aligns with the wind direction.
- Headwind slows the boat.
- Sail lowered: the boat can slowly move forward/backward in the direction it faces and naturally comes to a stop.
- Wind does not affect flying creatures globally. Only scripts that explicitly read `WorldWindController` respond to it.
- Rare boat encounters only roll while a pilot is aboard.

## Notes

This is intentionally additive. It does not modify InputBindings, PlayerCombatController, scenes, prefabs, or asmdefs. It should compile inside the existing `Elementborn.Game` assembly.
