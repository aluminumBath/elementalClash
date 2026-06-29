# Elementborn Unity Update Steps

This file gives a detailed, practical Unity update checklist for the current rolling patch series, using `elementborn_combined_master_patch_v49.zip` as the latest base.

## Before you start

Recommended repo/project locations from the current workflow:

```text
Repo: C:\Users\steel\Desktop\Code\elementalClash
Unity project: C:\Users\steel\Desktop\Code\elementalClash\Elementborn
```

Before importing:

```text
1. Close Unity.
2. Commit or stash any current changes.
3. Create a new branch.
4. Keep a copy of the last working patch zip.
```

Suggested Git commands:

```bash
cd C:\Users\steel\Desktop\Code\elementalClash
git status
git checkout -b elementborn-v49-unity-import
git add .
git commit -m "Backup before importing Elementborn v49 patch"
```

If you do not want to commit yet, make a manual backup ZIP of the `Elementborn` folder first.

---

# 1. Import the latest patch

1. Download the latest patch ZIP.
2. Extract it into the repo root so the included `Elementborn` folder merges with your existing `Elementborn` Unity project.
3. Do **not** extract nested files into `Assets` only unless the ZIP structure specifically requires it.
4. Open Unity Hub.
5. Open:

```text
C:\Users\steel\Desktop\Code\elementalClash\Elementborn
```

6. Wait for Unity to import and compile.

## If Unity shows compile errors immediately

Do not run menus yet. Copy the first 10 unique Console errors and send them back.

Capture:

```text
error code
file path
line/column
full message
first stack trace if available
Unity version
```

---

# 2. Run the new triage kit

After Unity compiles enough for menus to appear, run:

```text
Elementborn → Triage → Run Full Import Triage Kit
```

This generates reports under:

```text
Assets/Elementborn/Generated/Reports/Triage
```

Open these first:

```text
UnityErrorIntakeChecklist.md
CompileIssuePreflightReport.md
ExpectedSceneObjectVerification.md
UnityTestRunnerGuide.md
```

## If the scene enters Play Mode but immediately spams errors

Run:

```text
Elementborn → Triage → 6 Enable Emergency Safe Mode
```

Then enter Play Mode again and manually trigger systems from the Playtest Harness or wrist admin UI.

To restore normal behavior:

```text
Elementborn → Triage → 7 Disable Emergency Safe Mode
```

---

# 3. Run the full playable scene builder

Run:

```text
Elementborn → Playable Setup → Build Rounded Playable Scene
```

This should create/open/save:

```text
Assets/Elementborn/Generated/Scenes/Elementborn_Playable_Test.unity
```

Expected installed systems include:

```text
Elementborn Main Gameplay Loop
Elementborn Runtime Systems
Elementborn Story Debug Dashboard
Elementborn Left Wrist Admin Panel
Elementborn Playtest Harness
Elementborn Playtest Systems
Capital Landmarks
Gameplay Spawn Points
Fire Capital Systems
Crab-Sign Creature Orphanage
Romilus Madrangea Pack
```

---

# 4. Run test readiness setup

Run:

```text
Elementborn → Playtest → Run Test Readiness Setup
```

This updates:

```text
gameplay loop
left wrist admin UI
playtest harness panel
playtest systems
onboarding quest
editor readiness report
```

Then open:

```text
Assets/Elementborn/Generated/Reports/EditorTestReadinessReport_v48.md
```

Also optionally run:

```text
Elementborn → Playtest → Write Editor Test Readiness Report
Elementborn → Triage → 4 Verify Expected Scene Objects
```

---

# 5. Press Play and use the harness

Press Play in the generated scene.

You should see the Playtest Harness panel with buttons:

```text
Start Loop
Teleport Fire
Teleport Orphanage
Teleport Wolf
Spawn Wave
Fire Intro
Social Event
Admit Creature
Save Slot 0
Load Slot 0
Reset Runtime
Reset Saves
Write Report
Stable Fire
Fire Chaos
Clean Fresh
```

Basic smoke-test route:

```text
1. Press Start Loop.
2. Press Teleport Fire.
3. Press Fire Intro.
4. Press Spawn Wave.
5. Press Teleport Orphanage.
6. Press Admit Creature.
7. Press Teleport Wolf.
8. Press Write Report.
```

The runtime readiness report writes to:

```text
Application.persistentDataPath/test_readiness/Elementborn_TestReadinessReport.md
```

---

# 6. Use the left wrist admin UI

Press:

```text
F8
```

The left wrist admin panel should appear. In non-VR/editor playtests it follows the main camera with a left-side offset.

Use:

```text
Category dropdown
Action dropdown
Dynamic form fields
Apply Change
Cheat Code dropdown
Apply Cheat
Raw command input
Run Raw
```

Useful cheat presets:

```text
stabilize_fire_capital
chaos_fire_capital
start_fire_intro
spawn_wave
save_slot_zero
load_slot_zero
admit_demo_creature
resolve_wolf_pack
pulse_volcano
calm_volcano
```

Useful raw commands:

```text
loop.start
loop.spawn
fire.summary
fire.volcano
fire.calm
eventdir.summary
eventdir.eval
chain.summary
socialgroup.summary
orphanage.recovery
narrative.save
narrative.load
```

