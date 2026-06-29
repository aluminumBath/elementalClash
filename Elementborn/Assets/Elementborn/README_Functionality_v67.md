# v67 Functionality: Restore Test Discovery with Safe Smoke Tests

The latest exported test XML showed:

```text
testcasecount="0"
total="0"
passed="0"
failed="0"
```

So the Unity Test Runner was not discovering any tests.

## Why

Earlier broken tests were removed/quarantined to get the project compiling. That left the Test Runner with no valid test assemblies to run.

## What v67 adds

v67 adds a small safe smoke-test suite under:

```text
Assets/Tests/ElementbornSmoke/
```

### EditMode

```text
Assets/Tests/ElementbornSmoke/EditMode/Elementborn.EditMode.SmokeTests.asmdef
Assets/Tests/ElementbornSmoke/EditMode/ElementbornEditModeSmokeTests.cs
```

Tests:
- confirms the test assembly is discoverable
- confirms critical runtime types are present by reflection:
  - ElementbornRuntimeBootstrap
  - PlayerAttunementHud
  - BoatController

### PlayMode

```text
Assets/Tests/ElementbornSmoke/PlayMode/Elementborn.PlayMode.SmokeTests.asmdef
Assets/Tests/ElementbornSmoke/PlayMode/ElementbornPlayModeSmokeTests.cs
```

Tests:
- confirms PlayMode can advance a frame
- creates `PlayerAttunementHud` without serialized UI references and waits a frame, catching the HUD null-safety class of bug

## Expected result

After applying v67 and reopening Unity, Test Runner should discover at least:

```text
EditMode: 2 tests
PlayMode: 2 tests
```
