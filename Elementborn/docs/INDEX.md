# Elementborn — Documentation Index

Start here. The docs in reading order:

1. **`README.md`** — what the project is, the architecture, and every system at a glance.
2. **`GETTING_STARTED.md`** — open the project and get it running (flat → VR → tests).
3. **`DEPLOYMENT.md`** — the full reference: requirements, scene/prefab wiring, XR/OpenXR, per-platform
   builds, Android signing, CI, the Quest performance checklist, troubleshooting, and the release flow.
4. **`WHATS_LEFT.md`** — the remaining-work roadmap: what's done, what's your turn, and optional
   enhancements.
5. **`ART_GUIDE.md`** — replace the code-built placeholders with low-poly, vertex-colored art in Blender,
   object by object.
6. **`PALETTE.md`** — the importable color palette (Blender script + labeled PNG + `.gpl`) in `palette/`.
7. **`UI_SPRITES.md`** — exact sprite sizes, anchors, and 9-slice borders for the code-built UI.

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
| Know what's still left | `WHATS_LEFT.md` |
| Make the 3D art | `ART_GUIDE.md` (+ `PALETTE.md`) |
| Make the UI art | `UI_SPRITES.md` (+ `PALETTE.md`) |
| See how the code is organized | `README.md` (Architecture / Layout) |

## A note on scope
The repository is **code-complete and statically verified** — gameplay systems, shaders, the mesh terrain,
tests, CI, and these docs. The remaining work (running it in the Editor, making art, building, and shipping)
can only happen in the Unity Editor, Blender, and on devices; see `WHATS_LEFT.md`.
