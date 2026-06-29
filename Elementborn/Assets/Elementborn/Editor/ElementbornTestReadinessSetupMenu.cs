#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornTestReadinessSetupMenu
    {
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/Playtest";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";
        private const string QuestDir = "Assets/Elementborn/Generated/QuestUI/Playtest";

        [MenuItem("Elementborn/Playtest/Run Test Readiness Setup")]
        public static void RunTestReadinessSetup()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);
            Directory.CreateDirectory(QuestDir);

            CreateOnboardingQuest();
            GameplayLoopAssetGenerator.InstallGameplayLoopInOpenScene();
            AdminWristUiSetupMenu.InstallInOpenScene();
            InstallTestHarnessInOpenScene();
            WriteEditorReadinessReport();
            Debug.Log("Elementborn test readiness setup complete.");
        }

        [MenuItem("Elementborn/Playtest/Generate Test Harness Prefab")]
        public static void GenerateTestHarnessPrefab()
        {
            Directory.CreateDirectory(PrefabDir);
            GameObject root = BuildHarnessCanvas();
            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/Elementborn_PlaytestHarness.prefab");
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated Elementborn_PlaytestHarness prefab.");
        }

        [MenuItem("Elementborn/Playtest/Install Test Harness In Open Scene")]
        public static void InstallTestHarnessInOpenScene()
        {
            GenerateTestHarnessPrefab();

            GameObject existing = GameObject.Find("Elementborn Playtest Harness");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Elementborn_PlaytestHarness.prefab");
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate playtest harness prefab.");
                return;
            }

            instance.name = "Elementborn Playtest Harness";
            Undo.RegisterCreatedObjectUndo(instance, "Install playtest harness");

            GameObject systems = GameObject.Find("Elementborn Playtest Systems");
            if (systems == null)
            {
                systems = new GameObject("Elementborn Playtest Systems");
                Undo.RegisterCreatedObjectUndo(systems, "Create playtest systems");
            }

            if (systems.GetComponent<ElementbornTestReadinessScanner>() == null) systems.AddComponent<ElementbornTestReadinessScanner>();
            if (systems.GetComponent<ElementbornPlaytestResetService>() == null) systems.AddComponent<ElementbornPlaytestResetService>();
            if (systems.GetComponent<ElementbornPlaytestPresetService>() == null) systems.AddComponent<ElementbornPlaytestPresetService>();
            if (systems.GetComponent<ElementbornPlaytestHarnessController>() == null) systems.AddComponent<ElementbornPlaytestHarnessController>();

            EditorUtility.SetDirty(instance);
            EditorUtility.SetDirty(systems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Installed playtest harness in open scene.");
        }

        [MenuItem("Elementborn/Playtest/Create Onboarding Quest")]
        public static QuestUiDefinition CreateOnboardingQuest()
        {
            Directory.CreateDirectory(QuestDir);
            string path = $"{QuestDir}/playtest_onboarding_route.asset";
            QuestUiDefinition quest = AssetDatabase.LoadAssetAtPath<QuestUiDefinition>(path);
            if (quest == null)
            {
                quest = ScriptableObject.CreateInstance<QuestUiDefinition>();
                AssetDatabase.CreateAsset(quest, path);
            }

            var so = new SerializedObject(quest);
            so.FindProperty("questId").stringValue = "playtest_onboarding_route";
            so.FindProperty("title").stringValue = "Elementborn Playtest Route";
            so.FindProperty("description").stringValue = "A first-run playtest quest that walks testers through the core generated systems.";
            so.FindProperty("region").stringValue = "Playtest";
            so.FindProperty("giverName").stringValue = "Elementborn Test Harness";
            so.FindProperty("autoTrack").boolValue = true;

            var objectives = so.FindProperty("objectives");
            objectives.arraySize = 5;
            SetObjective(objectives.GetArrayElementAtIndex(0), "visit_fire_capital", "Visit the Fire Capital", "Go to the volcano citadel and trigger the Fire Capital intro hook.", new Vector3(-70f, 1f, -58f));
            SetObjective(objectives.GetArrayElementAtIndex(1), "use_admin_panel", "Open the wrist admin panel", "Press F8 and try a dropdown action or cheat code.", new Vector3(0f, 1f, -8f));
            SetObjective(objectives.GetArrayElementAtIndex(2), "visit_orphanage", "Visit the creature orphanage", "Teleport or walk to the Crab-Sign Creature Orphanage and admit a demo creature.", new Vector3(-8f, 1f, -13f));
            SetObjective(objectives.GetArrayElementAtIndex(3), "test_wolf_pack", "Test the wolf pack", "Teleport to Romilus and Madrangea's pack and try respawn/resolve flows.", new Vector3(22f, 1f, -18f));
            SetObjective(objectives.GetArrayElementAtIndex(4), "write_readiness_report", "Write the readiness report", "Use the Playtest Harness report button to generate a readiness markdown file.", new Vector3(0f, 1f, 10f));

            var rewards = so.FindProperty("rewards");
            rewards.arraySize = 1;
            var reward = rewards.GetArrayElementAtIndex(0);
            reward.FindPropertyRelative("RewardId").stringValue = "playtest_onboarding_reward";
            reward.FindPropertyRelative("DisplayName").stringValue = "Test Readiness Confidence";
            reward.FindPropertyRelative("Quantity").intValue = 1;
            reward.FindPropertyRelative("Currency").intValue = 0;
            reward.FindPropertyRelative("SkillPoints").intValue = 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(quest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return quest;
        }

        [MenuItem("Elementborn/Playtest/Write Editor Test Readiness Report")]
        public static void WriteEditorReadinessReport()
        {
            Directory.CreateDirectory(ReportDir);
            string path = $"{ReportDir}/EditorTestReadinessReport_v48.md";

            int spawnPoints = Object.FindObjectsOfType<ElementbornSpawnPoint>(true).Length;
            int landmarks = Object.FindObjectsOfType<CapitalLandmarkDescriptor>(true).Length;
            bool hasDashboard = Object.FindObjectOfType<StorySystemsDebugDashboard>(true) != null;
            bool hasAdmin = Object.FindObjectOfType<AdminWristPanelView>(true) != null;
            bool hasHarness = Object.FindObjectOfType<ElementbornPlaytestHarnessPanel>(true) != null;
            bool hasLoop = Object.FindObjectOfType<ElementbornMainGameplayLoopDirector>(true) != null;

            File.WriteAllText(path,
$@"# Editor Test Readiness Report v48

## Scene checks

```text
Gameplay loop director: {hasLoop}
Story dashboard: {hasDashboard}
Left wrist admin UI: {hasAdmin}
Playtest harness: {hasHarness}
Spawn points: {spawnPoints}
Capital landmarks: {landmarks}
```

## Required next steps

```text
1. Let Unity compile.
2. Run Elementborn → Playtest → Run Test Readiness Setup.
3. Press Play.
4. Use Playtest Harness buttons.
5. Press F8 for left wrist admin UI.
6. Run EditMode and PlayMode tests.
```

## Expected scene systems

```text
Elementborn Main Gameplay Loop
Elementborn Story Debug Dashboard
Elementborn Left Wrist Admin Panel
Elementborn Playtest Harness
Elementborn Playtest Systems
Capital Landmarks
Gameplay Spawn Points
Fire Capital Systems
```
");
            AssetDatabase.Refresh();
            Debug.Log("Wrote editor test readiness report: " + path);
        }

        [MenuItem("Elementborn/Playtest/Reset Runtime State In Open Scene")]
        public static void ResetRuntimeStateInOpenScene()
        {
            ElementbornPlaytestResetService.Ensure().ResetRuntimeState(deleteSaves: false);
        }

        [MenuItem("Elementborn/Playtest/Delete Playtest Saves")]
        public static void DeletePlaytestSaves()
        {
            ElementbornPlaytestResetService.Ensure().ResetRuntimeState(deleteSaves: true);
        }

        private static void SetObjective(SerializedProperty objective, string id, string title, string description, Vector3 position)
        {
            objective.FindPropertyRelative("ObjectiveId").stringValue = id;
            objective.FindPropertyRelative("Title").stringValue = title;
            objective.FindPropertyRelative("Description").stringValue = description;
            objective.FindPropertyRelative("WorldPosition").vector3Value = position;
            objective.FindPropertyRelative("CreateWaypoint").boolValue = true;
            objective.FindPropertyRelative("Required").boolValue = true;
        }

        private static GameObject BuildHarnessCanvas()
        {
            GameObject root = new GameObject("Elementborn_PlaytestHarness");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.AddComponent<GraphicRaycaster>();

            ElementbornPlaytestHarnessPanel panel = root.AddComponent<ElementbornPlaytestHarnessPanel>();

            GameObject box = Panel(root.transform, "Harness Panel", new Vector2(0.665f, 0.04f), new Vector2(0.98f, 0.62f), new Color(0.04f, 0.05f, 0.07f, 0.82f));
            Text title = Label(box.transform, "Title", "PLAYTEST HARNESS", 18, new Vector2(12f, -10f), new Vector2(520f, 28f), TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.82f, 0.42f, 1f);

            Button start = Button(box.transform, "Start Loop", "Start Loop", 0, 0);
            Button fire = Button(box.transform, "Teleport Fire", "Teleport Fire", 1, 0);
            Button orphanage = Button(box.transform, "Teleport Orphanage", "Teleport Orphanage", 0, 1);
            Button wolf = Button(box.transform, "Teleport Wolf", "Teleport Wolf", 1, 1);
            Button wave = Button(box.transform, "Spawn Wave", "Spawn Wave", 0, 2);
            Button fireIntro = Button(box.transform, "Fire Intro", "Fire Intro", 1, 2);
            Button social = Button(box.transform, "Social Event", "Social Event", 0, 3);
            Button creature = Button(box.transform, "Admit Creature", "Admit Creature", 1, 3);
            Button save = Button(box.transform, "Save", "Save Slot 0", 0, 4);
            Button load = Button(box.transform, "Load", "Load Slot 0", 1, 4);
            Button resetRuntime = Button(box.transform, "Reset Runtime", "Reset Runtime", 0, 5);
            Button resetSaves = Button(box.transform, "Reset Saves", "Reset Saves", 1, 5);
            Button report = Button(box.transform, "Readiness Report", "Write Report", 0, 6);
            Button stable = Button(box.transform, "Stable Preset", "Stable Fire", 1, 6);
            Button chaos = Button(box.transform, "Chaos Preset", "Fire Chaos", 0, 7);
            Button clean = Button(box.transform, "Clean Preset", "Clean Fresh", 1, 7);

            SetPrivate(panel, "startLoopButton", start);
            SetPrivate(panel, "teleportFireButton", fire);
            SetPrivate(panel, "teleportOrphanageButton", orphanage);
            SetPrivate(panel, "teleportWolfButton", wolf);
            SetPrivate(panel, "spawnWaveButton", wave);
            SetPrivate(panel, "fireIntroButton", fireIntro);
            SetPrivate(panel, "socialEventButton", social);
            SetPrivate(panel, "admitCreatureButton", creature);
            SetPrivate(panel, "saveButton", save);
            SetPrivate(panel, "loadButton", load);
            SetPrivate(panel, "resetRuntimeButton", resetRuntime);
            SetPrivate(panel, "resetSavesButton", resetSaves);
            SetPrivate(panel, "readinessReportButton", report);
            SetPrivate(panel, "stablePresetButton", stable);
            SetPrivate(panel, "chaosPresetButton", chaos);
            SetPrivate(panel, "cleanPresetButton", clean);

            return root;
        }

        private static GameObject Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        private static Text Label(Transform parent, string name, string text, int size, Vector2 position, Vector2 dimensions, TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text label = go.AddComponent<Text>();
            Elementborn.Game.ElementbornBuiltinFontUtility.ApplyDefaultFont(label);
            label.fontSize = size;
            label.color = Color.white;
            label.text = text;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            return label;
        }

        private static Button Button(Transform parent, string name, string text, int column, int row)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.18f, 0.2f, 0.28f, 0.95f);
            Button button = go.AddComponent<Button>();
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(14f + column * 265f, -52f - row * 48f);
            rect.sizeDelta = new Vector2(250f, 38f);
            Label(go.transform, "Label", text, 13, Vector2.zero, rect.sizeDelta, TextAnchor.MiddleCenter);
            return button;
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                if (target is Object obj)
                {
                    EditorUtility.SetDirty(obj);
                }
            }
        }
    }
}
#endif
