# v48 Functionality: Unity Test Readiness Pass

This patch adds the pre-Unity-test hardening layer requested before running Unity compiler/tests.

## Major additions

```text
One-click test readiness setup
Scene health/readiness scanner
In-game playtest harness panel
Runtime reset tools
Admin/playtest presets
First-run onboarding quest
Editor readiness report
Additional EditMode tests
```

## New runtime systems

```text
ElementbornTestReadinessSeverity
ElementbornPlaytestPreset
ElementbornPlaytestTeleportTarget
ElementbornTestReadinessIssue
ElementbornTestReadinessReport
ElementbornTestReadinessScanner
ElementbornPlaytestResetService
ElementbornPlaytestPresetService
ElementbornPlaytestHarnessController
ElementbornPlaytestHarnessPanel
```

## New Unity menus

```text
Elementborn → Playtest → Run Test Readiness Setup
Elementborn → Playtest → Generate Test Harness Prefab
Elementborn → Playtest → Install Test Harness In Open Scene
Elementborn → Playtest → Create Onboarding Quest
Elementborn → Playtest → Write Editor Test Readiness Report
Elementborn → Playtest → Reset Runtime State In Open Scene
Elementborn → Playtest → Delete Playtest Saves
```

## One-click setup

```text
Elementborn → Playtest → Run Test Readiness Setup
```

This installs/updates:

```text
gameplay loop
left wrist admin UI
playtest harness panel
playtest systems
onboarding quest
editor readiness report
```

## Playtest Harness buttons

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

## Presets

```text
CleanFreshPlaytest
StableFireCapital
FireCapitalInChaos
WolfPackUnresolved
CreatureRanAway
SocialChaosActive
FullSystemsReady
```

## Readiness report

At runtime, the harness can write:

```text
Application.persistentDataPath/test_readiness/Elementborn_TestReadinessReport.md
```

The editor menu writes:

```text
Assets/Elementborn/Generated/Reports/EditorTestReadinessReport_v48.md
```

## Onboarding quest

Added:

```text
Assets/Elementborn/Generated/QuestUI/Playtest/playtest_onboarding_route.asset
```

The onboarding route points testers through:

```text
Fire Capital
left wrist admin panel
creature orphanage
wolf pack
readiness report
```

## Recommended Unity flow

```text
1. Import v48.
2. Let Unity compile.
3. Run Elementborn → Playable Setup → Build Rounded Playable Scene.
4. Run Elementborn → Playtest → Run Test Readiness Setup.
5. Press Play.
6. Use the Playtest Harness panel.
7. Press F8 for left wrist admin UI.
8. Write the readiness report.
9. Run EditMode tests.
10. Run PlayMode tests.
```
