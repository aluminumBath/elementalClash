#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class NpcWorldIntegrationAssetGenerator
    {
        private const string NpcDir = "Assets/Elementborn/Generated/NPC/WorldEntries";
        private const string DialogueDir = "Assets/Elementborn/Generated/NPC/DialogueProfiles";
        private const string IntegrationDir = "Assets/Elementborn/Generated/NPC/Integration";
        private const string QuestDir = "Assets/Elementborn/Generated/QuestUI/NPC";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/NPC";

        [MenuItem("Elementborn/NPC Tools/Generate NPC World Integration Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(DialogueDir);
            Directory.CreateDirectory(IntegrationDir);
            Directory.CreateDirectory(QuestDir);
            Directory.CreateDirectory(PrefabDir);

            NpcRosterCsvImporter.ImportAllRosters();
            List<NpcWorldEntryDefinition> entries = LoadAllNpcEntries();
            CreateManifest(entries);
            CreateDialogueProfiles(entries);
            CreateEarthCapitalRoyalQuests(entries);
            CreateFireCapitalRoyalQuests(entries);
            CreateIntegrationPrefab();
            WriteReport(entries);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated NPC world integration assets for {entries.Count} NPCs.");
        }

        [MenuItem("Elementborn/NPC Tools/Install NPC Integration Manager In Open Scene")]
        public static void InstallManager()
        {
            GenerateAll();
            GameObject existing = GameObject.Find("NPC World Integration Manager");
            if (existing == null)
            {
                existing = new GameObject("NPC World Integration Manager");
            }

            var manager = existing.GetComponent<NpcWorldIntegrationManager>();
            if (manager == null)
            {
                manager = existing.AddComponent<NpcWorldIntegrationManager>();
            }

            var manifest = AssetDatabase.LoadAssetAtPath<NpcWorldIntegrationManifest>($"{IntegrationDir}/NpcWorldIntegrationManifest.asset");
            var so = new SerializedObject(manager);
            so.FindProperty("manifest").objectReferenceValue = manifest;
            so.FindProperty("registerMarkersOnStart").boolValue = true;
            so.FindProperty("addJournalEntriesOnStart").boolValue = true;
            so.FindProperty("spawnPlaceholderNpcsOnStart").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(existing);
            Debug.Log("Installed NPC World Integration Manager in open scene.");
        }

        private static List<NpcWorldEntryDefinition> LoadAllNpcEntries()
        {
            var entries = new List<NpcWorldEntryDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:NpcWorldEntryDefinition", new[] { NpcDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var entry = AssetDatabase.LoadAssetAtPath<NpcWorldEntryDefinition>(path);
                if (entry != null && !entries.Contains(entry))
                {
                    entries.Add(entry);
                }
            }
            return entries;
        }

        private static void CreateManifest(List<NpcWorldEntryDefinition> entries)
        {
            string path = $"{IntegrationDir}/NpcWorldIntegrationManifest.asset";
            var manifest = AssetDatabase.LoadAssetAtPath<NpcWorldIntegrationManifest>(path);
            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<NpcWorldIntegrationManifest>();
                AssetDatabase.CreateAsset(manifest, path);
            }

            var so = new SerializedObject(manifest);
            var array = so.FindProperty("entries");
            array.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = entries[i];
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manifest);
        }

        private static void CreateDialogueProfiles(List<NpcWorldEntryDefinition> entries)
        {
            foreach (var entry in entries)
            {
                string path = $"{DialogueDir}/{Sanitize(entry.NpcId)}_Dialogue.asset";
                var profile = AssetDatabase.LoadAssetAtPath<NpcConversationProfile>(path);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<NpcConversationProfile>();
                    AssetDatabase.CreateAsset(profile, path);
                }

                profile.NpcName = entry.DisplayName;
                profile.Role = $"{entry.TitleOrRank} in {entry.Region}. Role: {entry.Role}.";
                profile.Personality = string.IsNullOrWhiteSpace(entry.PersonalityNotes) ? "Grounded, brief, and in-character." : entry.PersonalityNotes;
                profile.LocalKnowledge = $"I am usually found at {entry.LocationName} in {entry.Region}. {entry.RelationshipSummary}";
                profile.Boundaries = "Do not reveal hidden plot secrets unless the player has earned them through quests.";
                profile.Greeting = BuildGreeting(entry);
                profile.UnknownResponse = "That is not something I can speak on yet, but the city has many stories for those who listen.";
                profile.KeywordResponses.Clear();
                profile.KeywordResponses.Add(new NpcKeywordResponse { Keyword = "element", Response = $"My element is {entry.PrimaryElement}{(string.IsNullOrWhiteSpace(entry.SecondaryElement) ? "" : ", with a trace of " + entry.SecondaryElement)}." });
                profile.KeywordResponses.Add(new NpcKeywordResponse { Keyword = "family", Response = string.IsNullOrWhiteSpace(entry.RelationshipSummary) ? "Family shapes the fate of every capital." : entry.RelationshipSummary });
                profile.KeywordResponses.Add(new NpcKeywordResponse { Keyword = "where", Response = $"You can find me at {entry.LocationName} in {entry.Region}." });
                profile.KeywordResponses.Add(new NpcKeywordResponse { Keyword = "capital", Response = $"The {entry.Region} is strong because its people endure, learn, and build together." });
                EditorUtility.SetDirty(profile);
            }
        }

        private static string BuildGreeting(NpcWorldEntryDefinition entry)
        {
            if (entry.NpcId == "queen_alexis_malachite")
            {
                return "Welcome to the earth capital. Strength means very little without wisdom and kindness.";
            }

            if (entry.NpcId == "chrief_gover")
            {
                return "Welcome, traveler. Reefwood taught me that water finds every path; this palace teaches me which paths are worth protecting.";
            }

            if (entry.NpcId == "tatiana_tibidi_malachite")
            {
                return "Hi! Most people call me Tibidi. The garden listens better than some grown-ups do.";
            }

            if (entry.NpcId == "queen_seraphine_cindervale")
            {
                return "Welcome to the Fire Capital. A calm heart governs flame better than rage ever could.";
            }

            if (entry.NpcId == "prince_consort_oren_ashmantle")
            {
                return "The city survives because discipline stands beside passion. Remember that if you mean to stay near the caldera.";
            }

            if (entry.NpcId == "crown_prince_kaelen_cindervale")
            {
                return "If you are looking for the future of the Fire Capital, I intend to be worthy of it.";
            }

            if (entry.NpcId == "princess_lyra_cindervale")
            {
                return "Court politics burn almost as hot as the volcano. Smile, listen, and try not to step in either.";
            }

            if (entry.NpcId == "dowager_queen_maelis_pyre")
            {
                return "Every ruler thinks they command the mountain. The wise ones learn they merely bargain with it.";
            }

            if (entry.Role == NpcWorldRole.RoyalFamily)
            {
                return $"Welcome. I am {entry.DisplayName}.";
            }

            if (entry.Role == NpcWorldRole.Villain)
            {
                return "You should not have come here.";
            }

            return $"Greetings. I am {entry.DisplayName}.";
        }

        private static void CreateEarthCapitalRoyalQuests(List<NpcWorldEntryDefinition> entries)
        {
            var queen = entries.Find(e => e.NpcId == "queen_alexis_malachite");
            var chrief = entries.Find(e => e.NpcId == "chrief_gover");
            var tibidi = entries.Find(e => e.NpcId == "tatiana_tibidi_malachite");

            if (queen != null && chrief != null)
            {
                CreateQuest(
                    "earth_royal_audience",
                    "Audience with the Malachite Court",
                    "Meet Queen Alexis Malachite and Chrief Gover to learn how the earth capital balances strength, diplomacy, and elemental unity.",
                    "Earth Capital",
                    queen.DisplayName,
                    new QuestObjectiveUiDefinition { ObjectiveId = "meet_queen_alexis", Title = "Speak with Queen Alexis", Description = "Find Queen Alexis Malachite in the Royal Palace.", WorldPosition = queen.WorldPosition, CreateWaypoint = true, Required = true },
                    new QuestObjectiveUiDefinition { ObjectiveId = "meet_chrief_gover", Title = "Speak with Chrief Gover", Description = "Speak with Chrief Gover about Neritha Reefwood and the earth capital alliance.", WorldPosition = chrief.WorldPosition, CreateWaypoint = true, Required = true });
            }

            if (tibidi != null)
            {
                CreateQuest(
                    "tibidis_garden_lesson",
                    "Tibidi's Garden Lesson",
                    "Princess Tatiana, nicknamed Tibidi, can teach the player how strong plant channeling grows from patience and care.",
                    "Earth Capital",
                    tibidi.DisplayName,
                    new QuestObjectiveUiDefinition { ObjectiveId = "find_tibidi", Title = "Find Tibidi in the Palace Gardens", Description = "Look for Princess Tatiana in the gardens near the palace.", WorldPosition = tibidi.WorldPosition, CreateWaypoint = true, Required = true },
                    new QuestObjectiveUiDefinition { ObjectiveId = "observe_plants", Title = "Observe the channeling plants", Description = "Study how Tibidi guides plant growth without forcing it.", WorldPosition = tibidi.WorldPosition + new Vector3(3f, 0f, 2f), CreateWaypoint = true, Required = true });
            }
        }


