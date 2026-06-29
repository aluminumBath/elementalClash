# v49 Functionality: Unity Import + Compile Triage Kit

This patch adds the final pre-Unity-import/testing support layer.

## Main menu

```text
Elementborn → Triage → Run Full Import Triage Kit
```

## Added triage tools

```text
Unity Error Intake Checklist
Compile Issue Preflight Report
Repair Missing References And Regenerate
Expected Scene Object Verification
Unity Test Runner Guide
Emergency Safe Mode
```

## Menu items

```text
Elementborn → Triage → Run Full Import Triage Kit
Elementborn → Triage → 1 Write Unity Error Intake Checklist
Elementborn → Triage → 2 Write Compile Issue Report
Elementborn → Triage → 3 Repair Missing References And Regenerate
Elementborn → Triage → 4 Verify Expected Scene Objects
Elementborn → Triage → 5 Write Test Runner Guide
Elementborn → Triage → 6 Enable Emergency Safe Mode
Elementborn → Triage → 7 Disable Emergency Safe Mode
Elementborn → Triage → Write Emergency Safe Mode Guide
```

## Generated reports

```text
Assets/Elementborn/Generated/Reports/Triage/UnityErrorIntakeChecklist.md
Assets/Elementborn/Generated/Reports/Triage/CompileIssuePreflightReport.md
Assets/Elementborn/Generated/Reports/Triage/RepairMissingReferencesReport.md
Assets/Elementborn/Generated/Reports/Triage/ExpectedSceneObjectVerification.md
Assets/Elementborn/Generated/Reports/Triage/UnityTestRunnerGuide.md
Assets/Elementborn/Generated/Reports/Triage/EmergencySafeModeGuide.md
Assets/Elementborn/Generated/Reports/Triage/EmergencySafeModeStatus.md
```

## Repair/regenerate sequence

The repair menu reruns the core generators and installers in a safe order:

```text
Fire Capital royal family roster
NPC roster import
capital landmarks
Fire Capital assets
capital world state
political events
quest chains
social NPCs
social groups
story encounters
gameplay loop assets
onboarding quest
left wrist admin UI
playtest harness
runtime scene installers
dashboard
save bridges
```

## Emergency Safe Mode

Emergency Safe Mode disables optional auto-start style fields where they exist:

```text
startOnAwake
startOnStart
startIntroQuestOnStart
pulseAutomatically
autoRotate
```

Use it if the scene compiles but entering Play Mode immediately spams runtime errors. Disable it when you are ready to test the full loop again.

## Recommended next step

```text
1. Import v49 into Unity.
2. Let Unity compile.
3. Run Elementborn → Triage → Run Full Import Triage Kit.
4. Run Elementborn → Playable Setup → Build Rounded Playable Scene.
5. Run Elementborn → Playtest → Run Test Readiness Setup.
6. Run EditMode tests.
7. Run PlayMode tests.
8. Send first unique Console errors for the v50 fix pass.
```
