using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPlaytestResetService : MonoBehaviour
    {
        public static ElementbornPlaytestResetService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static ElementbornPlaytestResetService Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(ElementbornPlaytestResetService));
            return go.AddComponent<ElementbornPlaytestResetService>();
        }

        public void ResetRuntimeState(bool deleteSaves = false)
        {
            ClearListField(CapitalWorldStateTracker.Ensure(), "runtimeStates");
            ClearListField(PoliticalWorldEventDirector.Ensure(), "runtimeRecords");
            ClearListField(QuestChainDirector.Ensure(), "runtimeRecords");
            ClearListField(SocialGroupRegistry.Ensure(), "runtimeRecords");
            ClearListField(CreatureOrphanageRecoveryRegistry.Ensure(), "residents");
            ClearListField(StoryEncounterProgressTracker.Ensure(), "records");
            ClearListField(FireCapitalRegistry.Ensure(), "records");

            StoryEncounterProgressTracker.Ensure().ReplaceRecords(new List<StoryEncounterRuntimeRecord>());

            GameObject spawned = GameObject.Find("Spawned Gameplay Objects");
            if (spawned != null)
            {
                Destroy(spawned);
            }

            if (deleteSaves)
            {
                DeleteSaveDirectory("capital_world_state");
                DeleteSaveDirectory("political_world_events");
                DeleteSaveDirectory("quest_chains");
                DeleteSaveDirectory("story_encounters");
                DeleteSaveDirectory("creature_orphanage_recovery");
                DeleteSaveDirectory("social_groups");
                DeleteSaveDirectory("test_readiness");
            }

            NotificationFeed.Post(deleteSaves ? "Playtest state and saves reset." : "Playtest runtime state reset.", NotificationType.Info);
        }

        public void ResetAndStartFresh()
        {
            ResetRuntimeState(deleteSaves: true);
            ElementbornMainGameplayLoopDirector.Ensure().StartGame();
        }

        private void DeleteSaveDirectory(string folderName)
        {
            string path = Path.Combine(Application.persistentDataPath, folderName);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        private void ClearListField(object target, string fieldName)
        {
            if (target == null)
            {
                return;
            }

            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                return;
            }

            object value = field.GetValue(target);
            if (value is IList list)
            {
                list.Clear();
            }
        }
    }
}
