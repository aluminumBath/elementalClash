#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class FireCapitalAssetGenerator
    {
        private const string HookDir = "Assets/Elementborn/Generated/FireCapital/Hooks";
        private const string QuestDir = "Assets/Elementborn/Generated/QuestUI/FireCapital";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/FireCapital";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";
        private const string NpcDir = "Assets/Elementborn/Generated/NPC/WorldEntries";

        [MenuItem("Elementborn/Fire Capital/Generate Fire Capital Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(HookDir);
            Directory.CreateDirectory(QuestDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            FireCapitalRoyalFamilyGenerator.GenerateAll();
            NpcRosterCsvImporter.ImportAllRosters();

            QuestUiDefinition audience = CreateQuest(
                "fire_caldera_audience",
                "Audience at the Caldera Throne",
                "Queen Seraphine asks the player to learn the needs of the Fire Capital before judging its politics.",
                "Fire Capital",
                "Queen Seraphine Cindervale",
                new QuestObjectiveUiDefinition { ObjectiveId = "speak_seraphine", Title = "Speak with Queen Seraphine", Description = "Meet Seraphine at the Caldera Throne.", WorldPosition = new Vector3(520f, 0f, -210f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "speak_oren", Title = "Speak with Oren", Description = "Ask Oren about the Furnace Guard.", WorldPosition = new Vector3(524f, 0f, -214f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "inspect_volcano", Title = "Inspect the volcano", Description = "Check the warning vents near the caldera.", WorldPosition = new Vector3(508f, 0f, -224f), CreateWaypoint = true, Required = true });

            QuestUiDefinition evacuation = CreateQuest(
                "fire_evacuation_drill",
                "The Caldera Evacuation Drill",
                "Master Ventwright Boros and Nix Ashrunner need help testing evacuation routes before the volcano surges.",
                "Fire Capital",
                "Master Ventwright Boros",
                new QuestObjectiveUiDefinition { ObjectiveId = "meet_boros", Title = "Meet Boros at the ventworks", Description = "Find the volcano engineer near the emergency valves.", WorldPosition = new Vector3(508f, 0f, -224f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "warn_market", Title = "Warn the Lava Market", Description = "Tell Talia Embermarket to prepare supplies.", WorldPosition = new Vector3(518f, 0f, -226f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "run_route", Title = "Run the outer evacuation route", Description = "Follow Nix Ashrunner's route out of the danger zone.", WorldPosition = new Vector3(500f, 0f, -236f), CreateWaypoint = true, Required = true });

            QuestUiDefinition guardTrial = CreateQuest(
                "furnace_guard_trial",
                "The Furnace Guard Trial",
                "Captain Vaela tests whether the player can protect civilians without worsening Fire Capital unrest.",
                "Fire Capital",
                "Captain Vaela Furnaceguard",
                new QuestObjectiveUiDefinition { ObjectiveId = "meet_vaela", Title = "Meet Captain Vaela", Description = "Report to the Furnace Guard Barracks.", WorldPosition = new Vector3(532f, 0f, -212f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "hold_line", Title = "Hold the defensive line", Description = "Stand between civilians and a simulated threat.", WorldPosition = new Vector3(536f, 0f, -216f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "debrief_oren", Title = "Debrief with Oren", Description = "Return to Prince-Consort Oren with results.", WorldPosition = new Vector3(524f, 0f, -214f), CreateWaypoint = true, Required = true });

            QuestUiDefinition lyraDiplomacy = CreateQuest(
                "lyra_glassfire_mediation",
                "Lyra's Glassfire Mediation",
                "Princess Lyra asks the player to calm a dispute before court rumor turns into factional pressure.",
                "Fire Capital",
                "Princess Lyra Cindervale",
                new QuestObjectiveUiDefinition { ObjectiveId = "meet_lyra", Title = "Meet Princess Lyra", Description = "Find Lyra on the Glassfire Balcony.", WorldPosition = new Vector3(514f, 0f, -216f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "speak_talia", Title = "Hear the market side", Description = "Ask Talia what the commoners fear.", WorldPosition = new Vector3(518f, 0f, -226f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "speak_calda", Title = "Hear the shrine side", Description = "Ask Sister Calda what old vows require.", WorldPosition = new Vector3(512f, 0f, -202f), CreateWaypoint = true, Required = true });

            QuestUiDefinition ashMemory = CreateQuest(
                "maelis_ash_memory",
                "Maelis and the Ash Memory",
                "Dowager Queen Maelis sends the player to recover a memory of the last great eruption.",
                "Fire Capital",
                "Dowager Queen Maelis Pyre",
                new QuestObjectiveUiDefinition { ObjectiveId = "meet_maelis", Title = "Meet Dowager Maelis", Description = "Speak with Maelis in the Old Hearth Library.", WorldPosition = new Vector3(510f, 0f, -208f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "visit_shrine", Title = "Visit the Glassfire Shrine", Description = "Ask Sister Calda to unlock the old ash record.", WorldPosition = new Vector3(512f, 0f, -202f), CreateWaypoint = true, Required = true },
                new QuestObjectiveUiDefinition { ObjectiveId = "return_memory", Title = "Return the memory", Description = "Bring the recovered memory back to Maelis.", WorldPosition = new Vector3(510f, 0f, -208f), CreateWaypoint = true, Required = true });

            CreateHook("fire_caldera_audience_hook", "Caldera Throne Audience", FireCapitalHookType.RoyalAudience, FireCapitalDistrict.CalderaThrone, "queen_seraphine_cindervale", audience, new Vector3(520f, 0f, -210f), CapitalPressureType.RulerLegitimacy, -2, 3,
                "The royal family receives the player inside the volcano citadel.",
                "Introduces the court and turns the Fire Capital from placeholder into a quest hub.");

            CreateHook("fire_evacuation_drill_hook", "Caldera Evacuation Drill", FireCapitalHookType.EvacuationDrill, FireCapitalDistrict.OuterEvacuationRoute, "master_ventwright_boros", evacuation, new Vector3(508f, 0f, -224f), CapitalPressureType.HiddenThreat, 5, 8,
                "Boros needs help proving the evacuation route works before the mountain surges.",
                "This hook supports route running, market warnings, and rescue beats.");

            CreateHook("furnace_guard_trial_hook", "Furnace Guard Trial", FireCapitalHookType.FurnaceGuardTrial, FireCapitalDistrict.FurnaceGuardBarracks, "captain_vaela_furnaceguard", guardTrial, new Vector3(532f, 0f, -212f), CapitalPressureType.ChannelerTension, 4, 5,
                "Captain Vaela tests whether the player can defend civilians without worsening panic.",
                "A light combat/defense tutorial hook.");

            CreateHook("lyra_glassfire_mediation_hook", "Lyra's Glassfire Mediation", FireCapitalHookType.DiplomaticMediation, FireCapitalDistrict.GlassfireBalcony, "princess_lyra_cindervale", lyraDiplomacy, new Vector3(514f, 0f, -216f), CapitalPressureType.Unrest, -2, 5,
                "Lyra turns a court dispute into a chance for the player to build trust.",
                "A non-combat social quest that connects palace, shrine, and market.");

            CreateHook("maelis_ash_memory_hook", "Maelis and the Ash Memory", FireCapitalHookType.AshMemory, FireCapitalDistrict.OldHearthLibrary, "dowager_queen_maelis_pyre", ashMemory, new Vector3(510f, 0f, -208f), CapitalPressureType.HiddenThreat, -3, 4,
                "Maelis reveals that old eruption records may explain present volcanic danger.",
                "History/lore hook that can foreshadow a Fire Capital boss or disaster.");

            CreateRegistryPrefab();
            WriteReport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated Fire Capital assets.");
        }

        [MenuItem("Elementborn/Fire Capital/Install Fire Capital Systems In Open Scene")]
        public static void InstallSystems()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Fire Capital Systems");
            if (root == null)
            {
                root = new GameObject("Fire Capital Systems");
            }

            var registry = root.GetComponent<FireCapitalRegistry>();
            if (registry == null) registry = root.AddComponent<FireCapitalRegistry>();
            registry.SetHooks(LoadAllHooks());

            if (root.GetComponent<FireCapitalVolcanoHazardController>() == null)
            {
                root.AddComponent<FireCapitalVolcanoHazardController>();
            }

            if (root.GetComponent<FireCapitalAdminCommandBridge>() == null)
            {
                root.AddComponent<FireCapitalAdminCommandBridge>();
            }

            CreateSceneHookInteractables(root.transform);
            EditorUtility.SetDirty(root);
            Debug.Log("Installed Fire Capital systems in open scene.");
        }

        private static void CreateSceneHookInteractables(Transform parent)
        {
            foreach (FireCapitalCourtHookDefinition hook in LoadAllHooks())
            {
                if (hook == null)
                {
                    continue;
                }

                GameObject existing = GameObject.Find("Fire Hook - " + hook.HookId);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Fire Hook - " + hook.HookId;
                go.transform.SetParent(parent, true);
                go.transform.position = hook.WorldPosition + Vector3.up * 0.5f;
                go.transform.localScale = new Vector3(1.4f, 1f, 1.4f);
                var interactable = go.AddComponent<FireCapitalCourtInteractable>();
                interactable.Configure(hook, false);
            }
        }

        private static void CreateRegistryPrefab()
        {
            GameObject go = new GameObject("Elementborn_FireCapitalRegistry");
            var registry = go.AddComponent<FireCapitalRegistry>();
            registry.SetHooks(LoadAllHooks());
            go.AddComponent<FireCapitalVolcanoHazardController>();
            go.AddComponent<FireCapitalAdminCommandBridge>();
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_FireCapitalRegistry.prefab");
            Object.DestroyImmediate(go);
        }

        private static QuestUiDefinition CreateQuest(string id, string title, string description, string region, string giverName, params QuestObjectiveUiDefinition[] objectives)
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
            so.FindProperty("region").stringValue = region;
            so.FindProperty("giverName").stringValue = giverName;
            so.FindProperty("autoTrack").boolValue = true;

            var array = so.FindProperty("objectives");
            array.arraySize = objectives.Length;
            for (int i = 0; i < objectives.Length; i++)
            {
                var src = objectives[i];
                var dst = array.GetArrayElementAtIndex(i);
                dst.FindPropertyRelative("ObjectiveId").stringValue = src.ObjectiveId;
                dst.FindPropertyRelative("Title").stringValue = src.Title;
                dst.FindPropertyRelative("Description").stringValue = src.Description;
                dst.FindPropertyRelative("WorldPosition").vector3Value = src.WorldPosition;
                dst.FindPropertyRelative("CreateWaypoint").boolValue = src.CreateWaypoint;
                dst.FindPropertyRelative("Required").boolValue = src.Required;
            }

            var rewards = so.FindProperty("rewards");
            rewards.arraySize = 1;
            var reward = rewards.GetArrayElementAtIndex(0);
            reward.FindPropertyRelative("RewardId").stringValue = id + "_reward";
            reward.FindPropertyRelative("DisplayName").stringValue = "Fire Capital trust";
            reward.FindPropertyRelative("Quantity").intValue = 1;
            reward.FindPropertyRelative("Currency").intValue = 30;
            reward.FindPropertyRelative("SkillPoints").intValue = 1;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(quest);
            return quest;
        }

        private static FireCapitalCourtHookDefinition CreateHook(string id, string title, FireCapitalHookType type, FireCapitalDistrict district, string primaryNpcId, QuestUiDefinition quest, Vector3 position, CapitalPressureType pressureType, int pressureDelta, int stabilityDelta, string summary, string notes)
        {
            string path = $"{HookDir}/{id}.asset";
            var hook = AssetDatabase.LoadAssetAtPath<FireCapitalCourtHookDefinition>(path);
            if (hook == null)
            {
                hook = ScriptableObject.CreateInstance<FireCapitalCourtHookDefinition>();
                AssetDatabase.CreateAsset(hook, path);
            }

            var so = new SerializedObject(hook);
            so.FindProperty("hookId").stringValue = id;
            so.FindProperty("title").stringValue = title;
            so.FindProperty("hookType").enumValueIndex = (int)type;
            so.FindProperty("district").enumValueIndex = (int)district;
            so.FindProperty("primaryNpc").objectReferenceValue = LoadNpc(primaryNpcId);
            so.FindProperty("questToStart").objectReferenceValue = quest;
            so.FindProperty("worldPosition").vector3Value = position;
            so.FindProperty("pressureType").enumValueIndex = (int)pressureType;
            so.FindProperty("pressureDeltaOnStart").intValue = pressureDelta;
            so.FindProperty("stabilityDeltaOnResolve").intValue = stabilityDelta;
            so.FindProperty("playerFacingSummary").stringValue = summary;
            so.FindProperty("hiddenDirectorNotes").stringValue = notes;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(hook);
            return hook;
        }

        private static NpcWorldEntryDefinition LoadNpc(string npcId)
        {
            return AssetDatabase.LoadAssetAtPath<NpcWorldEntryDefinition>($"{NpcDir}/{npcId}.asset");
        }

        private static List<FireCapitalCourtHookDefinition> LoadAllHooks()
        {
            var hooks = new List<FireCapitalCourtHookDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:FireCapitalCourtHookDefinition", new[] { HookDir }))
            {
                var hook = AssetDatabase.LoadAssetAtPath<FireCapitalCourtHookDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (hook != null && !hooks.Contains(hook))
                {
                    hooks.Add(hook);
                }
            }
            return hooks;
        }

        private static void WriteReport()
        {
            File.WriteAllText($"{ReportDir}/FireCapitalDeepeningReport.md",
@"# Fire Capital Deepening Report

Generated by v46.

## New quests

```text
Audience at the Caldera Throne
The Caldera Evacuation Drill
The Furnace Guard Trial
Lyra's Glassfire Mediation
Maelis and the Ash Memory
```

## New hooks

```text
fire_caldera_audience_hook
fire_evacuation_drill_hook
furnace_guard_trial_hook
lyra_glassfire_mediation_hook
maelis_ash_memory_hook
```

## New NPC roster

```text
Assets/Elementborn/Generated/NPC/Rosters/fire_capital_court_and_citizens.csv
```
");
        }
    }
}
#endif
