#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StoryEncounterPrefabSetupMenu
    {
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/StoryEncounters/Placeholders";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/Story Encounters/Create Placeholder Encounter Prefabs")]
        public static void CreatePlaceholderEncounterPrefabs()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            CreateDonowlPrefab();
            CreateJudgePrefab();
            CreateWolfPackPrefab();
            CreateMichellePrefab();
            CreateOrphanagePrefab();
            WriteReport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created placeholder encounter prefabs.");
        }

        [MenuItem("Elementborn/Story Encounters/Install Placeholder Encounter Objects In Open Scene")]
        public static void InstallPlaceholderEncounterObjects()
        {
            CreatePlaceholderEncounterPrefabs();
            InstantiatePrefab("Elementborn_DonowlPlaceholder.prefab", new Vector3(18f, 0f, 44f));
            InstantiatePrefab("Elementborn_TheJudgePlaceholder.prefab", new Vector3(490f, 20f, 260f));
            InstantiatePrefab("Elementborn_RomilusMadrangeaPackPlaceholder.prefab", new Vector3(-260f, 0f, 310f));
            InstantiatePrefab("Elementborn_MichelleRaindancerPlaceholder.prefab", new Vector3(730f, 25f, 418f));
            InstantiatePrefab("Elementborn_CrabSignCreatureOrphanagePlaceholder.prefab", new Vector3(210f, 0f, 284f));
            Debug.Log("Installed placeholder encounter objects in open scene.");
        }

        private static void CreateDonowlPrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Donowl Placeholder";
            root.transform.localScale = new Vector3(2.5f, 3.2f, 2.5f);
            root.AddComponent<DistractedSleeperMonsterController>();
            root.AddComponent<StoryEncounterProgressAdminCommandBridge>();
            SavePrefab(root, "Elementborn_DonowlPlaceholder.prefab");
        }

        private static void CreateJudgePrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "The Judge Placeholder";
            root.transform.localScale = new Vector3(4f, 2.8f, 7f);
            root.AddComponent<ThunderVoiceTerritoryGuardian>();
            SavePrefab(root, "Elementborn_TheJudgePlaceholder.prefab");
        }

        private static void CreateWolfPackPrefab()
        {
            GameObject root = new GameObject("Romilus Madrangea Pack Placeholder");
            var controller = root.AddComponent<TimedDualLeaderPackRespawnController>();

            GameObject romilus = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            romilus.name = "Romilus Placeholder";
            romilus.transform.SetParent(root.transform, false);
            romilus.transform.localPosition = new Vector3(-2f, 0f, 0f);
            var romilusNotifier = romilus.AddComponent<PackLeaderDefeatNotifier>();

            GameObject madrangea = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            madrangea.name = "Madrangea Placeholder";
            madrangea.transform.SetParent(root.transform, false);
            madrangea.transform.localPosition = new Vector3(2f, 0f, 0f);
            var madrangeaNotifier = madrangea.AddComponent<PackLeaderDefeatNotifier>();

            var members = new List<GameObject>();
            for (int i = 0; i < 4; i++)
            {
                GameObject wolf = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                wolf.name = "Wolf Pack Member Placeholder " + (i + 1);
                wolf.transform.SetParent(root.transform, false);
                wolf.transform.localPosition = new Vector3(-3f + i * 2f, 0f, 4f);
                members.Add(wolf);
            }

            var so = new SerializedObject(controller);
            so.FindProperty("leaderA").objectReferenceValue = romilus;
            so.FindProperty("leaderB").objectReferenceValue = madrangea;
            var packMembers = so.FindProperty("packMembers");
            packMembers.arraySize = members.Count;
            for (int i = 0; i < members.Count; i++)
            {
                packMembers.GetArrayElementAtIndex(i).objectReferenceValue = members[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            var n1 = new SerializedObject(romilusNotifier);
            n1.FindProperty("packController").objectReferenceValue = controller;
            n1.FindProperty("leaderId").stringValue = "romilus";
            n1.ApplyModifiedPropertiesWithoutUndo();

            var n2 = new SerializedObject(madrangeaNotifier);
            n2.FindProperty("packController").objectReferenceValue = controller;
            n2.FindProperty("leaderId").stringValue = "madrangea";
            n2.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, "Elementborn_RomilusMadrangeaPackPlaceholder.prefab");
        }

        private static void CreateMichellePrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Michelle Raindancer Placeholder";
            root.transform.localScale = new Vector3(1.2f, 2.2f, 1.2f);
            root.AddComponent<StoryEncounterProgressAdminCommandBridge>();
            SavePrefab(root, "Elementborn_MichelleRaindancerPlaceholder.prefab");
        }

        private static void CreateOrphanagePrefab()
        {
            GameObject root = new GameObject("Crab-Sign Creature Orphanage Placeholder");
            root.AddComponent<CreatureOrphanageHealingService>();
            root.AddComponent<CreatureOrphanageInteractable>();
            root.AddComponent<CreatureOrphanageRecoveryRegistry>();
            root.AddComponent<CreatureOrphanageRecoveryInteractable>();
            root.AddComponent<CreatureOrphanageRecoveryAdminCommandBridge>();
            root.AddComponent<CreatureOrphanageRecoverySaveBridge>();

            GameObject crabSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crabSign.name = "Crab Symbol Sign Placeholder";
            crabSign.transform.SetParent(root.transform, false);
            crabSign.transform.localPosition = new Vector3(0f, 2f, 0f);
            crabSign.transform.localScale = new Vector3(2.5f, 1f, 0.2f);

            SavePrefab(root, "Elementborn_CrabSignCreatureOrphanagePlaceholder.prefab");
        }

        private static void InstantiatePrefab(string fileName, Vector3 position)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{fileName}");
            if (prefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                instance.transform.position = position;
            }
        }

        private static void SavePrefab(GameObject root, string fileName)
        {
            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/{fileName}");
            Object.DestroyImmediate(root);
        }

        private static void WriteReport()
        {
            File.WriteAllText($"{ReportDir}/StoryEncounterPrefabSetupReport.md",
@"# Story Encounter Prefab Setup Report

Generated by v40.

## Prefabs

```text
Elementborn_DonowlPlaceholder.prefab
Elementborn_TheJudgePlaceholder.prefab
Elementborn_RomilusMadrangeaPackPlaceholder.prefab
Elementborn_MichelleRaindancerPlaceholder.prefab
Elementborn_CrabSignCreatureOrphanagePlaceholder.prefab
```

## Menus

```text
Elementborn → Story Encounters → Create Placeholder Encounter Prefabs
Elementborn → Story Encounters → Install Placeholder Encounter Objects In Open Scene
```
");
        }
    }
}
#endif
