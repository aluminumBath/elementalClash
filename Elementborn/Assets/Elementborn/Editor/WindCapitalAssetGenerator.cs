#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class WindCapitalAssetGenerator
    {
        private const string HookDir = "Assets/Elementborn/Generated/WindCapital/Hooks";
        private const string QuestDir = "Assets/Elementborn/Generated/QuestUI/WindCapital";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/WindCapital";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";
        private const string NpcDir = "Assets/Elementborn/Generated/NPC/WorldEntries";

        [MenuItem("Elementborn/Wind Capital/Generate Wind Capital Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(HookDir);
            Directory.CreateDirectory(QuestDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            NpcRosterCsvImporter.ImportAllRosters();
            NpcWorldIntegrationAssetGenerator.GenerateAll();

            QuestUiDefinition sarahQuest = CreateQuest(
                "sarah_wind_silence",
                "The Wind Sarah Won't Name",
                "Sarah refuses to speak of the Wind Capital. Ramón knows the truth, and the new theocracy may be the reason she fled.",
                "First Mate Sarah",
                new[]
                {
                    ("ask_ramon", "Ask Ramón what he knows", "Ramón knows what happened to Sarah, but he may not betray her trust easily.", new Vector3(265f, 0f, 338f)),
                    ("speak_to_sarah", "Speak with Sarah after the deck quiets", "Sarah may only answer if the player has earned her trust.", new Vector3(268f, 0f, 342f)),
                    ("follow_the_wind_rumor", "Follow the rumor to the Wind Capital", "A pilgrim recognizes Sarah's old wind-reading technique.", new Vector3(715f, 22f, 410f))
                });

            QuestUiDefinition redbeardQuest = CreateQuest(
                "the_chaotic_aerie",
                "The Chaotic Aerie",
                "High Priest Redbeard and Priestess Lizkota rule a recently usurped Wind Capital, while infant Ruth's steam channeling is being treated as a divine omen.",
                "High Priest Redbeard",
                new[]
                {
                    ("enter_cathedral", "Enter the High Aerie Cathedral", "Witness the fervor surrounding Redbeard's new rule.", new Vector3(720f, 25f, 420f)),
                    ("hear_lizkota", "Hear Lizkota's quiet orders", "Lizkota may be the strategist behind the new theocracy.", new Vector3(724f, 25f, 424f)),
                    ("learn_about_ruth", "Learn why Ruth matters", "The infant's steam channeling is fueling the capital's religious chaos.", new Vector3(726f, 25f, 426f))
                });

            CreateHook("sarah_wind_capital_exile", "The Wind Sarah Won't Name", WindCapitalHookType.SarahPast, WindCapitalDistrict.WhisperingDocks, "first_mate_sarah", "captain_ramon", sarahQuest, new Vector3(715f, 22f, 410f),
                "A Wind Capital pilgrim recognizes Sarah's hidden wind-reading habits from the old regime.",
                "Sarah may have served or protected someone connected to the old Wind Capital leaders before Redbeard's theocracy seized power. Ramón helped her disappear.",
                -5,
                ("rumor_sarah_old_wind", "A sailor says Sarah ties knots in an old Wind Capital prayer pattern, then cuts them apart before anyone sees.", true, "The knots may be a mourning pattern for the old leaders."));

            CreateHook("redbeard_usurpation", "Redbeard's Usurpation", WindCapitalHookType.TheocracyRule, WindCapitalDistrict.HighAerieCathedral, "high_priest_redbeard", "priestess_lizkota", redbeardQuest, new Vector3(720f, 25f, 420f),
                "Redbeard's sermons hold the capital together and tear it apart at the same time.",
                "The theocracy's rise was not bloodless. Several old leaders vanished into exile, prison, or hidden sanctuary.",
                10,
                ("rumor_redbeard_sermon", "The faithful chant Redbeard's name until the skybridges hum.", true, "The chants may be amplified by controlled air-channeling."));

            CreateHook("lizkota_right_hand", "Lizkota's Quiet Hand", WindCapitalHookType.TheocracyRule, WindCapitalDistrict.BellTowerQuarter, "priestess_lizkota", "high_priest_redbeard", null, new Vector3(724f, 25f, 424f),
                "Lizkota speaks softly, but guards move before Redbeard finishes a sentence.",
                "Lizkota may be the true architect of the new religious order and the one tracking exiles.",
                4,
                ("rumor_lizkota_orders", "A bell-tower guard says Lizkota never raises her voice twice.", true, "She uses coded prayer ribbons to move loyalists."));

            CreateHook("ruth_steam_omen", "Ruth, the Steam Omen", WindCapitalHookType.InfantSteamOmen, WindCapitalDistrict.NurserySanctum, "infant_ruth", "priestess_lizkota", redbeardQuest, new Vector3(726f, 25f, 426f),
                "Infant Ruth's steam channeling is being proclaimed a divine sign.",
                "Ruth is not proof of Redbeard's divine mandate; she is a rare steam channeler whose existence is being politically weaponized.",
                12,
                ("rumor_ruth_mist", "When the infant cries, warm mist beads on the cathedral glass.", true, "The steam is real, but the sermons around it are carefully staged."));

            CreateRegistryPrefab(sarahQuest);
            WriteReport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated Wind Capital assets.");
        }

        [MenuItem("Elementborn/Wind Capital/Install Wind Capital Systems In Open Scene")]
        public static void InstallSystems()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Elementborn Wind Capital Systems");
            if (root == null)
            {
                root = new GameObject("Elementborn Wind Capital Systems");
            }

            var registry = root.GetComponent<WindCapitalRegistry>();
            if (registry == null)
            {
                registry = root.AddComponent<WindCapitalRegistry>();
            }

            registry.SetHooks(LoadAllHooks());

            if (root.GetComponent<ReligiousFervorTracker>() == null)
            {
                root.AddComponent<ReligiousFervorTracker>();
            }

            if (root.GetComponent<WindCapitalSecretTracker>() == null)
            {
                root.AddComponent<WindCapitalSecretTracker>();
            }

            if (root.GetComponent<WindCapitalAdminCommandBridge>() == null)
            {
                root.AddComponent<WindCapitalAdminCommandBridge>();
            }

            var bridge = root.GetComponent<SarahPastQuestBridge>();
            if (bridge == null)
            {
                bridge = root.AddComponent<SarahPastQuestBridge>();
            }

            bridge.SetData(LoadNpc("first_mate_sarah"), LoadNpc("captain_ramon"), AssetDatabase.LoadAssetAtPath<QuestUiDefinition>($"{QuestDir}/sarah_wind_silence.asset"));

            EditorUtility.SetDirty(root);
            Debug.Log("Installed Wind Capital systems in open scene.");
        }

        private static void CreateHook(string id, string title, WindCapitalHookType type, WindCapitalDistrict district, string primaryNpcId, string secondaryNpcId, QuestUiDefinition quest, Vector3 position, string summary, string secret, int fervorDelta, (string id, string text, bool publicKnown, string hiddenTruth) rumor)
        {
            string path = $"{HookDir}/{id}.asset";
            var hook = AssetDatabase.LoadAssetAtPath<WindCapitalIntrigueHookDefinition>(path);
            if (hook == null)
            {
                hook = ScriptableObject.CreateInstance<WindCapitalIntrigueHookDefinition>();
                AssetDatabase.CreateAsset(hook, path);
            }

            var so = new SerializedObject(hook);
            so.FindProperty("hookId").stringValue = id;
            so.FindProperty("title").stringValue = title;
            so.FindProperty("hookType").enumValueIndex = (int)type;
            so.FindProperty("district").enumValueIndex = (int)district;
            so.FindProperty("primaryNpc").objectReferenceValue = LoadNpc(primaryNpcId);
            so.FindProperty("secondaryNpc").objectReferenceValue = string.IsNullOrWhiteSpace(secondaryNpcId) ? null : LoadNpc(secondaryNpcId);
            so.FindProperty("quest").objectReferenceValue = quest;
            so.FindProperty("worldPosition").vector3Value = position;
            so.FindProperty("summary").stringValue = summary;
            so.FindProperty("secretTruth").stringValue = secret;
            so.FindProperty("fervorDelta").intValue = fervorDelta;
            so.FindProperty("createsMapMarker").boolValue = true;

            var rumors = so.FindProperty("rumors");
            rumors.arraySize = 1;
            var r = rumors.GetArrayElementAtIndex(0);
            r.FindPropertyRelative("RumorId").stringValue = rumor.id;
            r.FindPropertyRelative("DisplayText").stringValue = rumor.text;
            r.FindPropertyRelative("HookType").enumValueIndex = (int)type;
            r.FindPropertyRelative("PubliclyKnown").boolValue = rumor.publicKnown;
            r.FindPropertyRelative("HiddenTruth").stringValue = rumor.hiddenTruth;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(hook);
        }

        private static QuestUiDefinition CreateQuest(string id, string title, string description, string giver, (string id, string title, string desc, Vector3 pos)[] objectives)
        {
            string path = $"{QuestDir}/{id}.asset";
            var quest = AssetDatabase.LoadAssetAtPath<QuestUiDefinition>(path);
            if (quest == null)
            {
                quest = ScriptableObject.CreateInstance<QuestUiDefinition>();
                AssetDatabase.CreateAsset(quest, path);
            }

            var so = new SerializedObject(quest);
            so.FindProperty("questId").stringValue = id;
            so.FindProperty("title").stringValue = title;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("region").stringValue = "Wind Capital";
            so.FindProperty("giverName").stringValue = giver;
            so.FindProperty("autoTrack").boolValue = true;

            var objectiveArray = so.FindProperty("objectives");
            objectiveArray.arraySize = objectives.Length;
            for (int i = 0; i < objectives.Length; i++)
            {
                var objective = objectiveArray.GetArrayElementAtIndex(i);
                objective.FindPropertyRelative("ObjectiveId").stringValue = objectives[i].id;
                objective.FindPropertyRelative("Title").stringValue = objectives[i].title;
                objective.FindPropertyRelative("Description").stringValue = objectives[i].desc;
                objective.FindPropertyRelative("WorldPosition").vector3Value = objectives[i].pos;
                objective.FindPropertyRelative("CreateWaypoint").boolValue = true;
                objective.FindPropertyRelative("Required").boolValue = true;
            }

            var rewards = so.FindProperty("rewards");
            rewards.arraySize = 1;
            var reward = rewards.GetArrayElementAtIndex(0);
            reward.FindPropertyRelative("RewardId").stringValue = id + "_reward";
            reward.FindPropertyRelative("DisplayName").stringValue = "Wind Capital Insight";
            reward.FindPropertyRelative("Currency").intValue = 40;
            reward.FindPropertyRelative("SkillPoints").intValue = 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(quest);
            return quest;
        }

        private static void CreateRegistryPrefab(QuestUiDefinition sarahQuest)
        {
            GameObject go = new GameObject("Elementborn_WindCapitalRegistry");
            var registry = go.AddComponent<WindCapitalRegistry>();
            registry.SetHooks(LoadAllHooks());
            go.AddComponent<ReligiousFervorTracker>();
            go.AddComponent<WindCapitalSecretTracker>();
            go.AddComponent<WindCapitalAdminCommandBridge>();
            var bridge = go.AddComponent<SarahPastQuestBridge>();
            bridge.SetData(LoadNpc("first_mate_sarah"), LoadNpc("captain_ramon"), sarahQuest);
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_WindCapitalRegistry.prefab");
            Object.DestroyImmediate(go);
        }

        private static List<WindCapitalIntrigueHookDefinition> LoadAllHooks()
        {
            var hooks = new List<WindCapitalIntrigueHookDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:WindCapitalIntrigueHookDefinition", new[] { HookDir }))
            {
                var hook = AssetDatabase.LoadAssetAtPath<WindCapitalIntrigueHookDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (hook != null)
                {
                    hooks.Add(hook);
                }
            }
            return hooks;
        }

        private static NpcWorldEntryDefinition LoadNpc(string id)
        {
            return AssetDatabase.LoadAssetAtPath<NpcWorldEntryDefinition>($"{NpcDir}/{id}.asset");
        }

        private static void WriteReport()
        {
            string path = $"{ReportDir}/WindCapitalReport.md";
            File.WriteAllText(path,
@"# Wind Capital Report

Generated by v33.

## Themes

```text
Sarah's hidden past
Wind Capital religious theocracy
Redbeard's usurpation
Lizkota's quiet control
Ruth as steam-channeler omen
chaotic religious fervor
old leaders missing or exiled
```

## Quests

```text
The Wind Sarah Won't Name
The Chaotic Aerie
```

## NPCs

```text
High Priest Redbeard
Priestess Lizkota
Infant Ruth
First Mate Sarah
Captain Ramón
```

## Runtime commands

```text
wind.summary
wind.fervor
wind.fervor.add amount
wind.hook id
wind.reveal id
wind.sarah
```
");
        }
    }
}
#endif
