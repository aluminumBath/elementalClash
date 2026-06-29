#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class SocialGroupAssetGenerator
    {
        private const string GroupDir = "Assets/Elementborn/Generated/SocialNPC/Groups";
        private const string EventDir = "Assets/Elementborn/Generated/SocialNPC/GroupEvents";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/SocialNPC";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";
        private const string NpcDir = "Assets/Elementborn/Generated/NPC/WorldEntries";

        [MenuItem("Elementborn/Social NPCs/Generate Social Group Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(GroupDir);
            Directory.CreateDirectory(EventDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            SocialNpcQuestAndPrefabGenerator.GenerateQuestsAndDialogue();

            var lowerTerrace = CreateGroup(
                "wind_lower_terrace_circle",
                "Wind Lower Terrace Social Circle",
                CapitalId.WindCapital,
                new Vector3(746f, 22f, 438f),
                "Rekr, Manon, Marie, Amy, and Johna form a messy, funny, rumor-rich lower-terrace social circle.",
                new[] {
                    ("rekr_ap", "sickly lava-channeler husband", new Vector3(742f, 21f, 436f), "coughs lava-smoke and tries to avoid medicine"),
                    ("manon", "cleanliness enforcer", new Vector3(744f, 21f, 438f), "restores order with terrifying precision"),
                    ("marie_conflag", "sleepy fire hazard", new Vector3(748f, 22f, 440f), "flirts, dozes, and accidentally lights things"),
                    ("amy_whine", "rumor drifter", new Vector3(750f, 22f, 442f), "wanders through conversations and accidentally hears useful clues"),
                    ("johna_rold", "pipe-smoking advisor", new Vector3(752f, 26f, 430f), "quietly gives advice that steadies people")
                });

            CreateEvent("wind_lower_hangout", "Lower Terrace Hangout", SocialGroupEventType.Hangout, lowerTerrace, null, CapitalPressureType.Unrest, -1,
                "The lower-terrace circle gathers, somehow producing both comfort and confusion.",
                "Good ambient scene for introducing the group.");

            CreateEvent("marie_sleeping_flare_social", "Marie Falls Asleep On Fire Again", SocialGroupEventType.AccidentalFire, lowerTerrace, LoadQuest("marie_accidental_conflagration"), CapitalPressureType.Unrest, 5,
                "Marie nods off and starts another small fire while insisting everyone is overreacting.",
                "Use as a repeatable local crisis.");

            CreateEvent("amy_rumor_drift_social", "Amy Misremembers A Useful Rumor", SocialGroupEventType.RumorDrift, lowerTerrace, LoadQuest("amy_goes_with_it"), CapitalPressureType.HiddenThreat, 4,
                "Amy wanders through gossip and accidentally remembers one useful clue.",
                "Can lead toward Sarah, Redbeard, or neighborhood secrets.");

            CreateEvent("johna_pipe_counsel_social", "Johna's Pipe Counsel", SocialGroupEventType.AdviceSession, lowerTerrace, LoadQuest("johna_pipe_counsel"), CapitalPressureType.Unrest, -4,
                "Johna's quiet advice calms the lower terraces.",
                "Use as a low-stakes stabilizer event.");

            CreateEvent("manon_cleanup_crisis_social", "Manon's Cleanup Crisis", SocialGroupEventType.CleanupCrisis, lowerTerrace, LoadQuest("manon_immaculate_crisis"), CapitalPressureType.Unrest, -3,
                "Manon turns domestic disaster cleanup into an organized operation.",
                "Good comic cleanup objective.");

            var fireWatch = CreateGroup(
                "fire_basalt_row_watch",
                "Basalt Row Neighborhood Watch",
                CapitalId.FireCapital,
                new Vector3(526f, 0f, -198f),
                "Kelly protects Fire Capital neighbors while her flame-colored hair reveals her mood.",
                new[] {
                    ("kelly", "moodflame protector", new Vector3(526f, 0f, -198f), "protects neighbors and enjoys mischievous justice"),
                    ("marie_conflag", "visiting fire hazard", new Vector3(528f, 0f, -196f), "occasionally brings fire chaos back home")
                });

            CreateEvent("kelly_moodflame_watch_social", "Kelly's Moodflame Watch", SocialGroupEventType.NeighborhoodProtection, fireWatch, LoadQuest("kelly_moodflame_watch"), CapitalPressureType.Unrest, -5,
                "Kelly steps between trouble and her neighbors while her hair burns protective blue-orange.",
                "Use as a recurring Fire Capital protector beat.");

            CreatePrefab();
            WriteReport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated social group assets.");
        }

        [MenuItem("Elementborn/Social NPCs/Install Social Group Registry In Open Scene")]
        public static void InstallRegistry()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Elementborn Social Group Registry");
            if (root == null)
            {
                root = new GameObject("Elementborn Social Group Registry");
            }

            var registry = root.GetComponent<SocialGroupRegistry>();
            if (registry == null)
            {
                registry = root.AddComponent<SocialGroupRegistry>();
            }

            registry.SetData(LoadAllGroups(), LoadAllEvents());

            if (root.GetComponent<SocialGroupScheduleDirector>() == null)
            {
                root.AddComponent<SocialGroupScheduleDirector>();
            }

            if (root.GetComponent<SocialGroupAdminCommandBridge>() == null)
            {
                root.AddComponent<SocialGroupAdminCommandBridge>();
            }

            EditorUtility.SetDirty(root);
            Debug.Log("Installed social group registry in open scene.");
        }

        private static SocialNpcGroupDefinition CreateGroup(string id, string title, CapitalId capital, Vector3 center, string summary, (string npcId, string role, Vector3 position, string ambient)[] members)
        {
            string path = $"{GroupDir}/{id}.asset";
            var group = AssetDatabase.LoadAssetAtPath<SocialNpcGroupDefinition>(path);
            if (group == null)
            {
                group = ScriptableObject.CreateInstance<SocialNpcGroupDefinition>();
                AssetDatabase.CreateAsset(group, path);
            }

            var so = new SerializedObject(group);
            so.FindProperty("groupId").stringValue = id;
            so.FindProperty("displayName").stringValue = title;
            so.FindProperty("primaryCapital").enumValueIndex = (int)capital;
            so.FindProperty("groupCenter").vector3Value = center;
            so.FindProperty("summary").stringValue = summary;

            var memberArray = so.FindProperty("members");
            memberArray.arraySize = members.Length;
            for (int i = 0; i < members.Length; i++)
            {
                var member = memberArray.GetArrayElementAtIndex(i);
                member.FindPropertyRelative("Npc").objectReferenceValue = LoadNpc(members[i].npcId);
                member.FindPropertyRelative("UsualRole").stringValue = members[i].role;
                member.FindPropertyRelative("DefaultPosition").vector3Value = members[i].position;
                member.FindPropertyRelative("AmbientBehavior").stringValue = members[i].ambient;
            }

            var relationArray = so.FindProperty("relationships");
            relationArray.arraySize = 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(group);
            return group;
        }

        private static void CreateEvent(string id, string title, SocialGroupEventType type, SocialNpcGroupDefinition group, QuestUiDefinition quest, CapitalPressureType pressure, int delta, string summary, string notes)
        {
            string path = $"{EventDir}/{id}.asset";
            var evt = AssetDatabase.LoadAssetAtPath<SocialGroupEventDefinition>(path);
            if (evt == null)
            {
                evt = ScriptableObject.CreateInstance<SocialGroupEventDefinition>();
                AssetDatabase.CreateAsset(evt, path);
            }

            var so = new SerializedObject(evt);
            so.FindProperty("eventId").stringValue = id;
            so.FindProperty("displayName").stringValue = title;
            so.FindProperty("eventType").enumValueIndex = (int)type;
            so.FindProperty("group").objectReferenceValue = group;
            so.FindProperty("questToStart").objectReferenceValue = quest;
            so.FindProperty("pressureType").enumValueIndex = (int)pressure;
            so.FindProperty("pressureDelta").intValue = delta;
            so.FindProperty("createJournalEntry").boolValue = true;
            so.FindProperty("createMapMarker").boolValue = true;
            so.FindProperty("playerFacingSummary").stringValue = summary;
            so.FindProperty("directorNotes").stringValue = notes;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(evt);
        }

        private static void CreatePrefab()
        {
            GameObject go = new GameObject("Elementborn_SocialGroupRegistry");
            var registry = go.AddComponent<SocialGroupRegistry>();
            registry.SetData(LoadAllGroups(), LoadAllEvents());
            go.AddComponent<SocialGroupScheduleDirector>();
            go.AddComponent<SocialGroupAdminCommandBridge>();
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_SocialGroupRegistry.prefab");
            Object.DestroyImmediate(go);
        }

        private static QuestUiDefinition LoadQuest(string id)
        {
            return AssetDatabase.LoadAssetAtPath<QuestUiDefinition>($"Assets/Elementborn/Generated/QuestUI/SocialNPC/{id}.asset");
        }

        private static NpcWorldEntryDefinition LoadNpc(string id)
        {
            return AssetDatabase.LoadAssetAtPath<NpcWorldEntryDefinition>($"{NpcDir}/{id}.asset");
        }

        private static List<SocialNpcGroupDefinition> LoadAllGroups()
        {
            var results = new List<SocialNpcGroupDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:SocialNpcGroupDefinition", new[] { GroupDir }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<SocialNpcGroupDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                {
                    results.Add(asset);
                }
            }
            return results;
        }

        private static List<SocialGroupEventDefinition> LoadAllEvents()
        {
            var results = new List<SocialGroupEventDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:SocialGroupEventDefinition", new[] { EventDir }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<SocialGroupEventDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                {
                    results.Add(asset);
                }
            }
            return results;
        }

        private static void WriteReport()
        {
            File.WriteAllText($"{ReportDir}/SocialGroupReport.md",
@"# Social Group Report

Generated by v43.

## Social groups

```text
Wind Lower Terrace Social Circle
Basalt Row Neighborhood Watch
```

## Events

```text
Lower Terrace Hangout
Marie Falls Asleep On Fire Again
Amy Misremembers A Useful Rumor
Johna's Pipe Counsel
Manon's Cleanup Crisis
Kelly's Moodflame Watch
```

## Runtime commands

```text
socialgroup.summary
socialgroup.register
socialgroup.event eventId
socialgroup.next
```
");
        }
    }
}
#endif
