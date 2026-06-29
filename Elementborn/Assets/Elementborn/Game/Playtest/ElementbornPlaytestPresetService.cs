using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPlaytestPresetService : MonoBehaviour
    {
        public static ElementbornPlaytestPresetService Instance { get; private set; }

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

        public static ElementbornPlaytestPresetService Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(ElementbornPlaytestPresetService));
            return go.AddComponent<ElementbornPlaytestPresetService>();
        }

        public void ApplyPreset(ElementbornPlaytestPreset preset)
        {
            switch (preset)
            {
                case ElementbornPlaytestPreset.CleanFreshPlaytest:
                    ElementbornPlaytestResetService.Ensure().ResetRuntimeState(deleteSaves: true);
                    ElementbornMainGameplayLoopDirector.Ensure().StartGame();
                    break;

                case ElementbornPlaytestPreset.StableFireCapital:
                    CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.Unrest, -30, "Preset: Stable Fire Capital");
                    CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.HiddenThreat, -30, "Preset: Stable Fire Capital");
                    CapitalWorldStateTracker.Ensure().AddStability(CapitalId.FireCapital, 25, "Preset: Stable Fire Capital");
                    FireCapitalRegistry.Ensure().Discover("fire_caldera_audience_hook");
                    break;

                case ElementbornPlaytestPreset.FireCapitalInChaos:
                    CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.Unrest, 35, "Preset: Fire Capital in chaos");
                    CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.HiddenThreat, 30, "Preset: Fire Capital in chaos");
                    FireCapitalRegistry.Ensure().StartHook("fire_evacuation_drill_hook");
                    break;

                case ElementbornPlaytestPreset.WolfPackUnresolved:
                    StoryEncounterProgressTracker.Ensure().StartEncounter("romilus_madrangea_pack", "Preset: wolf pack unresolved");
                    TimedDualLeaderPackRespawnController wolf = ElementbornFindUtility.FindFirst<TimedDualLeaderPackRespawnController>();
                    if (wolf != null)
                    {
                        wolf.ForceRespawnPack();
                    }
                    break;

                case ElementbornPlaytestPreset.CreatureRanAway:
                    CreatureOrphanageRecoveryRegistry.Ensure().AdmitCreature(
                        "preset_runaway_emberfox",
                        "Preset Runaway Emberfox",
                        CreatureOrphanageDepartureReason.RanAway,
                        "Preset: creature ran away");
                    break;

                case ElementbornPlaytestPreset.SocialChaosActive:
                    SocialGroupRegistry.Ensure().ActivateEvent("marie_sleeping_flare_social");
                    SocialGroupRegistry.Ensure().ActivateEvent("amy_rumor_drift_social");
                    break;

                case ElementbornPlaytestPreset.FullSystemsReady:
                    ElementbornMainGameplayLoopDirector.Ensure().StartGame();
                    CapitalWorldStateTracker.Ensure().SyncRegionalSystems();
                    PoliticalWorldEventDirector.Ensure().EvaluateAll();
                    FireCapitalRegistry.Ensure().StartHook("fire_caldera_audience_hook");
                    CreatureOrphanageRecoveryRegistry.Ensure().AdmitCreature(
                        "preset_fullsystems_shellback",
                        "Preset Shellback",
                        CreatureOrphanageDepartureReason.Released,
                        "Preset: full systems ready");
                    break;
            }

            NotificationFeed.Post("Applied playtest preset: " + preset, NotificationType.Info);
        }
    }
}
