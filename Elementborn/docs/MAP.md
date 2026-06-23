# Map, minimap & fast travel

Runtime navigation on top of the procedural `WorldMap`: a minimap, a full map viewer, leyline-rift fast travel,
and locating yourself and (consenting) friends. The rules live in `MapNavigation.cs` (Core, pure, unit-tested in
`MapNavigationTests`); the HUD/overlay and world objects are the wiring step (below).

## Leyline rifts (fast travel)

A `LeylineRift` is a fast-travel node at a world position. The `FastTravelNetwork` registers every rift but only
lets you **warp to ones you've discovered** (stepped on / activated): `Discover(id)`, `CanTravelTo(id)`,
`Discovered()`, and `NearestDiscovered(pos)` for "warp to closest". Discovery is savable and never un-sets.

## Locating people

- **Yourself — always.** `Locator.Self(localId, worldPos)` returns your marker unconditionally.
- **Friends — only if they allow it.** `LocationSharing` is an explicit per-user opt-in that's **off by default**
  (privacy). `Locator.VisibleFriends(friendIds, sharing, positions)` returns markers only for friends who have
  opted in *and* whose position is known — so a friend who hasn't shared never appears.

Markers (`MapMarker` + `MapMarkerKind`: Self, Friend, LeylineRift, Checkpoint, City, Quest) are what the minimap
and viewer draw.

## Minimap & viewer math

`Minimap.WorldToNormalized(world, min, max)` maps a world XZ position into `[0,1]` map space (for placing a dot
on any sized map/minimap rect), and `Minimap.WithinRange(center, radius, world)` is the minimap's nearby-ring
filter (height-independent).

## Still to wire (UI + world)

The Core is complete and tested. What the in-world pass adds:

1. **Rift world objects** — place `LeylineRift` nodes in the world; a trigger calls `Discover` on first touch and
   offers the warp interaction (via `InteractionArbiter`); travelling moves the rig to the chosen rift.
2. **Minimap HUD** — a corner map that plots `Locator.Self` + discovered rifts + nearby POIs via
   `WorldToNormalized`/`WithinRange`, rotating with the player.
3. **Map viewer overlay** — a full-screen map (tabs/details, checkpoints, the rift network) with tap-to-fast-travel
   to discovered rifts. (Needs a VR opener — see `VR_INPUT_MAP.md`.)
4. **Friend positions** — feed live friend positions (from the social/Nakama layer) into `VisibleFriends`, and a
   settings toggle that drives `LocationSharing` for the local player.
5. **Persistence** — save `FastTravelNetwork.ToSave()` and the sharing opt-in with the rest of the profile.

## Note on geography

The playable world is **seed-generated** (see `WORLD.md`), so there's no single fixed layout. An illustrated
overworld/atlas image (key art) can back the map viewer and define a canonical rift network; if we ever want a
fixed hand-authored world, that image is the place to start.
