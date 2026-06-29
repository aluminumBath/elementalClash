#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class MetalCapitalAssetGenerator
    {
        private const string ContactDir = "Assets/Elementborn/Generated/MetalCapital/Contacts";
        private const string HookDir = "Assets/Elementborn/Generated/MetalCapital/Hooks";
        private const string QuestDir = "Assets/Elementborn/Generated/QuestUI/MetalCapital";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/MetalCapital";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";
        private const string NpcDir = "Assets/Elementborn/Generated/NPC/WorldEntries";

        [MenuItem("Elementborn/Metal Capital/Generate Metal Capital Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(ContactDir);
            Directory.CreateDirectory(HookDir);
            Directory.CreateDirectory(QuestDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            NpcRosterCsvImporter.ImportAllRosters();
            NpcWorldIntegrationAssetGenerator.GenerateAll();

            QuestUiDefinition bubbaQuest = CreateQuest(
                "bubba_workshop_errand",
                "Bubba's Black-Market Bolt",
                "Prince William, better known as Bubba, needs help finding a missing prototype bolt before the thieves guild sells it back to the palace.",
                "Prince William",
                new[]
                {
                    ("meet_bubba", "Meet Bubba in the court workshops", "Ask Bubba what went missing.", new Vector3(618f, 0f, 136f)),
                    ("visit_market", "Visit the black market", "Look for a vendor moving rare metal components.", new Vector3(602f, 0f, 122f)),
                    ("decide_return_or_trade", "Decide what to do with the bolt", "Return it to Bubba or use it as leverage with the thieves guild.", new Vector3(610f, 0f, 128f))
                });

            QuestUiDefinition larissaQuest = CreateQuest(
                "larissa_hidden_metal",
                "The Metal in Larissa's Silence",
                "Howlj hates channelers, but his wife Larissa is secretly a metal channeler. Discover the truth without putting her in danger.",
                "Larissa",
                new[]
                {
                    ("listen_to_howlj", "Listen to Howlj's accusations", "Hear what Howlj says about channelers.", new Vector3(590f, 0f, 118f)),
                    ("speak_with_larissa", "Speak privately with Larissa", "Find a way to talk without Howlj overhearing.", new Vector3(592f, 0f, 120f)),
                    ("protect_the_secret", "Protect or expose the secret", "Choose whether Larissa's channeling remains hidden.", new Vector3(592f, 0f, 120f))
                });

            CreateContacts();
            CreateHook("bubba_black_market_bolt", "Bubba's Missing Prototype", MetalCapitalDistrict.CourtWorkshops, "prince_william_bubba", null, bubbaQuest,
                "A prototype from Bubba's workshop has slipped into the black market.",
                "The thieves guild did not steal it at first; a palace worker sold it to pay off debt.",
                "A nervous workshop runner says Bubba lost something small, expensive, and embarrassing.",
                5);

            CreateHook("larissa_secret_metal", "Larissa's Hidden Channeling", MetalCapitalDistrict.BackroomSalons, "larissa", "howlj", larissaQuest,
                "Larissa is secretly a metal channeler while Howlj publicly despises channelers.",
                "Larissa has hidden her power for years because Howlj's narcissism and hatred make honesty dangerous.",
                "A ring bent itself back into shape in Larissa's hand, but nobody admits seeing it.",
                8);

            CreateHook("thieves_guild_open_door", "The Guild's Open Door", MetalCapitalDistrict.ThievesGuild, "king_randy", "queen_rhonda", null,
                "The thieves guild thrives because the Metal Capital court rarely intervenes directly.",
                "Rhonda and Randy believe pressure valves prevent revolution, but their court's tolerance has empowered dangerous criminals.",
                "A guild runner claims the crown sees more than it admits.",
                3);

            CreateRegistryPrefab();
            WriteReport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated Metal Capital assets.");
        }

        [MenuItem("Elementborn/Metal Capital/Install Metal Capital Systems In Open Scene")]
        public static void InstallSystems()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Elementborn Metal Capital Systems");
            if (root == null)
            {
                root = new GameObject("Elementborn Metal Capital Systems");
            }

            var registry = root.GetComponent<MetalCapitalRegistry>();
            if (registry == null)
            {
                registry = root.AddComponent<MetalCapitalRegistry>();
            }

            registry.SetData(LoadAllContacts(), LoadAllHooks());

            if (root.GetComponent<ThievesGuildReputationTracker>() == null)
            {
                root.AddComponent<ThievesGuildReputationTracker>();
            }

            if (root.GetComponent<HiddenChannelerSecretTracker>() == null)
            {
                root.AddComponent<HiddenChannelerSecretTracker>();
            }

            if (root.GetComponent<MetalCapitalAdminCommandBridge>() == null)
            {
                root.AddComponent<MetalCapitalAdminCommandBridge>();
            }

            EditorUtility.SetDirty(root);
            Debug.Log("Installed Metal Capital systems in open scene.");
        }

        private static void CreateContacts()
        {
            CreateContact("mira_coinveil", "Mira Coinveil", MetalCapitalContactType.BlackMarketVendor, MetalCapitalDistrict.BlackMarket, new Vector3(602f, 0f, 122f),
                "A cheerful vendor who sells tools, rumors, and things that definitely fell off a wagon.",
                "Mira quietly passes names to the thieves guild when buyers ask for forbidden metalwork.",
                new[]
                {
                    ("mirrored_lockpick", "Mirrored Lockpick Set", "metal_lockpick_set", 1, 85, BlackMarketRiskLevel.Watched, "Said to open court workshop cabinets.", "Buying it may attract guild attention."),
                    ("black_gear_charm", "Black Gear Charm", "black_gear_charm", 1, 120, BlackMarketRiskLevel.Risky, "A charm used by guild messengers.", "Wearing it may invite a test from thieves.")
                });

            CreateContact("oskar_underbridge", "Oskar Underbridge", MetalCapitalContactType.Fence, MetalCapitalDistrict.CoinLocks, new Vector3(598f, 0f, 116f),
                "A careful fence who appraises stolen components and never says where they came from.",
                "Oskar knows who first moved Bubba's missing prototype.",
                new[]
                {
                    ("bubba_prototype_hint", "Prototype Bolt Rumor", "rumor_bubba_bolt", 1, 35, BlackMarketRiskLevel.Safe, "A palace bolt came through the wrong door.", "May unlock Bubba's workshop quest shortcut.")
                });

            CreateContact("velvet_pin", "The Velvet Pin", MetalCapitalContactType.ThievesGuildHandler, MetalCapitalDistrict.ThievesGuild, new Vector3(594f, 0f, 110f),
                "A masked guild handler who speaks in party invitations and debt ledgers.",
                "The Velvet Pin knows the guild is testing whether Bubba can be manipulated.",
                new[]
                {
                    ("guild_favor", "Guild Favor", "guild_favor", 1, 200, BlackMarketRiskLevel.Dangerous, "A favor buys silence, not loyalty.", "Debt to the guild may come due later.")
                });
        }

        private static void CreateContact(string id, string name, MetalCapitalContactType type, MetalCapitalDistrict district, Vector3 position, string description, string secret, (string id, string name, string item, int qty, int price, BlackMarketRiskLevel risk, string rumor, string consequence)[] listings)
        {
            string path = $"{ContactDir}/{id}.asset";
            var contact = AssetDatabase.LoadAssetAtPath<MetalCapitalContactDefinition>(path);
            if (contact == null)
            {
                contact = ScriptableObject.CreateInstance<MetalCapitalContactDefinition>();
                AssetDatabase.CreateAsset(contact, path);
            }

            var so = new SerializedObject(contact);
            so.FindProperty("contactId").stringValue = id;
            so.FindProperty("displayName").stringValue = name;
            so.FindProperty("contactType").enumValueIndex = (int)type;
            so.FindProperty("district").enumValueIndex = (int)district;
            so.FindProperty("worldPosition").vector3Value = position;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("secret").stringValue = secret;

            var list = so.FindProperty("listings");
            list.arraySize = listings.Length;
            for (int i = 0; i < listings.Length; i++)
            {
                var entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("ListingId").stringValue = listings[i].id;
                entry.FindPropertyRelative("DisplayName").stringValue = listings[i].name;
                entry.FindPropertyRelative("ItemId").stringValue = listings[i].item;
                entry.FindPropertyRelative("Quantity").intValue = listings[i].qty;
                entry.FindPropertyRelative("Price").intValue = listings[i].price;
                entry.FindPropertyRelative("RiskLevel").enumValueIndex = (int)listings[i].risk;
                entry.FindPropertyRelative("Rumor").stringValue = listings[i].rumor;
                entry.FindPropertyRelative("Consequence").stringValue = listings[i].consequence;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(contact);
        }

        private static void CreateHook(string id, string title, MetalCapitalDistrict district, string primaryNpcId, string secondaryNpcId, QuestUiDefinition quest, string summary, string secret, string rumor, int repDelta)
        {
            string path = $"{HookDir}/{id}.asset";
            var hook = AssetDatabase.LoadAssetAtPath<MetalCapitalIntrigueHookDefinition>(path);
            if (hook == null)
            {
                hook = ScriptableObject.CreateInstance<MetalCapitalIntrigueHookDefinition>();
                AssetDatabase.CreateAsset(hook, path);
            }

            var so = new SerializedObject(hook);
            so.FindProperty("hookId").stringValue = id;
            so.FindProperty("title").stringValue = title;
            so.FindProperty("district").enumValueIndex = (int)district;
            so.FindProperty("primaryNpc").objectReferenceValue = LoadNpc(primaryNpcId);
            so.FindProperty("secondaryNpc").objectReferenceValue = string.IsNullOrWhiteSpace(secondaryNpcId) ? null : LoadNpc(secondaryNpcId);
            so.FindProperty("quest").objectReferenceValue = quest;
            so.FindProperty("summary").stringValue = summary;
            so.FindProperty("secretTruth").stringValue = secret;
            so.FindProperty("playerFacingRumor").stringValue = rumor;
            so.FindProperty("thievesGuildReputationDelta").intValue = repDelta;
            so.FindProperty("createsMapMarker").boolValue = true;
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
            so.FindProperty("region").stringValue = "Metal Capital";
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
            reward.FindPropertyRelative("DisplayName").stringValue = "Metal Capital Standing";
            reward.FindPropertyRelative("Currency").intValue = 50;
            reward.FindPropertyRelative("SkillPoints").intValue = 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(quest);
            return quest;
        }

        private static void CreateRegistryPrefab()
        {
            GameObject go = new GameObject("Elementborn_MetalCapitalRegistry");
            var registry = go.AddComponent<MetalCapitalRegistry>();
            registry.SetData(LoadAllContacts(), LoadAllHooks());
            go.AddComponent<ThievesGuildReputationTracker>();
            go.AddComponent<HiddenChannelerSecretTracker>();
            go.AddComponent<MetalCapitalAdminCommandBridge>();
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_MetalCapitalRegistry.prefab");
            Object.DestroyImmediate(go);
        }

        private static List<MetalCapitalContactDefinition> LoadAllContacts()
        {
            var results = new List<MetalCapitalContactDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:MetalCapitalContactDefinition", new[] { ContactDir }))
            {
                var item = AssetDatabase.LoadAssetAtPath<MetalCapitalContactDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null)
                {
                    results.Add(item);
                }
            }
            return results;
        }

        private static List<MetalCapitalIntrigueHookDefinition> LoadAllHooks()
        {
            var results = new List<MetalCapitalIntrigueHookDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:MetalCapitalIntrigueHookDefinition", new[] { HookDir }))
            {
                var item = AssetDatabase.LoadAssetAtPath<MetalCapitalIntrigueHookDefinition>(AssetDatabase.GUIDToAssetPath(guid));
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
            string path = $"{ReportDir}/MetalCapitalReport.md";
            File.WriteAllText(path,
@"# Metal Capital Report

Generated by v32.

## Themes

```text
laissez-faire monarchy
black market
thieves guild
hidden metal channelers
court workshop intrigue
Howlj and Larissa secret-channeler tension
Bubba palace/workshop questline
```

## Quests

```text
Bubba's Black-Market Bolt
The Metal in Larissa's Silence
```

## Contacts

```text
Mira Coinveil
Oskar Underbridge
The Velvet Pin
```

## Runtime commands

```text
metal.summary
metal.guild
metal.guild.add amount
metal.contact id
metal.hook id
metal.reveal npcId
```
");
        }
    }
}
#endif
