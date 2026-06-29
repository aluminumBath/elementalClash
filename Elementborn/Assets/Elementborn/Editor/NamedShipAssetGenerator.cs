#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class NamedShipAssetGenerator
    {
        private const string ShipDir = "Assets/Elementborn/Generated/Ships";
        private const string NpcDir = "Assets/Elementborn/Generated/NPC/WorldEntries";
        private const string QuestDir = "Assets/Elementborn/Generated/QuestUI/Ships";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/Ships";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/Ship Tools/Generate Named Ship Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(ShipDir);
            Directory.CreateDirectory(QuestDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            NpcRosterCsvImporter.ImportAllRosters();
            CreateRaucousTide();
            CreateRegistryPrefab();
            WriteReport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated named ship assets.");
        }

        [MenuItem("Elementborn/Ship Tools/Install Ship Systems In Open Scene")]
        public static void InstallShipSystems()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Elementborn Ship Systems");
            if (root == null)
            {
                root = new GameObject("Elementborn Ship Systems");
            }

            var registry = root.GetComponent<NamedShipRegistry>();
            if (registry == null)
            {
                registry = root.AddComponent<NamedShipRegistry>();
            }

            registry.SetShips(LoadAllShips());

            if (root.GetComponent<ShipReputationTracker>() == null)
            {
                root.AddComponent<ShipReputationTracker>();
            }

            if (root.GetComponent<ShipAdminCommandBridge>() == null)
            {
                root.AddComponent<ShipAdminCommandBridge>();
            }

            if (root.GetComponent<ShipRaidCelebrationController>() == null)
            {
                root.AddComponent<ShipRaidCelebrationController>();
            }

            EditorUtility.SetDirty(root);
            Debug.Log("Installed ship systems in open scene.");
        }

        private static void CreateRaucousTide()
        {
            QuestUiDefinition raidQuest = CreateRaucousTideQuest();

            string path = $"{ShipDir}/RaucousTide.asset";
            var ship = AssetDatabase.LoadAssetAtPath<NamedShipDefinition>(path);
            if (ship == null)
            {
                ship = ScriptableObject.CreateInstance<NamedShipDefinition>();
                AssetDatabase.CreateAsset(ship, path);
            }

            var so = new SerializedObject(ship);
            so.FindProperty("shipId").stringValue = "raucous_tide";
            so.FindProperty("displayName").stringValue = "Raucous Tide";
            so.FindProperty("region").stringValue = "Neritha Reefwood";
            so.FindProperty("dockLocation").stringValue = "Hidden Reef Anchorage";
            so.FindProperty("worldPosition").vector3Value = new Vector3(266f, 0f, 340f);
            so.FindProperty("captain").objectReferenceValue = LoadNpc("captain_ramon");
            so.FindProperty("firstMate").objectReferenceValue = LoadNpc("first_mate_sarah");
            so.FindProperty("startingReputation").enumValueIndex = (int)ShipReputationTier.Raucous;
            so.FindProperty("faction").enumValueIndex = (int)ElementbornFactionId.SeaRaiders;
            so.FindProperty("description").stringValue = "A loud, lavish pirate ship from Neritha Reefwood, famous for turning every raid into a floating festival.";
            so.FindProperty("celebrationStyle").stringValue = "Lanterns, drums, stolen silk banners, reef-fruit punch, dancing on the rigging, and a feast so loud nearby gulls change course.";
            so.FindProperty("raidStyle").stringValue = "Fast water-channeler approach, wind-assisted boarding angles from Sarah, and a theatrical post-fight party that makes allies and enemies talk.";

            var crew = so.FindProperty("crew");
            string[] ids = {"captain_ramon","first_mate_sarah","maribel_tideglass","tomas_brightknife","ina_breezebell","garruk_copperjaw","luz_pearlstep"};
            ShipCrewRole[] roles = {ShipCrewRole.Captain,ShipCrewRole.FirstMate,ShipCrewRole.Quartermaster,ShipCrewRole.BoardingLead,ShipCrewRole.Lookout,ShipCrewRole.Cook,ShipCrewRole.Musician};
            string[] stations = {"Captain's Quarters","Main Deck","Quartermaster Stores","Boarding Rails","Crow's Nest","Galley","Party Deck"};
            string[] combat = {
                "Commands the ship and uses water channels to control boarding lanes.",
                "Reads wind and emotion; redirects fights before they collapse.",
                "Controls supplies, bribes, and emergency repairs.",
                "First over the rail, loud enough to distract everyone.",
                "Spots storms, patrols, and people hiding secrets.",
                "Turns galley tools into blunt weapons.",
                "Boosts morale and distracts enemies with rhythm and performance."
            };
            string[] partyRoles = {
                "Raises the first toast after victory.",
                "Quietly keeps the celebration from becoming dangerous.",
                "Unlocks the prize stores and rations the good bottles.",
                "Starts the victory songs.",
                "Rings wind bells over the dance deck.",
                "Feeds everyone until they forgive each other.",
                "Leads the music and knows which song breaks tension."
            };
            string[] hooks = {
                "His charm hides what he knows about Sarah's past.",
                "Her Wind Capital origin is a secret.",
                "Her ledger can expose half the crew.",
                "He picks fights when the crew needs a distraction.",
                "She may hear messages in storm bells.",
                "He knows Metal Capital dock gossip.",
                "She hears secrets during dances."
            };

            crew.arraySize = ids.Length;
            for (int i = 0; i < ids.Length; i++)
            {
                var entry = crew.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Npc").objectReferenceValue = LoadNpc(ids[i]);
                entry.FindPropertyRelative("CrewRole").enumValueIndex = (int)roles[i];
                entry.FindPropertyRelative("Station").stringValue = stations[i];
                entry.FindPropertyRelative("CombatRole").stringValue = combat[i];
                entry.FindPropertyRelative("PartyRole").stringValue = partyRoles[i];
                entry.FindPropertyRelative("SecretOrHook").stringValue = hooks[i];
            }

            var hooksArray = so.FindProperty("eventHooks");
            hooksArray.arraySize = 2;

            var raid = hooksArray.GetArrayElementAtIndex(0);
            raid.FindPropertyRelative("EventId").stringValue = "raucous_tide_raid";
            raid.FindPropertyRelative("EventType").enumValueIndex = (int)ShipEventType.Raid;
            raid.FindPropertyRelative("DisplayName").stringValue = "Raucous Tide Raid";
            raid.FindPropertyRelative("Description").stringValue = "The Raucous Tide is preparing a loud, risky raid near Neritha Reefwood.";
            raid.FindPropertyRelative("QuestToStart").objectReferenceValue = raidQuest;
            raid.FindPropertyRelative("Sound").enumValueIndex = (int)ElementbornSoundEventId.BoatWaveCreak;
            raid.FindPropertyRelative("ReputationDelta").intValue = 10;
            raid.FindPropertyRelative("AddJournalEntry").boolValue = true;

            var partyHook = hooksArray.GetArrayElementAtIndex(1);
            partyHook.FindPropertyRelative("EventId").stringValue = "raucous_tide_party";
            partyHook.FindPropertyRelative("EventType").enumValueIndex = (int)ShipEventType.Celebration;
            partyHook.FindPropertyRelative("DisplayName").stringValue = "Lavish Deck Party";
            partyHook.FindPropertyRelative("Description").stringValue = "After a fight, the Raucous Tide becomes a floating festival of drums, food, stories, and suspiciously expensive decorations.";
            partyHook.FindPropertyRelative("QuestToStart").objectReferenceValue = null;
            partyHook.FindPropertyRelative("Sound").enumValueIndex = (int)ElementbornSoundEventId.QuestComplete;
            partyHook.FindPropertyRelative("ReputationDelta").intValue = 3;
            partyHook.FindPropertyRelative("AddJournalEntry").boolValue = true;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ship);
        }

        private static QuestUiDefinition CreateRaucousTideQuest()
        {
            string path = $"{QuestDir}/RaucousTideAfterparty.asset";
            var quest = AssetDatabase.LoadAssetAtPath<QuestUiDefinition>(path);
            if (quest == null)
            {
                quest = ScriptableObject.CreateInstance<QuestUiDefinition>();
                AssetDatabase.CreateAsset(quest, path);
            }

            var so = new SerializedObject(quest);
            so.FindProperty("questId").stringValue = "raucous_tide_afterparty";
            so.FindProperty("title").stringValue = "The Raucous Tide Afterparty";
            so.FindProperty("description").stringValue = "Ramón's crew throws a lavish celebration after a raid, but Sarah's silence hints that someone from the Wind Capital may recognize her.";
            so.FindProperty("region").stringValue = "Neritha Reefwood";
            so.FindProperty("giverName").stringValue = "Captain Ramón";
            so.FindProperty("autoTrack").boolValue = true;

            var objectives = so.FindProperty("objectives");
            objectives.arraySize = 3;
            SetObjective(objectives.GetArrayElementAtIndex(0), "speak_to_ramon", "Speak with Captain Ramón", "Ask Ramón why the crew celebrates so loudly after every fight.", new Vector3(265f, 0f, 338f));
            SetObjective(objectives.GetArrayElementAtIndex(1), "check_on_sarah", "Check on First Mate Sarah", "Sarah refuses to discuss her past, but her reaction to the party feels important.", new Vector3(268f, 0f, 342f));
            SetObjective(objectives.GetArrayElementAtIndex(2), "join_the_party", "Join the deck celebration", "Listen for rumors while the crew celebrates across the Raucous Tide.", new Vector3(272f, 0f, 340f));

            var rewards = so.FindProperty("rewards");
            rewards.arraySize = 1;
            var reward = rewards.GetArrayElementAtIndex(0);
            reward.FindPropertyRelative("RewardId").stringValue = "raucous_tide_reputation";
            reward.FindPropertyRelative("DisplayName").stringValue = "Raucous Tide Reputation";
            reward.FindPropertyRelative("ItemId").stringValue = "";
            reward.FindPropertyRelative("Quantity").intValue = 1;
            reward.FindPropertyRelative("Currency").intValue = 35;
            reward.FindPropertyRelative("SkillPoints").intValue = 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(quest);
            return quest;
        }

        private static void SetObjective(SerializedProperty property, string id, string title, string description, Vector3 position)
        {
            property.FindPropertyRelative("ObjectiveId").stringValue = id;
            property.FindPropertyRelative("Title").stringValue = title;
            property.FindPropertyRelative("Description").stringValue = description;
            property.FindPropertyRelative("WorldPosition").vector3Value = position;
            property.FindPropertyRelative("CreateWaypoint").boolValue = true;
            property.FindPropertyRelative("Required").boolValue = true;
        }

        private static void CreateRegistryPrefab()
        {
            GameObject go = new GameObject("Elementborn_NamedShipRegistry");
            var registry = go.AddComponent<NamedShipRegistry>();
            registry.SetShips(LoadAllShips());
            go.AddComponent<ShipReputationTracker>();
            go.AddComponent<ShipAdminCommandBridge>();
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_NamedShipRegistry.prefab");
            Object.DestroyImmediate(go);
        }

        private static List<NamedShipDefinition> LoadAllShips()
        {
            var ships = new List<NamedShipDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:NamedShipDefinition", new[] { ShipDir }))
            {
                var ship = AssetDatabase.LoadAssetAtPath<NamedShipDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (ship != null)
                {
                    ships.Add(ship);
                }
            }
            return ships;
        }

        private static NpcWorldEntryDefinition LoadNpc(string id)
        {
            return AssetDatabase.LoadAssetAtPath<NpcWorldEntryDefinition>($"{NpcDir}/{id}.asset");
        }

        private static void WriteReport()
        {
            string path = $"{ReportDir}/NamedShipReport.md";
            File.WriteAllText(path,
@"# Named Ship Report

Generated by v31.

## Ships

```text
Raucous Tide
```

## Raucous Tide

A loud, lavish pirate ship from Neritha Reefwood, famous for turning every raid into a floating festival.

Crew:
- Captain Ramón
- First Mate Sarah
- Maribel Tideglass
- Tomas Brightknife
- Ina Breezebell
- Garruk Copperjaw
- Luz Pearlstep

## Runtime commands

```text
ship.list
ship.id raucous_tide
ship.raid raucous_tide
ship.celebrate raucous_tide
ship.rep raucous_tide|10
ship.reps
```
");
        }
    }
}
#endif