For VR later, assign:

```text
LeftWristAdminPanelController.leftWristAnchor
```

to the player rig's left wrist/hand transform.

---

# 7. Run EditMode tests

Open:

```text
Window → General → Test Runner
```

Then run EditMode tests first.

Suggested order:

```text
1. Core/map tests
2. Quest/journal/map-marker tests
3. World state tests
4. Political event tests
5. Quest chain tests
6. Story encounter tests
7. Social group tests
8. Orphanage recovery tests
9. Admin/wrist UI tests
10. Playtest readiness tests
```

If tests fail, copy:

```text
test name
assertion failure
first stack trace
Console error immediately above it
```

---

# 8. Run PlayMode tests

Run PlayMode tests after EditMode either passes or the obvious compile errors are fixed.

Expected PlayMode smoke areas:

```text
gameplay loop start/spawn flow
spawn waves
Fire Capital hook start/resolve
volcano pressure pulse/calm
dashboard action buttons
```

If PlayMode tests fail because of missing scene objects, rerun:

```text
Elementborn → Playable Setup → Build Rounded Playable Scene
Elementborn → Playtest → Run Test Readiness Setup
Elementborn → Triage → 4 Verify Expected Scene Objects
```

---

# 9. Unity art/model replacement updates

Use the art audit file as the replacement priority list.

## 9.1 Replace capital landmarks

Generated path after running menus:

```text
Assets/Elementborn/Generated/Prefabs/Capitals
```

Recommended safer workflow:

```text
1. Create authored model/prefab under Assets/Elementborn/Art/Models/Capitals.
2. Open the generated capital prefab.
3. Keep the root GameObject name and CapitalLandmarkDescriptor.
4. Keep PlayerSpawn child where relevant.
5. Delete primitive visual children.
6. Add the authored model as a child.
7. Apply prefab.
8. Reopen Elementborn_Playable_Test scene.
9. Confirm map marker and journal entry still register.
```

Fire Capital first:

```text
Assets/Elementborn/Generated/Prefabs/Capitals/FireCapital_VolcanoCitadel.prefab
```

Target look:

```text
stylized volcano city
chunky basalt silhouette
lava channels
cel-shaded throne-spire
painted bridges
warm orange/red palette
bold readable forms
```

## 9.2 Replace NPC/creature placeholders

Generated folders:

```text
Assets/Elementborn/Generated/Prefabs/SocialNPC
Assets/Elementborn/Generated/Prefabs/StoryEncounters/Placeholders
```

Safe workflow:

```text
1. Keep the root prefab and scripts.
2. Replace only primitive visual children.
3. Keep colliders/interactables/controllers.
4. Apply prefab.
5. Use Playtest Harness to teleport/test.
6. Use wrist admin UI to trigger the related event.
```

Important scripts not to remove:

```text
TimedDualLeaderPackRespawnController
PackLeaderDefeatNotifier
CreatureOrphanageInteractable
CreatureOrphanageRecoveryInteractable
FireCapitalCourtInteractable
SocialNpcDialogueHookInteractable
SocialGroupEventTrigger
```

## 9.3 Replace UI icon sets

Actual folders:

```text
Assets/Elementborn/Art/UI/MapIcons
Assets/Elementborn/Art/UI/Common
Assets/Elementborn/Art/UI/BossIcons
Assets/Elementborn/Art/UI/CombatIcons
Assets/Elementborn/Art/UI/SpellIcons
Assets/Elementborn/Art/UI/QuestIcons
Assets/Elementborn/Art/UI/EquipmentIcons
Assets/Elementborn/Art/VFX/HitFeedback
```

Recommended import settings:

```text
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Alpha Source: Input Texture Alpha
Pixels Per Unit: 100
Mesh Type: Full Rect for panels, Tight for icons
Compression: None or High Quality
Filter Mode: Bilinear for painted UI, Point only if intentionally pixel art
Generate Mip Maps: off for UI icons
```

Wind Waker-like UI direction:

```text
parchment panels
painted wood/rope trim
hand-inked outlines
less mobile-game gloss
less circular badge framing
warm fantasy palette
simple high-readability shapes
```

## 9.4 Replace audio placeholders

Actual folder:

```text
Assets/Elementborn/Audio/Generated
```

Safer workflow:

```text
1. Put final audio in Assets/Elementborn/Audio/Custom or Audio/Final.
2. Keep old generated files until routing is confirmed.
3. Update the sound bank or generator references in Unity.
4. Test with wrist admin actions and Playtest Harness.
5. Remove/ignore generated placeholders later.
```

Target sound:

```text
short musical UI chimes
playful impacts
airy wind swells
soft marimba/woodblock accents
less synthetic placeholder tone
```

---

# 10. What to send back for the next fix patch

After Unity import/testing, send:

```text
1. first 10 unique Console errors
2. first 5 failed EditMode tests
3. first 5 failed PlayMode tests
4. whether Elementborn menus appear
5. whether Build Rounded Playable Scene completed
6. whether Playtest Harness appears
7. whether F8 opens wrist admin UI
8. generated readiness report if possible
```

The next likely patch should be:

```text
v50: Unity compiler/test fix pass
```

It should be based on real Unity errors rather than adding new systems.
