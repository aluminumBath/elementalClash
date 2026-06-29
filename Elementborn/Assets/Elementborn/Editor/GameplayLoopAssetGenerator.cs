#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class GameplayLoopAssetGenerator
    {
        private const string WaveDir = "Assets/Elementborn/Generated/GameplayLoop/Waves";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/GameplayLoop";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/Gameplay Loop/Generate Gameplay Loop Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(WaveDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            GameObject enemyPrefab = CreateEnemyPrefab();
            ElementbornSpawnWaveDefinition introWave = CreateWave("intro_fire_capital_warmup", "Fire Capital Warmup Wave",
                "A small starter wave near the Fire Capital proving the spawn flow works.", enemyPrefab, ElementbornSpawnRole.EnemyWave, 3, 4f);

            CreateDirectorPrefab(introWave, enemyPrefab);
            WriteReport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated gameplay loop assets.");
        }

        [MenuItem("Elementborn/Gameplay Loop/Install Gameplay Loop In Open Scene")]
        public static void InstallGameplayLoopInOpenScene()
        {
            GenerateAll();
            InstallSpawnPoints();

            GameObject root = GameObject.Find("Elementborn Main Gameplay Loop");
            if (root == null)
            {
                root = new GameObject("Elementborn Main Gameplay Loop");
            }

            var director = root.GetComponent<ElementbornMainGameplayLoopDirector>();
            if (director == null)
            {
                director = root.AddComponent<ElementbornMainGameplayLoopDirector>();
            }

            var registry = root.GetComponent<ElementbornSpawnRegistry>();
            if (registry == null)
            {
                registry = root.AddComponent<ElementbornSpawnRegistry>();
            }

            var so = new SerializedObject(director);
            so.FindProperty("startOnAwake").boolValue = true;
            so.FindProperty("registerSystemsOnStart").boolValue = true;
            so.FindProperty("startIntroQuestOnStart").boolValue = true;
            so.FindProperty("introQuest").objectReferenceValue = LoadIntroQuest();
            so.FindProperty("fallbackEnemyPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Elementborn_BasicEnemySpawn.prefab");

            var waves = so.FindProperty("starterWaves");
            waves.arraySize = 1;
            waves.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ElementbornSpawnWaveDefinition>($"{WaveDir}/intro_fire_capital_warmup.asset");
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(root);
            Debug.Log("Installed gameplay loop in open scene.");
        }

        [MenuItem("Elementborn/Gameplay Loop/Install Spawn Points In Open Scene")]
        public static void InstallSpawnPoints()
        {
            GameObject parent = GameObject.Find("Gameplay Spawn Points");
            if (parent == null)
            {
                parent = new GameObject("Gameplay Spawn Points");
            }

            CreateSpawn(parent.transform, "PlayerStart_Central", ElementbornSpawnRole.PlayerStart, new Vector3(0f, 1f, -8f));
            CreateSpawn(parent.transform, "FireCapitalStart", ElementbornSpawnRole.FireCapitalStart, new Vector3(-70f, 1f, -58f));
            CreateSpawn(parent.transform, "FireEnemyWave_A", ElementbornSpawnRole.EnemyWave, new Vector3(-52f, 1f, -35f));
            CreateSpawn(parent.transform, "FireEnemyWave_B", ElementbornSpawnRole.EnemyWave, new Vector3(-84f, 1f, -35f));
            CreateSpawn(parent.transform, "OrphanageSpawn", ElementbornSpawnRole.CreatureOrphanage, new Vector3(-8f, 1f, -13f));
            CreateSpawn(parent.transform, "WolfPackSpawn", ElementbornSpawnRole.WolfPack, new Vector3(22f, 1f, -18f));
            CreateSpawn(parent.transform, "BoatSpawn", ElementbornSpawnRole.Boat, new Vector3(-24f, 1f, 24f));
            EditorUtility.SetDirty(parent);
        }

        private static GameObject CreateEnemyPrefab()
        {
            string path = $"{PrefabDir}/Elementborn_BasicEnemySpawn.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Elementborn_BasicEnemySpawn";
            enemy.transform.localScale = new Vector3(1f, 1.4f, 1f);
            enemy.AddComponent<SimpleCombatHealth>();
            enemy.AddComponent<TestEnemySpawnPoint>();
            PrefabUtility.SaveAsPrefabAsset(enemy, path);
            Object.DestroyImmediate(enemy);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static ElementbornSpawnWaveDefinition CreateWave(string id, string title, string summary, GameObject prefab, ElementbornSpawnRole role, int count, float radius)
        {
            string path = $"{WaveDir}/{id}.asset";
            var wave = AssetDatabase.LoadAssetAtPath<ElementbornSpawnWaveDefinition>(path);
            if (wave == null)
            {
                wave = ScriptableObject.CreateInstance<ElementbornSpawnWaveDefinition>();
                AssetDatabase.CreateAsset(wave, path);
            }

            var so = new SerializedObject(wave);
            so.FindProperty("waveId").stringValue = id;
            so.FindProperty("displayName").stringValue = title;
            so.FindProperty("summary").stringValue = summary;
            var entries = so.FindProperty("entries");
            entries.arraySize = 1;
            var entry = entries.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("EntryId").stringValue = id + "_enemy";
            entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
            entry.FindPropertyRelative("SpawnRole").enumValueIndex = (int)role;
            entry.FindPropertyRelative("Count").intValue = count;
            entry.FindPropertyRelative("Radius").floatValue = radius;
            entry.FindPropertyRelative("ParentToDirector").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(wave);
            return wave;
        }

        private static void CreateDirectorPrefab(ElementbornSpawnWaveDefinition wave, GameObject enemyPrefab)
        {
            GameObject go = new GameObject("Elementborn_MainGameplayLoopDirector");
            var director = go.AddComponent<ElementbornMainGameplayLoopDirector>();
            var registry = go.AddComponent<ElementbornSpawnRegistry>();

            var so = new SerializedObject(director);
            so.FindProperty("startOnAwake").boolValue = true;
            so.FindProperty("registerSystemsOnStart").boolValue = true;
            so.FindProperty("startIntroQuestOnStart").boolValue = true;
            so.FindProperty("introQuest").objectReferenceValue = LoadIntroQuest();
            so.FindProperty("fallbackEnemyPrefab").objectReferenceValue = enemyPrefab;
            var waves = so.FindProperty("starterWaves");
            waves.arraySize = 1;
            waves.GetArrayElementAtIndex(0).objectReferenceValue = wave;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_MainGameplayLoopDirector.prefab");
            Object.DestroyImmediate(go);
        }

        private static void CreateSpawn(Transform parent, string id, ElementbornSpawnRole role, Vector3 position)
        {
            GameObject existing = GameObject.Find(id);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject go = new GameObject(id);
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;
            var point = go.AddComponent<ElementbornSpawnPoint>();
            point.Configure(id, role);
        }


        private static QuestUiDefinition LoadIntroQuest()
        {
            QuestUiDefinition onboarding = AssetDatabase.LoadAssetAtPath<QuestUiDefinition>("Assets/Elementborn/Generated/QuestUI/Playtest/playtest_onboarding_route.asset");
            if (onboarding != null)
            {
                return onboarding;
            }

            return AssetDatabase.LoadAssetAtPath<QuestUiDefinition>("Assets/Elementborn/Generated/QuestUI/FireCapital/fire_caldera_audience.asset");
        }

        private static void WriteReport()
        {
            File.WriteAllText($"{ReportDir}/GameplayLoopReport.md",
@"# Gameplay Loop Report

Generated by v46.

## Loop states

```text
NotStarted
Bootstrapping
Intro
Explore
Encounter
Questing
Recovery
Completed
```

## Generated assets

```text
Assets/Elementborn/Generated/GameplayLoop/Waves/intro_fire_capital_warmup.asset
Assets/Elementborn/Generated/Prefabs/GameplayLoop/Elementborn_MainGameplayLoopDirector.prefab
Assets/Elementborn/Generated/Prefabs/GameplayLoop/Elementborn_BasicEnemySpawn.prefab
```

## Menus

```text
Elementborn → Gameplay Loop → Generate Gameplay Loop Assets
Elementborn → Gameplay Loop → Install Gameplay Loop In Open Scene
Elementborn → Gameplay Loop → Install Spawn Points In Open Scene
```
");
        }
    }
}
#endif
