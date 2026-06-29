#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StoryEncounterAssetGenerator
    {
        private const string EncounterDir = "Assets/Elementborn/Generated/StoryEncounters";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/StoryEncounters";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";
        private const string NpcDir = "Assets/Elementborn/Generated/NPC/WorldEntries";

        [MenuItem("Elementborn/Story Encounters/Generate New Villain and Orphanage Encounters")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(EncounterDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            NpcRosterCsvImporter.ImportAllRosters();
            NpcWorldIntegrationAssetGenerator.GenerateAll();

            CreateEncounter("donowl_sleepy_owl", "Donowl, the Drowsy Owl Monster", StoryEncounterThreatType.MonsterVillain, CapitalId.NeutralCentralCity, "donowl", null, new Vector3(18f, 0f, 44f),
                "Donowl is a very strong but confused owl monster who falls asleep and gets distracted at terrible times.",
                "Use DistractedSleeperMonsterController to make Donowl sleep/distract during encounters.",
                "Comedic villain encounter with real combat danger.");

            CreateEncounter("the_judge_dragon", "The Judge, Neon Pink Dragon", StoryEncounterThreatType.DragonVillain, CapitalId.FireCapital, "the_judge", null, new Vector3(490f, 20f, 260f),
                "The Judge is a harsh neon pink steam-channeler dragon who protects her area. Her voice cracks like thunder.",
                "Use ThunderVoiceTerritoryGuardian for warning/punishment behavior.",
                "Territory boss or regional guardian.");

            CreateEncounter("romilus_madrangea_pack", "Romilus and Madrangea's Wolf Pack", StoryEncounterThreatType.WolfPack, CapitalId.NeutralCentralCity, "romilus", "madrangea", new Vector3(-260f, 0f, 310f),
                "A villain wolf-like pack patrols its territory. The pack respawns unless Romilus and Madrangea are both defeated within five minutes.",
                "Use TimedDualLeaderPackRespawnController with PackLeaderDefeatNotifier on both leaders.",
                "Timed dual-leader pack mechanic.");

            CreateEncounter("michelle_raindancer_vizier", "Michelle Raindancer, Rule-Bound Blood Vizier", StoryEncounterThreatType.BloodVizier, CapitalId.WindCapital, "michelle_raindancer", null, new Vector3(730f, 25f, 418f),
                "Michelle Raindancer is a blood-channeler lieutenant/vizier who believes in rules and destroys anyone who breaks them.",
                "Tie her to capital law, theocracy, or harsh rule-enforcement quests.",
                "Excellent lieutenant under Redbeard/Lizkota or another authoritarian capital.");

            CreateEncounter("crab_sign_creature_orphanage", "Crab-Sign Creature Orphanage", StoryEncounterThreatType.CreatureOrphanage, CapitalId.NerithaReefwood, "ella", "eloc", new Vector3(210f, 0f, 284f),
                "Ella and Eloc run a hilarious, loving creature orphanage marked by a crab symbol. They heal creatures the player brings them.",
                "Use CreatureOrphanageHealingService and CreatureOrphanageInteractable.",
                "Friendly environmental non-channeler hub.");

            CreatePrefab();
            WriteReport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated story encounter assets.");
        }

        [MenuItem("Elementborn/Story Encounters/Install Story Encounter Registry In Open Scene")]
        public static void InstallRegistry()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Elementborn Story Encounter Registry");
            if (root == null)
            {
                root = new GameObject("Elementborn Story Encounter Registry");
            }

            var registry = root.GetComponent<StoryEncounterRegistry>();
            if (registry == null)
            {
                registry = root.AddComponent<StoryEncounterRegistry>();
            }

            registry.SetEncounters(LoadAllEncounters());

            if (root.GetComponent<StoryEncounterAdminCommandBridge>() == null)
            {
                root.AddComponent<StoryEncounterAdminCommandBridge>();
            }

            EditorUtility.SetDirty(root);
            Debug.Log("Installed Story Encounter Registry.");
        }

        private static void CreateEncounter(string id, string name, StoryEncounterThreatType type, CapitalId capital, string primaryNpcId, string secondaryNpcId, Vector3 position, string summary, string mechanics, string notes)
        {
            string path = $"{EncounterDir}/{id}.asset";
            var encounter = AssetDatabase.LoadAssetAtPath<StoryEncounterDefinition>(path);
            if (encounter == null)
            {
                encounter = ScriptableObject.CreateInstance<StoryEncounterDefinition>();
                AssetDatabase.CreateAsset(encounter, path);
            }

            var so = new SerializedObject(encounter);
            so.FindProperty("encounterId").stringValue = id;
            so.FindProperty("displayName").stringValue = name;
            so.FindProperty("threatType").enumValueIndex = (int)type;
            so.FindProperty("relatedCapital").enumValueIndex = (int)capital;
            so.FindProperty("primaryNpc").objectReferenceValue = LoadNpc(primaryNpcId);
            so.FindProperty("secondaryNpc").objectReferenceValue = string.IsNullOrWhiteSpace(secondaryNpcId) ? null : LoadNpc(secondaryNpcId);
            so.FindProperty("worldPosition").vector3Value = position;
            so.FindProperty("playerFacingSummary").stringValue = summary;
            so.FindProperty("mechanicsNotes").stringValue = mechanics;
            so.FindProperty("directorNotes").stringValue = notes;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(encounter);
        }

        private static void CreatePrefab()
        {
            GameObject go = new GameObject("Elementborn_StoryEncounterRegistry");
            var registry = go.AddComponent<StoryEncounterRegistry>();
            registry.SetEncounters(LoadAllEncounters());
            go.AddComponent<StoryEncounterAdminCommandBridge>();
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_StoryEncounterRegistry.prefab");
            Object.DestroyImmediate(go);
        }

        private static List<StoryEncounterDefinition> LoadAllEncounters()
        {
            var results = new List<StoryEncounterDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:StoryEncounterDefinition", new[] { EncounterDir }))
            {
                var item = AssetDatabase.LoadAssetAtPath<StoryEncounterDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null)
                {
                    results.Add(item);
                }
            }
            return results;
        }

        private static NpcWorldEntryDefinition LoadNpc(string id)
        {
            return AssetDatabase.LoadAssetAtPath<NpcWorldEntryDefinition>($"{NpcDir}/{id}.asset");
        }

        private static void WriteReport()
        {
            File.WriteAllText($"{ReportDir}/StoryEncounterReport.md",
@"# Story Encounter Report

Generated by v38.

## Added concepts

```text
Donowl
The Judge
Romilus and Madrangea's wolf pack
Michelle Raindancer
Ella and Eloc's Crab-Sign Creature Orphanage
```

## Runtime commands

```text
encounter.summary
encounter.register
pack.defeat romilus
pack.defeat madrangea
pack.respawn
orphanage.heal
donowl.sleep
donowl.distract
judge.warn
```
");
        }
    }
}
#endif
