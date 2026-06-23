# Elementborn — Documentation Index

Start here. The docs in reading order:

1. **`README.md`** — what the project is, the architecture, and every system at a glance.
2. **`GETTING_STARTED.md`** — open the project and get it running (flat → VR → tests).
3. **`DEPLOYMENT.md`** — the full reference: requirements, scene/prefab wiring, XR/OpenXR, per-platform
   builds, Android signing, CI, the Quest performance checklist, troubleshooting, and the release flow.
4. **`PORTING.md`** — the flat/third-person path and what console ports require (platform programs, SDKs,
   dev kits, certification).
5. **`WHATS_LEFT.md`** — the remaining-work roadmap: what's done, what's your turn, and optional enhancements.
6. **`LIMITATIONS.md`** — what can't be built from a code-only environment (art, water, console, VR tuning,
   Editor assets) plus starter fixes, and the one in-code gap (Interact arbitration).
7. **`ART_GUIDE.md`** — replace the code-built placeholders with low-poly, vertex-colored art in Blender,
   object by object.
8. **`PALETTE.md`** — the importable color palette (Blender script + labeled PNG + `.gpl`) in `palette/`.
9. **`MODELS.md`** — the placeholder 3D models and how the Blender / `.ply` / `.fbx` import path works.
10. **`UI_SPRITES.md`** — exact sprite sizes, anchors, and 9-slice borders for the code-built UI.
11. **`GENERATED_ART.md`** — the ready-made 2D UI/particle sprites and control glyphs.
12. **`AUDIO.md`** — the synthesized placeholder SFX and the element/ability-to-sound map.
13. **`INPUT.md`** — the flat/gamepad control scheme, runtime rebinding, and how it relates to VR.
14. **`VR_COMBAT.md`** — VR motion combat: the gesture recognizer, per-element fighting styles, and the
    stance layer (hold-to-charge, guard, Water's ice-flow combo).
15. **`ARENA.md`** — the dedicated combat mode: escalating waves, telegraphed/dodgeable enemies, combo
    scoring, stamina pacing, and the Heavy/Sweep per-element kit.
16. **`UNDERWATER.md`** — the underwater layer: submerged state, breathing/drowning, per-element rules, the
    freeze-trap / air-bubble interactions, and comfort-vignetted swim locomotion.
17. **`CREATURES.md`** — the creature roster: wild beasts, rideable mounts and rare companions, the exotic
    apex set with two-element mix attacks, wildlife by habitat, and the underwater ambushers and serpent boss.
18. **`HIDDEN_MOVES.md`** — the four hidden signature moves (Water spin-dash, Earth rock-armor, Air tornado,
    Fire breath) and their special gestures.
19. **`FACTIONS.md`** — the four joinable factions (Symbiasts, Separatists, Cleicists, Synodists): creeds,
    strength/weakness perks, and their stances on the Confluence and mixed gifts.
20. **`MODDING.md`** — the data-driven content system: registries, the JSON mod format, where mods live, and
    how to make a new content type moddable.
21. **`PLANTS.md`** — interactive flora (snaptrap, vines, spores, gleamlily + heartfruit, venomlily, willow
    gates) and plant control via the Verdancy specialty.
22. **`NPCS.md`** — the three guide NPCs (Willow, Kiana, Parfa): creature hints, Willow's sidekick menagerie
    and its hidden-ability reward, Parfa's frog trick, and Kiana's creature-welfare enforcement.
23. **`EVOLUTION.md`** — the evolution game mode: start one element, take a second, unlock a specialty.
24. **`SOCIAL.md`** — the online social layer (accounts/roles, notifications, feedback, friends, messaging,
    moderation): the backend choice (Nakama), the backend-agnostic seams, and the local-first build.
25. **`BOOTSTRAP.md`** — the one-click Editor generator for a playable scene + rig prefabs (and the cel-shaded
    materials), and what to finish by hand.
26. **`NETCODE.md`** — taking the social layer online with Nakama: the backend seam, the client adapters behind a
    define, the server module, and a local server.
27. **`VR_SETUP.md`** — getting into the headset: the XR plug-in, the rig structure, comfort locomotion, and the
    device-bound tuning that needs hardware.
28. **`QUESTS.md`** — the NPC → dialogue → quest → reward loop: the quest engine, the event bus, the starter
    quests, and how to add more.

## By task

| I want to… | Go to |
| --- | --- |
| Understand the project | `README.md` |
| Get the zip running | `GETTING_STARTED.md` |
| Wire the scene & prefabs | `DEPLOYMENT.md` §4 |
| Configure XR / OpenXR | `DEPLOYMENT.md` §3 |
| Build for flat PC / PCVR / Quest | `DEPLOYMENT.md` §5–§7 |
| Sign & release | `DEPLOYMENT.md` §8, §12 |
| Set up CI | `DEPLOYMENT.md` §9 |
| Cut a versioned release / publish docs | `DEPLOYMENT.md` §13 |
| Build a playable scene + rig prefabs (one click) | `BOOTSTRAP.md` |
| Change controls / add a gamepad / rebind | `INPUT.md` |
| Know what's still left | `WHATS_LEFT.md` |
| Make the 3D art | `ART_GUIDE.md` (+ `PALETTE.md`) |
| Make the UI art | `UI_SPRITES.md` (+ `PALETTE.md`) |
| Use the ready-made UI/particle sprites | `GENERATED_ART.md` |
| Use the placeholder 3D models | `MODELS.md` |
| Add or change sound effects | `AUDIO.md` |
| Port to consoles | `PORTING.md` (+ `LIMITATIONS.md`) |
| Know what can't be built here | `LIMITATIONS.md` |
| Add online social features | `SOCIAL.md` |
| Take the social layer online (Nakama) | `NETCODE.md` |
| Set up VR (headset, comfort locomotion) | `VR_SETUP.md` |
| See how the code is organized | `README.md` (Architecture / Layout) |

## A note on scope
The repository is **code-complete and statically verified** — gameplay systems, shaders, the mesh terrain,
tests, CI, and these docs. The remaining work (running it in the Editor, making art, building, and shipping)
can only happen in the Unity Editor, Blender, and on devices; see `WHATS_LEFT.md`.
