#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class SocialNpcQuestAndPrefabGenerator
    {
        private const string QuestDir = "Assets/Elementborn/Generated/QuestUI/SocialNPC";
        private const string DialogueDir = "Assets/Elementborn/Generated/SocialNPC/DialogueProfiles";
        private const string ChainDir = "Assets/Elementborn/Generated/QuestChains";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/SocialNPC";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";
        private const string NpcDir = "Assets/Elementborn/Generated/NPC/WorldEntries";

        [MenuItem("Elementborn/Social NPCs/Generate Social NPC Quests And Dialogue")]
        public static void GenerateQuestsAndDialogue()
        {
            Directory.CreateDirectory(QuestDir);
            Directory.CreateDirectory(DialogueDir);
            Directory.CreateDirectory(ChainDir);
            Directory.CreateDirectory(ReportDir);

            SocialNpcRosterGenerator.GenerateRoster();

            var rekrQuest = CreateQuest("rekr_remedy_run", "Rekr's Remedy Run", "Rekr Ap is sickly, gross, and needs help before Manon runs out of patience.", "Rekr Ap",
                new[] {
                    ("check_rekr", "Check on Rekr", "Find out what is making Rekr worse today.", new Vector3(742f, 21f, 436f)),
                    ("find_remedy", "Find a tolerable remedy", "Collect something that might help a lava channeler living in the Wind Capital.", new Vector3(746f, 21f, 437f)),
                    ("calm_manon", "Calm Manon", "Convince Manon this will not ruin the apartment again.", new Vector3(744f, 21f, 438f))
                });

            var manonQuest = CreateQuest("manon_immaculate_crisis", "Manon's Immaculate Crisis", "Manon needs help restoring order after Rekr and Marie create a domestic disaster.", "Manon",
                new[] {
                    ("inspect_mess", "Inspect the mess", "Work out whether Rekr, Marie, or both caused the current disaster.", new Vector3(744f, 21f, 438f)),
                    ("clean_hazards", "Clean the hazards", "Remove soot, spilled remedy, and suspicious warm stains.", new Vector3(745f, 21f, 438f)),
                    ("restore_order", "Restore order", "Help Manon regain control before she starts organizing everyone alphabetically.", new Vector3(744f, 21f, 438f))
                });

            var marieQuest = CreateQuest("marie_accidental_conflagration", "Marie's Accidental Conflagration", "Marie Conflag falls asleep, flirts at the wrong time, and accidentally lights things on fire.", "Marie Conflag",
                new[] {
                    ("wake_marie", "Wake Marie", "Wake Marie before the fire spreads.", new Vector3(748f, 22f, 440f)),
                    ("put_out_fire", "Put out the accidental fire", "Contain the fire while Marie insists it is dramatic lighting.", new Vector3(749f, 22f, 441f)),
                    ("stop_flirting", "Redirect Marie", "Keep Marie focused long enough to apologize.", new Vector3(748f, 22f, 440f))
                });

            var amyQuest = CreateQuest("amy_goes_with_it", "Amy Goes With It", "Amy Whine accidentally drifts through important rumors because she simply goes with the flow.", "Amy Whine",
                new[] {
                    ("follow_amy", "Follow Amy's wandering conversation", "Amy is not sure where she is going, which may be why she overhears everything.", new Vector3(750f, 22f, 442f)),
                    ("sort_rumor", "Sort the rumor from nonsense", "Find the one useful clue buried inside Amy's drifting story.", new Vector3(751f, 22f, 442f)),
                    ("share_clue", "Share the clue", "Decide whether to tell Johna, Kelly, or the authorities.", new Vector3(752f, 26f, 430f))
                });

            var kellyQuest = CreateQuest("kelly_moodflame_watch", "Kelly's Moodflame Watch", "Kelly's flame-colored hair reveals her mood while she protects friends and neighbors.", "Kelly",
                new[] {
                    ("read_mood", "Read Kelly's moodflame", "Learn how Kelly's hair color reveals what is really happening.", new Vector3(526f, 0f, -198f)),
                    ("help_neighbor", "Help a neighbor", "Join Kelly as she protects someone nearby.", new Vector3(528f, 0f, -198f)),
                    ("mischief_or_mercy", "Choose mischief or mercy", "Decide how Kelly should handle the troublemaker.", new Vector3(526f, 0f, -198f))
                });

            var johnaQuest = CreateQuest("johna_pipe_counsel", "Johna's Pipe Counsel", "Johna Rold offers kind, smoke-wreathed advice that can calm the Wind Capital lower terraces.", "Johna Rold",
                new[] {
                    ("sit_with_johna", "Sit with Johna", "Listen to Johna's advice while smoke curls into the sky.", new Vector3(752f, 26f, 430f)),
                    ("carry_advice", "Carry advice to the neighbors", "Repeat Johna's counsel to someone who needs it.", new Vector3(748f, 22f, 440f)),
                    ("ease_tension", "Ease the tension", "Help the neighborhood settle before rumor becomes panic.", new Vector3(752f, 26f, 430f))
                });

            CreateProfile("rekr_ap", rekrQuest, new[] {
                ("greeting", "", "If the room smells like hot pennies and bad soup, that is probably me.", "Rekr is sickly and embarrassed, but not entirely helpless.", SocialNpcCueImportance.Flavor),
                ("remedy", "remedy", "Tell Manon I took it. Actually, wait. Tell her I almost took it.", "Rekr needs a remedy and a little dignity.", SocialNpcCueImportance.Quest),
                ("manon", "manon", "She is annoyed because she loves me. Probably. Mostly.", "Rekr knows Manon is frustrated but still devoted.", SocialNpcCueImportance.Hint)
            });

            CreateProfile("manon", manonQuest, new[] {
                ("greeting", "", "Do not touch that. Or that. Or Rekr, unless you are wearing gloves.", "Manon controls chaos through cleanliness.", SocialNpcCueImportance.Flavor),
                ("rekr", "rekr", "I married him, not the ecosystem growing on his coat.", "Manon is annoyed with Rekr but still responsible for him.", SocialNpcCueImportance.Hint),
                ("clean", "clean", "Finally. Someone who understands civilization begins with a clean floor.", "Manon can turn cleanup into a quest.", SocialNpcCueImportance.Quest)
            });

            CreateProfile("marie_conflag", marieQuest, new[] {
                ("greeting", "", "Is that smoke from me? How flattering.", "Marie is vain, sleepy, and dangerously casual with fire.", SocialNpcCueImportance.Flavor),
                ("fire", "fire", "Oh relax, darling. It was barely a blaze.", "Marie often needs help putting out accidental fires.", SocialNpcCueImportance.Quest),
                ("flirt", "flirt", "Send in someone handsome with a bucket.", "Marie flirts through crisis.", SocialNpcCueImportance.Flavor)
            });

            CreateProfile("amy_whine", amyQuest, new[] {
                ("greeting", "", "I was following Marie, then a bird, then maybe a rumor?", "Amy drifts into useful information by accident.", SocialNpcCueImportance.Flavor),
                ("rumor", "rumor", "Someone said something important. I remember because I forgot why it mattered.", "Amy can accidentally surface clues.", SocialNpcCueImportance.Hint),
                ("friends", "friends", "I just go where everyone else goes. It usually becomes interesting.", "Amy keeps the social group moving.", SocialNpcCueImportance.Flavor)
            });

            CreateProfile("kelly", kellyQuest, new[] {
                ("greeting", "", "My hair is gold, so nobody has annoyed me enough yet.", "Kelly's moodflame shows how she feels.", SocialNpcCueImportance.Flavor),
                ("protect", "protect", "Touch my neighbors and I become a local natural disaster.", "Kelly fiercely protects friends and neighbors.", SocialNpcCueImportance.Quest),
                ("mischief", "mischief", "Mercy is good. Mischief is also good. Balance matters.", "Kelly has a mischievous side.", SocialNpcCueImportance.Hint)
            });

            CreateProfile("johna_rold", johnaQuest, new[] {
                ("greeting", "", "Sit. The smoke will leave before the worry does.", "Johna gives gentle advice.", SocialNpcCueImportance.Flavor),
                ("advice", "advice", "When a city gets loud, ask who benefits from the noise.", "Johna can steer the player toward political truth.", SocialNpcCueImportance.Hint),
                ("sick", "sick", "A body can be weak and still carry useful words.", "Johna is sickly but wise.", SocialNpcCueImportance.Flavor)
            });

            CreateChain("rekr_manon_domestic_chain", "Rekr and Manon's Domestic Disaster", CapitalId.WindCapital, rekrQuest, "A comedy-domestic chain around illness, lava grossness, and cleanliness tension.");
            CreateChain("marie_amy_fire_gossip_chain", "Marie and Amy's Fire-Gossip Spiral", CapitalId.WindCapital, marieQuest, "Marie starts fires; Amy accidentally moves rumors through the smoke.");
            CreateChain("kelly_neighborhood_watch_chain", "Kelly's Neighborhood Watch", CapitalId.FireCapital, kellyQuest, "Kelly protects neighbors while her moodflame betrays her feelings.");
            CreateChain("johna_pipe_counsel_chain", "Johna's Pipe Counsel", CapitalId.WindCapital, johnaQuest, "Johna's advice calms the lower terraces.");

            WriteReport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated social NPC quests, dialogue, and chains.");
        }

        [MenuItem("Elementborn/Social NPCs/Create Social NPC Placeholder Prefabs")]
        public static void CreatePlaceholderPrefabs()
        {
            GenerateQuestsAndDialogue();
            Directory.CreateDirectory(PrefabDir);

            CreateNpcPrefab("rekr_ap", typeof(RekrGrossnessController));
            CreateNpcPrefab("manon", typeof(ManonCleanlinessController));
            CreateNpcPrefab("marie_conflag", typeof(MarieAccidentalFireController));
            CreateNpcPrefab("amy_whine", null);
            CreateNpcPrefab("kelly", typeof(KellyMoodFlameController));
            CreateNpcPrefab("johna_rold", typeof(JohnaAdviceController));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created social NPC placeholder prefabs.");
        }

        [MenuItem("Elementborn/Social NPCs/Install Social NPC Placeholders In Open Scene")]
        public static void InstallPlaceholders()
        {
            CreatePlaceholderPrefabs();

            GameObject root = GameObject.Find("Elementborn Social NPC Placeholders");
            if (root == null)
            {
                root = new GameObject("Elementborn Social NPC Placeholders");
            }

            InstallNpc("rekr_ap", new Vector3(742f, 21f, 436f), root.transform);
            InstallNpc("manon", new Vector3(744f, 21f, 438f), root.transform);
            InstallNpc("marie_conflag", new Vector3(748f, 22f, 440f), root.transform);
            InstallNpc("amy_whine", new Vector3(750f, 22f, 442f), root.transform);
            InstallNpc("kelly", new Vector3(526f, 0f, -198f), root.transform);
            InstallNpc("johna_rold", new Vector3(752f, 26f, 430f), root.transform);

            var registry = root.GetComponent<SocialNpcDialogueRegistry>();
            if (registry == null)
            {
                registry = root.AddComponent<SocialNpcDialogueRegistry>();
            }
            registry.SetProfiles(LoadAllProfiles());

            if (root.GetComponent<SocialNpcAdminCommandBridge>() == null)
            {
                root.AddComponent<SocialNpcAdminCommandBridge>();
            }

            EditorUtility.SetDirty(root);
            Debug.Log("Installed social NPC placeholders in open scene.");
        }

        private static void CreateNpcPrefab(string npcId, System.Type extraComponent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Elementborn_" + npcId + "_Placeholder";

            var npc = LoadNpc(npcId);
            var marker = go.AddComponent<NpcWorldPlacementMarker>();
            if (npc != null)
            {
                marker.SetNpc(npc);
            }

            var hook = go.AddComponent<SocialNpcDialogueHookInteractable>();
            hook.SetProfile(LoadProfile(npcId));

            if (extraComponent != null)
            {
                go.AddComponent(extraComponent);
            }

            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_{npcId}_Placeholder.prefab");
            Object.DestroyImmediate(go);
        }

        private static void InstallNpc(string npcId, Vector3 position, Transform parent)
        {
            string path = $"{PrefabDir}/Elementborn_{npcId}_Placeholder.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return;
            }

            if (GameObject.Find("Elementborn_" + npcId + "_Placeholder") != null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                instance.transform.SetParent(parent, true);
                instance.transform.position = position;
            }
        }

        private static void CreateProfile(string npcId, QuestUiDefinition quest, (string cueId, string keyword, string line, string journal, SocialNpcCueImportance importance)[] cues)
        {
            string path = $"{DialogueDir}/{npcId}_social_dialogue.asset";
            var profile = AssetDatabase.LoadAssetAtPath<SocialNpcDialogueProfileDefinition>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<SocialNpcDialogueProfileDefinition>();
                AssetDatabase.CreateAsset(profile, path);
            }

            var npc = LoadNpc(npcId);
            var so = new SerializedObject(profile);
            so.FindProperty("npcId").stringValue = npcId;
            so.FindProperty("displayName").stringValue = npc != null ? npc.DisplayName : npcId;
            so.FindProperty("npc").objectReferenceValue = npc;
            so.FindProperty("summary").stringValue = npc != null ? npc.Notes : "";

            var cueArray = so.FindProperty("cues");
            cueArray.arraySize = cues.Length;
            for (int i = 0; i < cues.Length; i++)
            {
                var cue = cueArray.GetArrayElementAtIndex(i);
                cue.FindPropertyRelative("CueId").stringValue = cues[i].cueId;
                cue.FindPropertyRelative("TriggerKeyword").stringValue = cues[i].keyword;
                cue.FindPropertyRelative("Line").stringValue = cues[i].line;
                cue.FindPropertyRelative("JournalNote").stringValue = cues[i].journal;
                cue.FindPropertyRelative("QuestToStart").objectReferenceValue = i == 1 ? quest : null;
                cue.FindPropertyRelative("Importance").enumValueIndex = (int)cues[i].importance;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(profile);
        }

        private static QuestUiDefinition CreateQuest(string id, string title, string description, string giver, (string id, string title, string description, Vector3 position)[] objectives)
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
            so.FindProperty("region").stringValue = "Social NPCs";
            so.FindProperty("giverName").stringValue = giver;
            so.FindProperty("autoTrack").boolValue = true;

            var objectiveArray = so.FindProperty("objectives");
            objectiveArray.arraySize = objectives.Length;
            for (int i = 0; i < objectives.Length; i++)
            {
                var objective = objectiveArray.GetArrayElementAtIndex(i);
                objective.FindPropertyRelative("ObjectiveId").stringValue = objectives[i].id;
                objective.FindPropertyRelative("Title").stringValue = objectives[i].title;
                objective.FindPropertyRelative("Description").stringValue = objectives[i].description;
                objective.FindPropertyRelative("WorldPosition").vector3Value = objectives[i].position;
                objective.FindPropertyRelative("CreateWaypoint").boolValue = true;
                objective.FindPropertyRelative("Required").boolValue = true;
            }

            var rewards = so.FindProperty("rewards");
            rewards.arraySize = 1;
            var reward = rewards.GetArrayElementAtIndex(0);
            reward.FindPropertyRelative("RewardId").stringValue = id + "_reward";
            reward.FindPropertyRelative("DisplayName").stringValue = "Neighborhood Trust";
            reward.FindPropertyRelative("Currency").intValue = 20;
            reward.FindPropertyRelative("SkillPoints").intValue = 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(quest);
            return quest;
        }

        private static void CreateChain(string id, string title, CapitalId capital, QuestUiDefinition quest, string summary)
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
            so.FindProperty("displayName").stringValue = title;
            so.FindProperty("primaryCapital").enumValueIndex = (int)capital;
            so.FindProperty("summary").stringValue = summary;

            var stages = so.FindProperty("stages");
            stages.arraySize = 1;
            var stage = stages.GetArrayElementAtIndex(0);
            stage.FindPropertyRelative("StageId").stringValue = "main";
            stage.FindPropertyRelative("Title").stringValue = title;
            stage.FindPropertyRelative("Summary").stringValue = summary;
            stage.FindPropertyRelative("Quest").objectReferenceValue = quest;
            stage.FindPropertyRelative("AutoStartQuest").boolValue = true;
            stage.FindPropertyRelative("DefaultNextStageId").stringValue = "";

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(chain);
        }

        private static List<SocialNpcDialogueProfileDefinition> LoadAllProfiles()
        {
            var profiles = new List<SocialNpcDialogueProfileDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:SocialNpcDialogueProfileDefinition", new[] { DialogueDir }))
            {
                var profile = AssetDatabase.LoadAssetAtPath<SocialNpcDialogueProfileDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (profile != null)
                {
                    profiles.Add(profile);
                }
            }
            return profiles;
        }

        private static SocialNpcDialogueProfileDefinition LoadProfile(string npcId)
        {
            return AssetDatabase.LoadAssetAtPath<SocialNpcDialogueProfileDefinition>($"{DialogueDir}/{npcId}_social_dialogue.asset");
        }

        private static NpcWorldEntryDefinition LoadNpc(string npcId)
        {
            return AssetDatabase.LoadAssetAtPath<NpcWorldEntryDefinition>($"{NpcDir}/{npcId}.asset");
        }

        private static void WriteReport()
        {
            File.WriteAllText($"{ReportDir}/SocialNpcQuestReport.md",
@"# Social NPC Quest Report

Generated by v42.

## Quests

```text
Rekr's Remedy Run
Manon's Immaculate Crisis
Marie's Accidental Conflagration
Amy Goes With It
Kelly's Moodflame Watch
Johna's Pipe Counsel
```

## Placeholder prefab menu

```text
Elementborn → Social NPCs → Create Social NPC Placeholder Prefabs
Elementborn → Social NPCs → Install Social NPC Placeholders In Open Scene
```
");
        }
    }
}
#endif
