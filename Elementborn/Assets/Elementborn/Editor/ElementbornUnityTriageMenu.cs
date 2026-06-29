#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornUnityTriageMenu
    {
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";
        private const string TriageDir = "Assets/Elementborn/Generated/Reports/Triage";

        [MenuItem("Elementborn/Triage/Run Full Import Triage Kit")]
        public static void RunFullImportTriageKit()
        {
            Directory.CreateDirectory(TriageDir);
            WriteUnityErrorIntakeChecklist();
            WriteCompileIssueReport();
            RepairMissingReferencesAndRegenerate();
            VerifyExpectedSceneObjects();
            WriteTestRunnerGuide();
            WriteEmergencySafeModeGuide();
            AssetDatabase.Refresh();
            Debug.Log("Elementborn full import triage kit complete.");
        }

        [MenuItem("Elementborn/Triage/1 Write Unity Error Intake Checklist")]
        public static void WriteUnityErrorIntakeChecklist()
        {
            Directory.CreateDirectory(TriageDir);
            File.WriteAllText($"{TriageDir}/UnityErrorIntakeChecklist.md",
@"# Unity Error Intake Checklist

Use this when Unity reports compile or playtest errors.

## Copy errors from Console

```text
1. Open Unity Console.
2. Turn off Collapse if you need full repeated details.
3. Click the first real C# compile error.
4. Copy:
   - full error code
   - file path
   - line/column
   - message
5. Repeat for the first 10 unique errors.
```

## Group errors by kind

```text
1. Missing type / namespace
2. Missing method or overload
3. Missing enum member
4. Serialized field/property name changed
5. Duplicate type
6. Assembly/reference problem
7. UI prefab/template problem
8. Runtime null reference
```

## First-pass fix order

```text
1. Duplicate type errors
2. Missing enum/type errors
3. Missing method signature errors
4. Editor generator errors
5. Runtime/UI prefab errors
6. Test-only errors
```

## What to send back

```text
- first 10 unique Console errors
- Unity version
- whether import finished
- whether menus appeared
- whether Build Rounded Playable Scene ran
- whether EditMode or PlayMode tests were run
```
");
            Debug.Log("Wrote Unity error intake checklist.");
        }

        [MenuItem("Elementborn/Triage/2 Write Compile Issue Report")]
        public static void WriteCompileIssueReport()
        {
            Directory.CreateDirectory(TriageDir);
            var report = new StringBuilder();
            report.AppendLine("# Elementborn Compile Issue Preflight Report");
            report.AppendLine();
            report.AppendLine("This is a static preflight report from inside Unity. It does not replace the compiler.");
            report.AppendLine();

            string[] csFiles = Directory.GetFiles("Assets/Elementborn", "*.cs", SearchOption.AllDirectories);
            int braceIssues = 0;
            int oldPromptIssues = 0;
            int badFindIssues = 0;
            int duplicateTypeIssues = 0;
            var typeMap = new Dictionary<string, List<string>>();

            foreach (string path in csFiles)
            {
                string text = File.ReadAllText(path);
                if (Count(text, '{') != Count(text, '}'))
                {
                    braceIssues++;
                    report.AppendLine($"- **Brace balance** `{path}`: {{={Count(text, '{')} }}={Count(text, '}')}");
                }

                if (text.Contains("public override string " + "GetPrompt(GameObject interactor)"))
                {
                    oldPromptIssues++;
                    report.AppendLine($"- **Old GetPrompt signature** `{path}`");
                }

                if (text.Contains("." + "ElementbornFindUtility"))
                {
                    badFindIssues++;
                    report.AppendLine($"- **Bad ElementbornFindUtility reference** `{path}`");
                }

                foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(text, "\\b(?:class|struct|enum|interface)\\s+([A-Za-z_][A-Za-z0-9_]*)"))
                {
                    string name = match.Groups[1].Value;
                    if (!typeMap.ContainsKey(name))
                    {
                        typeMap[name] = new List<string>();
                    }
                    typeMap[name].Add(path);
                }
            }

            foreach (var kvp in typeMap)
            {
                if (kvp.Value.Count > 1 && kvp.Key != "Object")
                {
                    duplicateTypeIssues++;
                    report.AppendLine($"- **Duplicate type name** `{kvp.Key}`:");
                    foreach (string path in kvp.Value)
                    {
                        report.AppendLine($"  - `{path}`");
                    }
                }
            }

            report.AppendLine();
            report.AppendLine("## Summary");
            report.AppendLine();
            report.AppendLine("```text");
            report.AppendLine($"C# files scanned: {csFiles.Length}");
            report.AppendLine($"Brace issues: {braceIssues}");
            report.AppendLine($"Old GetPrompt signatures: {oldPromptIssues}");
            report.AppendLine($"Bad ElementbornFindUtility references: {badFindIssues}");
            report.AppendLine($"Duplicate type names: {duplicateTypeIssues}");
            report.AppendLine("```");

            if (braceIssues + oldPromptIssues + badFindIssues + duplicateTypeIssues == 0)
            {
                report.AppendLine();
                report.AppendLine("No obvious generated-code hazards found by the triage scanner.");
            }

            File.WriteAllText($"{TriageDir}/CompileIssuePreflightReport.md", report.ToString());
            AssetDatabase.Refresh();
            Debug.Log("Wrote compile issue preflight report.");
        }

        [MenuItem("Elementborn/Triage/3 Repair Missing References And Regenerate")]
        public static void RepairMissingReferencesAndRegenerate()
        {
            Directory.CreateDirectory(TriageDir);

            Safe("Write Fire Capital royal family roster", () => FireCapitalRoyalFamilyGenerator.GenerateAll());
            Safe("Import all NPC rosters", () => NpcRosterCsvImporter.ImportAllRosters());
            Safe("Generate capital landmarks", () => CapitalLandmarkPrefabGenerator.GenerateAll());
            Safe("Generate Fire Capital assets", () => FireCapitalAssetGenerator.GenerateAll());
            Safe("Generate capital world state assets", () => CapitalWorldStateAssetGenerator.GenerateAll());
            Safe("Generate political world events", () => PoliticalWorldEventAssetGenerator.GenerateAll());
            Safe("Generate quest chain assets", () => QuestChainAssetGenerator.GenerateAll());
            Safe("Generate social NPC quests/dialogue", () => SocialNpcQuestAndPrefabGenerator.GenerateQuestsAndDialogue());
            Safe("Create social NPC placeholder prefabs", () => SocialNpcQuestAndPrefabGenerator.CreatePlaceholderPrefabs());
            Safe("Generate social group assets", () => SocialGroupAssetGenerator.GenerateAll());
            Safe("Generate story encounter assets", () => StoryEncounterAssetGenerator.GenerateAll());
            Safe("Generate gameplay loop assets", () => GameplayLoopAssetGenerator.GenerateAll());
            Safe("Create onboarding quest", () => ElementbornTestReadinessSetupMenu.CreateOnboardingQuest());
            Safe("Generate left wrist admin UI prefab", () => AdminWristUiSetupMenu.GeneratePrefab());
            Safe("Generate playtest harness prefab", () => ElementbornTestReadinessSetupMenu.GenerateTestHarnessPrefab());

            if (!Application.isPlaying)
            {
                Safe("Install capital landmarks in open scene", () => CapitalLandmarkPrefabGenerator.InstallInOpenScene());
                Safe("Install Fire Capital systems in open scene", () => FireCapitalAssetGenerator.InstallSystems());
                Safe("Install capital world state systems", () => CapitalWorldStateAssetGenerator.InstallSystems());
                Safe("Install political event director", () => PoliticalWorldEventAssetGenerator.InstallDirector());
                Safe("Install quest chain director", () => QuestChainAssetGenerator.InstallDirector());
                Safe("Install social group registry", () => SocialGroupAssetGenerator.InstallRegistry());
                Safe("Install story encounter registry", () => StoryEncounterAssetGenerator.InstallRegistry());
                Safe("Install gameplay loop", () => GameplayLoopAssetGenerator.InstallGameplayLoopInOpenScene());
                Safe("Install narrative save bridges", () => NarrativeRuntimeSaveSetupMenu.InstallNarrativeRuntimeSaveBridges());
                Safe("Install dashboard", () => StorySystemsDebugDashboardSetupMenu.InstallDashboard());
                Safe("Install wrist admin UI", () => AdminWristUiSetupMenu.InstallInOpenScene());
                Safe("Install playtest harness", () => ElementbornTestReadinessSetupMenu.InstallTestHarnessInOpenScene());
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            File.WriteAllText($"{TriageDir}/RepairMissingReferencesReport.md",
@"# Repair Missing References Report

The repair menu completed its best-effort regeneration/install sequence.

## Regenerated

```text
NPC rosters
Capital landmarks
Fire Capital assets
Capital world state
Political events
Quest chains
Social NPCs
Social groups
Story encounters
Gameplay loop assets
Onboarding quest
Left wrist admin UI prefab
Playtest harness prefab
```

## Reinstalled in open scene when possible

```text
capital landmarks
Fire Capital systems
capital world state systems
political event director
quest chain director
social group registry
story encounter registry
gameplay loop
narrative save bridges
dashboard
wrist admin UI
playtest harness
```
");
            Debug.Log("Repair/regenerate pass complete.");
        }

        [MenuItem("Elementborn/Triage/4 Verify Expected Scene Objects")]
        public static void VerifyExpectedSceneObjects()
        {
            Directory.CreateDirectory(TriageDir);
            var report = new StringBuilder();
            report.AppendLine("# Expected Scene Object Verification");
            report.AppendLine();

            Check(report, "Player tagged object", FindPlayer() != null, "Create Player Test Rig or Build Rounded Playable Scene.");
            Check(report, "Main Camera", Camera.main != null, "Create a camera tagged MainCamera.");
            Check(report, "EventSystem", UnityEngine.Object.FindObjectOfType<EventSystem>(true) != null, "Create an EventSystem.");
            Check(report, "Runtime bootstrap", UnityEngine.Object.FindObjectOfType<ElementbornRuntimeBootstrap>(true) != null, "Run Playable Setup → Create Runtime Systems Bootstrap.");
            Check(report, "Gameplay loop director", UnityEngine.Object.FindObjectOfType<ElementbornMainGameplayLoopDirector>(true) != null, "Run Gameplay Loop → Install Gameplay Loop In Open Scene.");
            Check(report, "Spawn registry", UnityEngine.Object.FindObjectOfType<ElementbornSpawnRegistry>(true) != null, "Run Gameplay Loop → Install Spawn Points.");
            Check(report, "Story dashboard", UnityEngine.Object.FindObjectOfType<StorySystemsDebugDashboard>(true) != null, "Run Debug → Install Story Systems Debug Dashboard.");
            Check(report, "Left wrist admin UI", UnityEngine.Object.FindObjectOfType<AdminWristPanelView>(true) != null, "Run Admin UI → Install Left Wrist Admin UI.");
            Check(report, "Playtest harness", UnityEngine.Object.FindObjectOfType<ElementbornPlaytestHarnessPanel>(true) != null, "Run Playtest → Install Test Harness.");
            Check(report, "Fire Capital registry", UnityEngine.Object.FindObjectOfType<FireCapitalRegistry>(true) != null, "Run Fire Capital → Install Fire Capital Systems.");
            Check(report, "Capital landmarks", UnityEngine.Object.FindObjectsOfType<CapitalLandmarkDescriptor>(true).Length > 0, "Run Capitals → Install Capital Landmarks.");
            Check(report, "Spawn points", UnityEngine.Object.FindObjectsOfType<ElementbornSpawnPoint>(true).Length > 0, "Run Gameplay Loop → Install Spawn Points.");

            File.WriteAllText($"{TriageDir}/ExpectedSceneObjectVerification.md", report.ToString());
            AssetDatabase.Refresh();
            Debug.Log("Wrote expected scene object verification report.");
        }

        [MenuItem("Elementborn/Triage/5 Write Test Runner Guide")]
        public static void WriteTestRunnerGuide()
        {
            Directory.CreateDirectory(TriageDir);
            File.WriteAllText($"{TriageDir}/UnityTestRunnerGuide.md",
@"# Unity Test Runner Guide

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
");
            Debug.Log("Wrote Unity Test Runner guide.");
        }

        [MenuItem("Elementborn/Triage/6 Enable Emergency Safe Mode")]
        public static void EnableEmergencySafeMode()
        {
            SetAutoStartFlags(false);
            File.WriteAllText($"{TriageDir}/EmergencySafeModeStatus.md",
@"# Emergency Safe Mode

Status: ENABLED

## What changed

```text
Gameplay loop auto-start disabled where serialized fields are present.
Playtest/harness/dashboard systems can still be run manually from menus or buttons.
```

## Use this when

```text
Unity compiles, but entering Play Mode immediately spams errors.
You need the scene to open quietly so you can inspect objects.
```
");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Emergency safe mode enabled.");
        }

        [MenuItem("Elementborn/Triage/7 Disable Emergency Safe Mode")]
        public static void DisableEmergencySafeMode()
        {
            Directory.CreateDirectory(TriageDir);
            SetAutoStartFlags(true);
            File.WriteAllText($"{TriageDir}/EmergencySafeModeStatus.md",
@"# Emergency Safe Mode

Status: DISABLED

Autostart behavior was restored where serialized fields are present.
");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Emergency safe mode disabled.");
        }

        [MenuItem("Elementborn/Triage/Write Emergency Safe Mode Guide")]
        public static void WriteEmergencySafeModeGuide()
        {
            Directory.CreateDirectory(TriageDir);
            File.WriteAllText($"{TriageDir}/EmergencySafeModeGuide.md",
@"# Emergency Safe Mode Guide

Emergency Safe Mode is a triage helper for noisy Play Mode startup.

## Enable

```text
Elementborn → Triage → 6 Enable Emergency Safe Mode
```

## Disable

```text
Elementborn → Triage → 7 Disable Emergency Safe Mode
```

## Purpose

```text
- disable auto-start fields where available
- reduce runtime side effects on entering Play Mode
- allow manual testing from dashboard/admin/harness buttons
```

## After enabling

```text
1. Press Play.
2. Open Playtest Harness.
3. Press Start Loop manually.
4. Use one feature at a time.
```
");
        }

        private static void SetAutoStartFlags(bool enabled)
        {
            Directory.CreateDirectory(TriageDir);
            foreach (MonoBehaviour behaviour in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                SerializedObject so = new SerializedObject(behaviour);
                bool changed = false;
                changed |= SetBoolIfExists(so, "startOnAwake", enabled);
                changed |= SetBoolIfExists(so, "startOnStart", enabled);
                changed |= SetBoolIfExists(so, "startIntroQuestOnStart", enabled);
                changed |= SetBoolIfExists(so, "pulseAutomatically", enabled);
                changed |= SetBoolIfExists(so, "autoRotate", enabled);
                if (changed)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(behaviour);
                }
            }
        }

        private static bool SetBoolIfExists(SerializedObject so, string propertyName, bool value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null || prop.propertyType != SerializedPropertyType.Boolean)
            {
                return false;
            }

            prop.boolValue = value;
            return true;
        }

        private static void Check(StringBuilder report, string label, bool ok, string fix)
        {
            report.AppendLine($"- {(ok ? "OK" : "MISSING")} — **{label}**");
            if (!ok)
            {
                report.AppendLine($"  - Fix: {fix}");
            }
        }

        private static GameObject FindPlayer()
        {
            try
            {
                return GameObject.FindGameObjectWithTag("Player");
            }
            catch
            {
                return GameObject.Find("Player Test Rig");
            }
        }

        private static int Count(string text, char value)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c == value)
                {
                    count++;
                }
            }
            return count;
        }

        private static void Safe(string label, Action action)
        {
            try
            {
                action?.Invoke();
                Debug.Log("Triage step OK: " + label);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Triage step failed: " + label + "\n" + ex.Message);
            }
        }
    }
}
#endif
