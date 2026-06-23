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

## The UI + world (built)

The runtime layer is in place, reading the canonical `WorldMap` (bounds ±250; seven rifts — the central
**Confluence Crystal** hub, the four elemental capitals, and two crossings) through `MapState`:

1. **Rift world objects** — `LeylineRiftSpawner` drops a floating cyan crystal for each `WorldMap` rift, snapped to
   the ground via `TerrainHeight`. Each carries a `LeylineRiftObject` (`IInteractable`): coming within range
   discovers it (a "Leyline attuned" toast), and standing close offers an **Interact → open the map**, all routed
   through the shared `InteractionArbiter`.
2. **Minimap HUD** — `MinimapHud`, an always-on top-right corner map: the player sits centred (north up) and
   discovered rifts within range plot around them via `WithinRange`, refreshed each frame from `MapState`.
3. **Map viewer overlay** — `MapViewerController` (default key **M**, also opened from a rift). Draws the overworld
   backdrop with every rift plotted by world position (discovered → a tappable **fast-travel** button, undiscovered
   → a faint dot), your own marker, sharing friends, and a "let friends see me" opt-in. Fast travel warps the rig
   through `RigTeleporter` (the same safe disable-CC/move/re-enable path respawn uses).
4. **Persistence** — `MapState.CaptureInto`/`RestoreFrom` save discovered rifts + the local sharing opt-in via
   `SaveData.discoveredRifts` / `shareLocation`, folded into `PlayerInventory`.

### Remaining
- **Live friend positions** — the consent-gated path is wired (`Locator.VisibleFriends`, the local opt-in toggle),
  but there's no position feed yet; `MapState` passes an empty set, so friend markers stay correct-but-empty until
  the Nakama presence layer pushes positions. **Locate-self is fully live.**
- **VR opener** — like the other overlays, **M** is keyboard-only (the rift's Interact gives a partial VR path once
  Interact is bound). Tracked in `VR_INPUT_MAP.md`.

## Note on geography

The playable world is **seed-generated** (see `WORLD.md`), so there's no single fixed layout. The illustrated
overworld key-art now backs the map viewer — installed at `Resources/ElementbornUI/worldmap` and loaded by
`MapViewerController.LoadBackdrop()` (it accepts a Sprite or a plain Texture and falls back to a flat panel if the
asset is absent). The rift network stays data-driven in `WorldMap` and is drawn *over* the art by world position,
so the backdrop is purely cosmetic; swapping the image never moves a rift.
