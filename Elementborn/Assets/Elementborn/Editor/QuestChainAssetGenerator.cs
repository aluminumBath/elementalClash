#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class QuestChainAssetGenerator
    {
        private const string ChainDir = "Assets/Elementborn/Generated/QuestChains";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/QuestChains";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/Quest Chains/Generate Quest Chain Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(ChainDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            PoliticalWorldEventAssetGenerator.GenerateAll();

            CreateSarahChain();
            CreateLarissaChain();
            CreateBubbaChain();
            CreateRaucousTideChain();
            CreateEarthCourtChain();

            CreatePrefab();
            WriteReport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated quest-chain assets.");
        }

        [MenuItem("Elementborn/Quest Chains/Install Quest Chain Director In Open Scene")]
        public static void InstallDirector()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Elementborn Quest Chain Director");
            if (root == null)
            {
                root = new GameObject("Elementborn Quest Chain Director");
            }

            var director = root.GetComponent<QuestChainDirector>();
            if (director == null)
            {
                director = root.AddComponent<QuestChainDirector>();
            }

            director.SetQuestChains(LoadAllChains());

            if (root.GetComponent<QuestChainAdminCommandBridge>() == null)
            {
                root.AddComponent<QuestChainAdminCommandBridge>();
            }

            EditorUtility.SetDirty(root);
            Debug.Log("Installed quest-chain director in open scene.");
        }

        private static void CreateSarahChain()
        {
            var chain = CreateChain("sarah_hidden_past_chain", "Sarah's Hidden Past", CapitalId.WindCapital,
                "A multi-stage chain connecting Sarah, Ramón, the Raucous Tide, and the Wind Capital theocracy.");

            var so = new SerializedObject(chain);
            so.FindProperty("autoStartOnEvent").boolValue = true;
            var stages = so.FindProperty("stages");
            stages.arraySize = 3;

            SetStage(stages.GetArrayElementAtIndex(0), "quiet_deck", "Quiet Deck Conversation",
                "Ask Ramón and Sarah what happened in the Wind Capital.",
                LoadQuest("Assets/Elementborn/Generated/QuestUI/WindCapital/sarah_wind_silence.asset"),
                LoadEvent("raucous_tide_port_emergency"),
                "pilgrim_rumor");

            SetStage(stages.GetArrayElementAtIndex(1), "pilgrim_rumor", "Pilgrim Rumor",
                "A Wind Capital pilgrim recognizes Sarah's old wind-reading habits.",
                null,
                null,
                "aerie_return");

            AddChoice(stages.GetArrayElementAtIndex(1), 0, "protect_sarah", "Protect Sarah's secret", QuestChainChoiceType.Protective,
                "Sarah's trust improves, but the Wind Capital truth remains hidden a little longer.",
                CapitalId.WindCapital, CapitalPressureType.HiddenThreat, -5, "The player protects Sarah from recognition.");

            AddChoice(stages.GetArrayElementAtIndex(1), 1, "press_for_truth", "Press Ramón for the truth", QuestChainChoiceType.Confrontational,
                "Ramón admits Sarah fled the Wind Capital after the old leaders disappeared.",
                CapitalId.WindCapital, CapitalPressureType.HiddenThreat, 8, "The player forces the Sarah thread toward the Wind Capital.");

            SetStage(stages.GetArrayElementAtIndex(2), "aerie_return", "Return to the Aerie",
                "The trail points back to Redbeard's usurped capital.",
                LoadQuest("Assets/Elementborn/Generated/QuestUI/WindCapital/the_chaotic_aerie.asset"),
                LoadEvent("wind_capital_riot"),
                "");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(chain);
        }

        private static void CreateLarissaChain()
        {
            var chain = CreateChain("larissa_secret_chain", "Larissa's Hidden Metal", CapitalId.MetalCapital,
                "A branching chain around Howlj's anti-channeler hatred and Larissa's secret metal channeling.");

            var so = new SerializedObject(chain);
            var stages = so.FindProperty("stages");
            stages.arraySize = 2;

            SetStage(stages.GetArrayElementAtIndex(0), "blackmail_attempt", "Blackmail Attempt",
                "A blackmailer threatens to expose Larissa.",
                LoadQuest("Assets/Elementborn/Generated/QuestUI/MetalCapital/larissa_hidden_metal.asset"),
                LoadEvent("larissa_secret_blackmail"),
                "choice_point");

            SetStage(stages.GetArrayElementAtIndex(1), "choice_point", "Protect or Expose",
                "Decide whether to protect Larissa, expose the truth, or use it against Howlj.",
                null,
                null,
                "");

            AddChoice(stages.GetArrayElementAtIndex(1), 0, "protect_larissa", "Protect Larissa", QuestChainChoiceType.Protective,
                "Larissa remains safe for now, but Howlj grows more suspicious.",
                CapitalId.MetalCapital, CapitalPressureType.HiddenThreat, -8, "The player protects Larissa's secret.");

            AddChoice(stages.GetArrayElementAtIndex(1), 1, "expose_howlj", "Expose Howlj's hypocrisy", QuestChainChoiceType.Confrontational,
                "Howlj loses influence, but anti-channeler tension spikes.",
                CapitalId.MetalCapital, CapitalPressureType.AntiChannelerSentiment, 10, "The player publicly challenges Howlj.");

            AddChoice(stages.GetArrayElementAtIndex(1), 2, "sell_secret", "Sell the secret to the guild", QuestChainChoiceType.Exploitative,
                "The thieves guild gains leverage over the couple.",
                CapitalId.MetalCapital, CapitalPressureType.ThievesGuildInfluence, 12, "The player lets the guild profit from the secret.");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(chain);
        }

        private static void CreateBubbaChain()
        {
            var chain = CreateChain("bubba_workshop_chain", "Bubba's Workshop Troubles", CapitalId.MetalCapital,
                "A palace/workshop chain around Prince William 'Bubba', black-market prototypes, and guild leverage.");

            var so = new SerializedObject(chain);
            var stages = so.FindProperty("stages");
            stages.arraySize = 2;

            SetStage(stages.GetArrayElementAtIndex(0), "missing_bolt", "The Missing Bolt",
                "Bubba's prototype bolt has slipped into black-market circulation.",
                LoadQuest("Assets/Elementborn/Generated/QuestUI/MetalCapital/bubba_workshop_errand.asset"),
                LoadEvent("metal_black_market_surge"),
                "choose_bolt");

            SetStage(stages.GetArrayElementAtIndex(1), "choose_bolt", "Return or Bargain",
                "Decide whether to return the prototype or use it as leverage.",
                null,
                null,
                "");

            AddChoice(stages.GetArrayElementAtIndex(1), 0, "return_to_bubba", "Return it to Bubba", QuestChainChoiceType.Merciful,
                "Bubba trusts the player, and court stability improves.",
                CapitalId.MetalCapital, CapitalPressureType.RoyalFamilyStability, 8, "The player helps Bubba.");

            AddChoice(stages.GetArrayElementAtIndex(1), 1, "trade_to_guild", "Trade it to the thieves guild", QuestChainChoiceType.Exploitative,
                "The guild grows stronger, but Bubba learns a hard lesson.",
                CapitalId.MetalCapital, CapitalPressureType.ThievesGuildInfluence, 10, "The player empowers the guild.");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(chain);
        }

        private static void CreateRaucousTideChain()
        {
            var chain = CreateChain("raucous_tide_chain", "Raucous Tide Revels", CapitalId.NerithaReefwood,
                "A quest chain for Ramón, Sarah, the crew, and their lavish post-raid celebrations.");

            var so = new SerializedObject(chain);
            var stages = so.FindProperty("stages");
            stages.arraySize = 2;

            SetStage(stages.GetArrayElementAtIndex(0), "afterparty", "The Afterparty",
                "Join the Raucous Tide's celebration and listen for secrets.",
                LoadQuest("Assets/Elementborn/Generated/QuestUI/Ships/RaucousTideAfterparty.asset"),
                LoadEvent("raucous_tide_port_emergency"),
                "morning_after");

            SetStage(stages.GetArrayElementAtIndex(1), "morning_after", "The Morning After",
                "Sort out consequences after the party turns into a political problem.",
                null,
                null,
                "");

            AddChoice(stages.GetArrayElementAtIndex(1), 0, "cover_for_crew", "Cover for the crew", QuestChainChoiceType.Chaotic,
                "Pirate activity rises, but Ramón owes the player.",
                CapitalId.NerithaReefwood, CapitalPressureType.PirateActivity, 8, "The player covers for the Raucous Tide.");

            AddChoice(stages.GetArrayElementAtIndex(1), 1, "calm_the_port", "Calm the port", QuestChainChoiceType.Diplomatic,
                "Neritha Reefwood stabilizes, but the crew thinks the player is less fun.",
                CapitalId.NerithaReefwood, CapitalPressureType.Unrest, -8, "The player defuses the port incident.");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(chain);
        }

        private static void CreateEarthCourtChain()
        {
            var chain = CreateChain("earth_court_chain", "The Malachite Court", CapitalId.EarthCapital,
                "A stabilizing court chain for Queen Alexis, Chrief Gover, and Tibidi.");

            var so = new SerializedObject(chain);
            var stages = so.FindProperty("stages");
            stages.arraySize = 1;

            SetStage(stages.GetArrayElementAtIndex(0), "audience", "Audience with the Malachite Court",
                "Meet the kind and intelligent leaders of the Earth Capital.",
                LoadQuest("Assets/Elementborn/Generated/QuestUI/NPC/audience_with_the_malachite_court.asset"),
                LoadEvent("earth_court_stabilizes"),
                "");

            AddChoice(stages.GetArrayElementAtIndex(0), 0, "support_tibidi", "Encourage Tibidi's plant channeling", QuestChainChoiceType.Merciful,
                "The court becomes more open to mixed element inheritance.",
                CapitalId.EarthCapital, CapitalPressureType.ChannelerTension, -6, "The player supports Tibidi.");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(chain);
        }

        private static QuestChainDefinition CreateChain(string id, string name, CapitalId capital, string summary)
        {
            string path = $"{ChainDir}/{id}.asset";
            var chain = AssetDatabase.LoadAssetAtPath<QuestChainDefinition>(path);
            if (chain == null)
            {
                chain = ScriptableObject.CreateInstance<QuestChainDefinition>();
                AssetDatabase.CreateAsset(chain, path);
            }

            var so = new SerializedObject(chain);
            so.FindProperty("chainId").stringValue = id;
            so.FindProperty("displayName").stringValue = name;
            so.FindProperty("primaryCapital").enumValueIndex = (int)capital;
            so.FindProperty("summary").stringValue = summary;
            so.ApplyModifiedProperties();
            return chain;
        }

        private static void SetStage(SerializedProperty stage, string id, string title, string summary, QuestUiDefinition quest, PoliticalWorldEventDefinition trigger, string next)
        {
            stage.FindPropertyRelative("StageId").stringValue = id;
            stage.FindPropertyRelative("Title").stringValue = title;
            stage.FindPropertyRelative("Summary").stringValue = summary;
            stage.FindPropertyRelative("Quest").objectReferenceValue = quest;
            stage.FindPropertyRelative("TriggeredByPoliticalEvent").objectReferenceValue = trigger;
            stage.FindPropertyRelative("AutoStartQuest").boolValue = true;
            stage.FindPropertyRelative("AutoCompleteWhenQuestCompletes").boolValue = false;
            stage.FindPropertyRelative("DefaultNextStageId").stringValue = next;
            stage.FindPropertyRelative("Choices").arraySize = 0;
        }

        private static void AddChoice(SerializedProperty stage, int index, string id, string text, QuestChainChoiceType type, string result, CapitalId capital, CapitalPressureType pressure, int delta, string notes)
        {
            var choices = stage.FindPropertyRelative("Choices");
            if (choices.arraySize <= index)
            {
                choices.arraySize = index + 1;
            }

            var choice = choices.GetArrayElementAtIndex(index);
            choice.FindPropertyRelative("ChoiceId").stringValue = id;
            choice.FindPropertyRelative("DisplayText").stringValue = text;
            choice.FindPropertyRelative("ChoiceType").enumValueIndex = (int)type;
            choice.FindPropertyRelative("PlayerFacingResult").stringValue = result;
            choice.FindPropertyRelative("HiddenDirectorNotes").stringValue = notes;

            var consequences = choice.FindPropertyRelative("Consequences");
            consequences.arraySize = 1;
            var consequence = consequences.GetArrayElementAtIndex(0);
            consequence.FindPropertyRelative("ConsequenceType").enumValueIndex = (int)QuestChainConsequenceType.CapitalPressure;
            consequence.FindPropertyRelative("Capital").enumValueIndex = (int)capital;
            consequence.FindPropertyRelative("PressureType").enumValueIndex = (int)pressure;
            consequence.FindPropertyRelative("Amount").intValue = delta;
            consequence.FindPropertyRelative("Notes").stringValue = notes;
        }

        private static QuestUiDefinition LoadQuest(string path)
        {
            return AssetDatabase.LoadAssetAtPath<QuestUiDefinition>(path);
        }

        private static PoliticalWorldEventDefinition LoadEvent(string id)
        {
            return AssetDatabase.LoadAssetAtPath<PoliticalWorldEventDefinition>($"Assets/Elementborn/Generated/WorldState/PoliticalEvents/{id}.asset");
        }

        private static void CreatePrefab()
        {
            GameObject go = new GameObject("Elementborn_QuestChainDirector");
            var director = go.AddComponent<QuestChainDirector>();
            director.SetQuestChains(LoadAllChains());
            go.AddComponent<QuestChainAdminCommandBridge>();
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_QuestChainDirector.prefab");
            Object.DestroyImmediate(go);
        }

        private static List<QuestChainDefinition> LoadAllChains()
        {
            var results = new List<QuestChainDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:QuestChainDefinition", new[] { ChainDir }))
            {
                var item = AssetDatabase.LoadAssetAtPath<QuestChainDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null)
                {
                    results.Add(item);
                }
            }

            return results;
        }

        private static void WriteReport()
        {
            string path = $"{ReportDir}/QuestChainDirectorReport.md";
            File.WriteAllText(path,
@"# Quest Chain Director Report

Generated by v36.

## Chains

```text
Sarah's Hidden Past
Larissa's Hidden Metal
Bubba's Workshop Troubles
Raucous Tide Revels
The Malachite Court
```

## Runtime commands

```text
chain.summary
chain.start chainId
chain.stage chainId|stageId
chain.complete chainId|stageId
chain.choice chainId|stageId|choiceId
```
");
        }
    }
}
#endif
