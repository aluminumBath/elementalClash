# Map, minimap & fast travel

Runtime navigation on top of the procedural `WorldMap`: a minimap, a full map viewer, leyline-rift fast travel,
and locating yourself and (consenting) friends. The rules live in `MapNavigation.cs` (Core, pure, unit-tested in
`MapNavigationTests`); the HUD/overlay and world objects are the wiring step (below).

## Leyline rifts (fast travel)

A `LeylineRift` is a fast-travel node at a world position. The `FastTravelNetwork` registers every rift but only
lets you **warp to ones you've discovered** (stepped on / activated): `Discover(id)`, `CanTravelTo(id)`,
`Discovered()`, and `NearestDiscovered(pos)` for "warp to closest". Discovery is savable and never un-sets.

## Elemental portals (capital pools)

Rifts are now **elemental portals**: a `LeylineRift` carries an `Element` (null for the neutral Confluence gate and
crossings) and a `PortalTier` — `Capital` (the elemental capital hubs) or `City`. `PortalTheme.For(element)` gives
each its look and glow colour: **water = a glassy pond that glows teal**, fire a molten rift, earth a mossy arch,
air a shimmering updraft, and the neutral gate a prismatic ring.

The routing is **discovery-gated and element-scoped**: you discover a city portal by visiting it, and from an
elemental **capital pool** you may travel only to the **discovered city portals of that same element** — capitals
themselves are hubs, not destinations from a pool. The rules are pure Core:
`FastTravelNetwork.DiscoveredCitiesOfElement(element)` and `CanRouteFromCapital(element, destId)`, unit-tested in
`PortalNetworkTests`. `WorldMapLayout` tags the four capitals (Stonereach/Earth, Gale Roost/Air, Tidewatch/Water,
Ember Bastion/Fire) and adds eight discoverable city portals (two per element).

In-world: `LeylineRiftObject` tints its meshes with the portal's `PortalTheme` colour and **brightens when
discovered**; a **capital** portal's Interact opens the **`PortalPoolController`** — an overlay listing that
element's discovered city portals as glowing, tap-to-travel buttons (warp via `MapState.WarpToRift`). A city or
neutral portal's Interact still opens the leyline map.

### The portal-pool map room

`PortalPoolRoom` builds the map as a **shimmering shallow pool you stand inside**: the overworld map is painted on
the pool floor, a translucent toon-water surface (the `Elementborn/ToonWater` shader) shimmers over it, and every
portal stands as a glowing **`PortalNode`** in the water at its map position (projected via `Minimap.WorldToNormalized`).
You **walk up and Interact to step through** — no map UI, because you're standing on the map. Discovered nodes glow
in their `PortalTheme` colour and warp you to that portal's world location (`MapState.WarpToRift`); undiscovered ones
stay dim and just hint you must find them first — so **everything discovered is reachable from the pool**. A large
inverted **water globe** plus a **reflection probe** give the "suspended inside a sphere of water, looking out" look.
Drop the component where the player stands and Play. Water transparency, the globe's inside-out cull, bespoke node
meshes, and VR scale are shader/inspector tuning; the layout, discovery glow, and travel are wired.

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

### Friend positions (the live feed)

The receive side is built and runs offline. A Core `PresenceRegistry` holds other players' positions with a
freshness window (a friend silent past the window drops off the map on their own). `MapState` owns one, and each
poll (~0.5 s) drives a registered `IFriendPresence` producer — publishing the local position when the player has
opted in, and pumping friends' positions into the registry — then rebuilds the **consent-gated** friend set
(`Locator.VisibleFriends`, so only friends who are both broadcasting and known appear). Friends are drawn on the
map viewer and the minimap (green). With no producer registered the set stays empty, exactly as before.

A `SimulatedFriendPresence` component (sandbox demo, added by the bootstrap) seeds one ally and orbits it so the
markers + consent path are visible without a server. Online, **`NakamaFriendPresence`** (behind the
`ELEMENTBORN_NAKAMA` define) is the real producer: it broadcasts the local position as the player's Nakama status —
packed by the pure, tested `PresenceCodec` — only while sharing, follows the current friends to receive theirs, and
caches what arrives, mirroring the existing Nakama adapters. `NakamaSocialInstaller` registers it after connect via
`MapState.SetPresence(...)`.

### Remaining
- **Live verification** — `NakamaFriendPresence` follows the project's live-server Nakama workflow; the offline
  gates don't compile the `#if` branch, so exercise it against a running Nakama with two clients. The dev simulator
  stands in offline, and `PresenceCodec` is unit-tested.
- **VR opener** — like the other overlays, **M** is keyboard-only (the rift's Interact gives a partial VR path once
  Interact is bound). Tracked in `VR_INPUT_MAP.md`.

## Checkpoints (respawn shrines)

Distinct from the leyline rifts, **checkpoints** are respawn shrines. `WorldMap.Checkpoints` defines the canonical
set (the cardinal *waystones* ringing the central crystal). `CheckpointSpawner` drops an amber obelisk for each,
snapped to the terrain, carrying a `CheckpointObject` (`IInteractable`): standing in range offers an Interact —
**"Set respawn point"** — which activates it through `CheckpointState` (a toast + a `UiConfirm` cue). The active
checkpoint is the most recently activated one; activating another moves the anchor.

`RespawnController` revives the player at the **active checkpoint if one is set, then a claimed house, then the
scene spawn** — a checkpoint is the player's most recent explicit "respawn here", so it wins. `CheckpointState`
persists the activated set + the active anchor via `SaveData.activatedCheckpoints` / `activeCheckpoint` through
`PlayerInventory`, and exposes `Markers()` (kind `MapMarkerKind.Checkpoint`) so the map viewer and minimap draw all
shrines in amber, with the active one brightened and labelled. The pure `CheckpointLog` is unit-tested.

## Note on geography

The playable world is **seed-generated** (see `WORLD.md`), so there's no single fixed layout. The illustrated
overworld key-art now backs the map viewer — installed at `Resources/ElementbornUI/worldmap` and loaded by
`MapViewerController.LoadBackdrop()` (it accepts a Sprite or a plain Texture and falls back to a flat panel if the
asset is absent). The rift network stays data-driven in `WorldMap` and is drawn *over* the art by world position,
so the backdrop is purely cosmetic; swapping the image never moves a rift.