private static void CreateFireCapitalRoyalQuests(List<NpcWorldEntryDefinition> entries)
{
    var queen = entries.Find(e => e.NpcId == "queen_seraphine_cindervale");
    var consort = entries.Find(e => e.NpcId == "prince_consort_oren_ashmantle");
    var prince = entries.Find(e => e.NpcId == "crown_prince_kaelen_cindervale");
    var princess = entries.Find(e => e.NpcId == "princess_lyra_cindervale");

    if (queen != null && consort != null)
    {
        CreateQuest(
            "fire_royal_audience",
            "Audience at the Caldera Throne",
            "Meet Queen Seraphine and Prince-Consort Oren to learn how the Fire Capital balances volcanic danger, royal duty, and the passions of its people.",
            "Fire Capital",
            queen.DisplayName,
            new QuestObjectiveUiDefinition { ObjectiveId = "meet_seraphine", Title = "Speak with Queen Seraphine", Description = "Seek the queen near the Caldera Throne.", WorldPosition = queen.WorldPosition, CreateWaypoint = true, Required = true },
            new QuestObjectiveUiDefinition { ObjectiveId = "meet_oren", Title = "Speak with Oren Ashmantle", Description = "Find Prince-Consort Oren in Ember Hall.", WorldPosition = consort.WorldPosition, CreateWaypoint = true, Required = true });
    }

    if (prince != null && princess != null)
    {
        CreateQuest(
            "embers_of_heirship",
            "Embers of Heirship",
            "Help Crown Prince Kaelen and Princess Lyra handle both military pressure and public confidence inside the Fire Capital.",
            "Fire Capital",
            prince.DisplayName,
            new QuestObjectiveUiDefinition { ObjectiveId = "meet_kaelen", Title = "Meet Kaelen on Ashwing Terrace", Description = "Find the crown prince near the training terraces.", WorldPosition = prince.WorldPosition, CreateWaypoint = true, Required = true },
            new QuestObjectiveUiDefinition { ObjectiveId = "meet_lyra", Title = "Speak with Princess Lyra", Description = "Discuss public tensions with Lyra on the Glassfire Balcony.", WorldPosition = princess.WorldPosition, CreateWaypoint = true, Required = true });
    }
}

        private static void CreateQuest(string id, string title, string description, string region, string giverName, params QuestObjectiveUiDefinition[] objectives)
        {
            string path = $"{QuestDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<QuestUiDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<QuestUiDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
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
            reward.FindPropertyRelative("RewardId").stringValue = id + "_reputation";
            reward.FindPropertyRelative("DisplayName").stringValue = "Earth Capital trust";
            reward.FindPropertyRelative("Quantity").intValue = 1;
            reward.FindPropertyRelative("Currency").intValue = 25;
            reward.FindPropertyRelative("SkillPoints").intValue = 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        private static void CreateIntegrationPrefab()
        {
            string manifestPath = $"{IntegrationDir}/NpcWorldIntegrationManifest.asset";
            var manifest = AssetDatabase.LoadAssetAtPath<NpcWorldIntegrationManifest>(manifestPath);
            if (manifest == null)
            {
                return;
            }

            GameObject go = new GameObject("Elementborn_NpcWorldIntegrationManager");
            var manager = go.AddComponent<NpcWorldIntegrationManager>();
            var so = new SerializedObject(manager);
            so.FindProperty("manifest").objectReferenceValue = manifest;
            so.FindProperty("registerMarkersOnStart").boolValue = true;
            so.FindProperty("addJournalEntriesOnStart").boolValue = true;
            so.FindProperty("spawnPlaceholderNpcsOnStart").boolValue = true;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_NpcWorldIntegrationManager.prefab");
            Object.DestroyImmediate(go);
        }

        private static void WriteReport(List<NpcWorldEntryDefinition> entries)
        {
            string reportDir = "Assets/Elementborn/Generated/Reports";
            Directory.CreateDirectory(reportDir);
            string path = $"{reportDir}/NpcWorldIntegrationReport.md";
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("# NPC World Integration Report");
                writer.WriteLine();
                writer.WriteLine($"Generated NPC count: {entries.Count}");
                writer.WriteLine();
                writer.WriteLine("## Generated assets");
                writer.WriteLine();
                writer.WriteLine("```text");
                writer.WriteLine("Assets/Elementborn/Generated/NPC/Integration/NpcWorldIntegrationManifest.asset");
                writer.WriteLine("Assets/Elementborn/Generated/NPC/DialogueProfiles");
                writer.WriteLine("Assets/Elementborn/Generated/QuestUI/NPC");
                writer.WriteLine("Assets/Elementborn/Generated/Prefabs/NPC/Elementborn_NpcWorldIntegrationManager.prefab");
                writer.WriteLine("```");
                writer.WriteLine();
                writer.WriteLine("## NPCs");
                foreach (var entry in entries)
                {
                    writer.WriteLine($"- **{entry.DisplayName}** — {entry.TitleOrRank}, {entry.Region}, {entry.LocationName}");
                }
            }
        }

        private static string Sanitize(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }
            return value.Replace(" ", "_").ToLowerInvariant();
        }
    }
}
#endif
