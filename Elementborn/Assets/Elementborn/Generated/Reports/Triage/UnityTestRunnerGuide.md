# Unity Test Runner Guide

## Before running tests

```text
1. Import latest patch.
2. Let Unity compile.
3. Run Elementborn → Triage → Run Full Import Triage Kit.
4. Run Elementborn → Playable Setup → Build Rounded Playable Scene.
5. Run Elementborn → Playtest → Run Test Readiness Setup.
```

## EditMode tests

Run all EditMode tests first. Suggested order:

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

## PlayMode tests

Run PlayMode after EditMode passes or after obvious compile errors are fixed.

```text
1. Gameplay loop PlayMode smoke tests
2. Fire Capital PlayMode smoke tests
3. Dashboard/admin PlayMode smoke tests
```

## Expected smoke-test route

```text
1. Press Play.
2. Press F8 to show wrist admin UI.
3. Use Playtest Harness → Start Loop.
4. Teleport Fire.
5. Trigger Fire Intro.
6. Spawn Wave.
7. Teleport Orphanage.
8. Admit Creature.
9. Teleport Wolf.
10. Write Report.
```

## Send back if failures occur

```text
- first 10 unique Console errors
- failed test names
- first stack trace for each failure
- whether scene generation completed
- whether F8 wrist UI opened
- whether Playtest Harness was visible
```
