using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPlaytestHarnessController : MonoBehaviour
    {
        public static ElementbornPlaytestHarnessController Instance { get; private set; }

        [SerializeField] private bool logActions = true;

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

        public static ElementbornPlaytestHarnessController Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(ElementbornPlaytestHarnessController));
            return go.AddComponent<ElementbornPlaytestHarnessController>();
        }

        public void StartLoop()
        {
            ElementbornMainGameplayLoopDirector.Ensure().StartGame();
            Log("Started main loop.");
        }

        public void SpawnWave()
        {
            ElementbornMainGameplayLoopDirector.Ensure().SpawnStarterWaves();
            Log("Spawned starter wave.");
        }

        public void TriggerFireIntro()
        {
            FireCapitalRegistry.Ensure().StartHook("fire_caldera_audience_hook");
            Log("Started Fire Capital intro hook.");
        }

        public void TriggerSocialEvent()
        {
            SocialGroupRegistry.Ensure().ActivateEvent("marie_sleeping_flare_social");
            Log("Triggered social chaos event.");
        }

        public void AdmitDemoCreature()
        {
            CreatureOrphanageRecoveryRegistry.Ensure().AdmitCreature(
                "demo_readiness_emberfox",
                "Readiness Emberfox",
                CreatureOrphanageDepartureReason.RanAway,
                "Admitted from playtest harness.");
            Log("Admitted demo creature.");
        }

        public void SaveSlotZero()
        {
            NarrativeRuntimeSaveBridge bridge = ElementbornFindUtility.FindFirst<NarrativeRuntimeSaveBridge>();
            if (bridge == null)
            {
                bridge = new GameObject(nameof(NarrativeRuntimeSaveBridge)).AddComponent<NarrativeRuntimeSaveBridge>();
            }

            bridge.SaveSlot(0);
            Log("Saved narrative slot 0.");
        }

        public void LoadSlotZero()
        {
            NarrativeRuntimeSaveBridge bridge = ElementbornFindUtility.FindFirst<NarrativeRuntimeSaveBridge>();
            if (bridge == null)
            {
                bridge = new GameObject(nameof(NarrativeRuntimeSaveBridge)).AddComponent<NarrativeRuntimeSaveBridge>();
            }

            bridge.LoadSlot(0);
            Log("Loaded narrative slot 0.");
        }

        public void ResetRuntime()
        {
            ElementbornPlaytestResetService.Ensure().ResetRuntimeState(deleteSaves: false);
            Log("Reset runtime state.");
        }

        public void ResetSavesAndRuntime()
        {
            ElementbornPlaytestResetService.Ensure().ResetRuntimeState(deleteSaves: true);
            Log("Reset saves and runtime state.");
        }

        public void WriteReadinessReport()
        {
            string path = ElementbornTestReadinessScanner.Ensure().WritePersistentReport();
            Log("Wrote readiness report: " + path);
        }

        public void ApplyStableFirePreset()
        {
            ElementbornPlaytestPresetService.Ensure().ApplyPreset(ElementbornPlaytestPreset.StableFireCapital);
            Log("Applied stable Fire Capital preset.");
        }

        public void ApplyChaosPreset()
        {
            ElementbornPlaytestPresetService.Ensure().ApplyPreset(ElementbornPlaytestPreset.FireCapitalInChaos);
            Log("Applied Fire Capital chaos preset.");
        }

        public void ApplyCleanPreset()
        {
            ElementbornPlaytestPresetService.Ensure().ApplyPreset(ElementbornPlaytestPreset.CleanFreshPlaytest);
            Log("Applied clean fresh playtest preset.");
        }

        public void TeleportFireCapital() => Teleport(ElementbornPlaytestTeleportTarget.FireCapital);
        public void TeleportOrphanage() => Teleport(ElementbornPlaytestTeleportTarget.CreatureOrphanage);
        public void TeleportWolfPack() => Teleport(ElementbornPlaytestTeleportTarget.WolfPack);
        public void TeleportBoat() => Teleport(ElementbornPlaytestTeleportTarget.Boat);
        public void TeleportCentralCity() => Teleport(ElementbornPlaytestTeleportTarget.CentralCity);

        public void Teleport(ElementbornPlaytestTeleportTarget target)
        {
            GameObject player = FindPlayer();
            if (player == null)
            {
                NotificationFeed.Post("Cannot teleport: player not found.", NotificationType.Info);
                return;
            }

            Vector3 destination = ResolveTarget(target);
            player.transform.position = destination;
            NotificationFeed.Post("Teleported to " + target, NotificationType.Info);
            Log("Teleported to " + target);
        }

        private Vector3 ResolveTarget(ElementbornPlaytestTeleportTarget target)
        {
            ElementbornSpawnRole role = ElementbornSpawnRole.PlayerStart;
            switch (target)
            {
                case ElementbornPlaytestTeleportTarget.FireCapital:
                    role = ElementbornSpawnRole.FireCapitalStart;
                    break;
                case ElementbornPlaytestTeleportTarget.CreatureOrphanage:
                    role = ElementbornSpawnRole.CreatureOrphanage;
                    break;
                case ElementbornPlaytestTeleportTarget.WolfPack:
                    role = ElementbornSpawnRole.WolfPack;
                    break;
                case ElementbornPlaytestTeleportTarget.Boat:
                    role = ElementbornSpawnRole.Boat;
                    break;
                case ElementbornPlaytestTeleportTarget.CentralCity:
                    return new Vector3(0f, 1f, 10f);
            }

            ElementbornSpawnPoint point = ElementbornSpawnRegistry.Ensure().FindFirst(role);
            if (point != null)
            {
                return point.transform.position;
            }

            switch (target)
            {
                case ElementbornPlaytestTeleportTarget.FireCapital: return new Vector3(-70f, 1f, -58f);
                case ElementbornPlaytestTeleportTarget.CreatureOrphanage: return new Vector3(-8f, 1f, -13f);
                case ElementbornPlaytestTeleportTarget.WolfPack: return new Vector3(22f, 1f, -18f);
                case ElementbornPlaytestTeleportTarget.Boat: return new Vector3(-24f, 1f, 24f);
                default: return Vector3.up;
            }
        }

        private GameObject FindPlayer()
        {
            try
            {
                GameObject tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null)
                {
                    return tagged;
                }
            }
            catch { }

            return GameObject.Find("Player Test Rig");
        }

        private void Log(string message)
        {
            if (logActions)
            {
                Debug.Log("Playtest Harness: " + message);
            }
        }
    }
}
