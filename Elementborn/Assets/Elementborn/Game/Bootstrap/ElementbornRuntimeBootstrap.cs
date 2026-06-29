using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool createOnAwake = true;

        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (createOnAwake)
            {
                EnsureRuntimeSystems();
            }
        }

        [ContextMenu("Ensure Runtime Systems")]
        public void EnsureRuntimeSystems()
        {
            EnsureSingleton<PlayerInventoryTracker>("PlayerInventoryTracker");
            EnsureSingleton<PlayerMapMarkerTracker>("PlayerMapMarkerTracker");
            EnsureSingleton<WaypointTracker>("WaypointTracker");
            EnsureSingleton<NotificationFeed>("NotificationFeed");
            EnsureSingleton<PlayerJournalTracker>("PlayerJournalTracker");
            EnsureSingleton<FactionReputationTracker>("FactionReputationTracker");
            EnsureSingleton<RecipeBookTracker>("RecipeBookTracker");
            EnsureSingleton<CreatureBondingTracker>("CreatureBondingTracker");
            EnsureSingleton<PlayerAbilityTracker>("PlayerAbilityTracker");
            EnsureSingleton<PlayerEquipmentTracker>("PlayerEquipmentTracker");
            EnsureSingleton<WorldEventTracker>("WorldEventTracker");
            EnsureSingleton<ResourceHarvestingTracker>("ResourceHarvestingTracker");
            EnsureSingleton<SpellCooldownTracker>("SpellCooldownTracker");
            EnsureSingleton<QuestUiTracker>("QuestUiTracker");

            // Narrative / systemic playtest singletons.
            EnsureSingleton<CapitalWorldStateTracker>("CapitalWorldStateTracker");
            EnsureSingleton<PoliticalWorldEventDirector>("PoliticalWorldEventDirector");
            EnsureSingleton<QuestChainDirector>("QuestChainDirector");
            EnsureSingleton<SocialNpcDialogueRegistry>("SocialNpcDialogueRegistry");
            EnsureSingleton<SocialGroupRegistry>("SocialGroupRegistry");
            EnsureSingleton<StoryEncounterProgressTracker>("StoryEncounterProgressTracker");
            EnsureSingleton<CreatureOrphanageRecoveryRegistry>("CreatureOrphanageRecoveryRegistry");
            EnsureSingleton<FireCapitalRegistry>("FireCapitalRegistry");
            EnsureSingleton<ElementbornSpawnRegistry>("ElementbornSpawnRegistry");
            EnsureSingleton<ElementbornMainGameplayLoopDirector>("ElementbornMainGameplayLoopDirector");
            EnsureSingleton<AdminRuntimeCommandRouter>("AdminRuntimeCommandRouter");
            EnsureSingleton<AdminActionCatalog>("AdminActionCatalog");
            EnsureSingleton<AdminActionExecutor>("AdminActionExecutor");
            EnsureSingleton<ElementbornTestReadinessScanner>("ElementbornTestReadinessScanner");
            EnsureSingleton<ElementbornPlaytestResetService>("ElementbornPlaytestResetService");
            EnsureSingleton<ElementbornPlaytestPresetService>("ElementbornPlaytestPresetService");
            EnsureSingleton<ElementbornPlaytestHarnessController>("ElementbornPlaytestHarnessController");
            EnsureSingleton<ReligiousFervorTracker>("ReligiousFervorTracker");
            EnsureSingleton<ThievesGuildReputationTracker>("ThievesGuildReputationTracker");
            EnsureSingleton<WindCapitalSecretTracker>("WindCapitalSecretTracker");
            EnsureSingleton<HiddenChannelerSecretTracker>("HiddenChannelerSecretTracker");
            EnsureSingleton<ShipReputationTracker>("ShipReputationTracker");
        }

        public static T EnsureSingleton<T>(string objectName) where T : Component
        {
            T existing = ElementbornFindUtility.FindFirst<T>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(objectName);

            // v60 editor safety:
            // DontDestroyOnLoad is only legal during Play Mode. Several Elementborn editor menus
            // intentionally create runtime-system placeholders while building playable scenes.
            // In edit mode those objects should remain normal scene objects so the scene can be saved.
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(go);
            }

            return go.AddComponent<T>();
        }
    }
}
