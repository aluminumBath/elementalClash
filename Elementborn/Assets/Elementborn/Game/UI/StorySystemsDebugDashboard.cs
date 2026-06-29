
using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class StorySystemsDebugDashboard : MonoBehaviour
    {
        [SerializeField] private Text outputText;
        [SerializeField] private bool refreshOnEnable = true;
        [SerializeField] private bool includeCapitalWorldState = true;
        [SerializeField] private bool includeGameplayLoop = true;
        [SerializeField] private bool includeFireCapital = true;
        [SerializeField] private bool includePoliticalEvents = true;
        [SerializeField] private bool includeQuestChains = true;
        [SerializeField] private bool includeSocialNpcs = true;
        [SerializeField] private bool includeSocialGroups = true;
        [SerializeField] private bool includeCreatureOrphanage = true;
        [SerializeField] private bool includeStoryEncounters = true;
        [SerializeField] private bool includeNarrativeSavePaths = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F10;
        [SerializeField] private GameObject panelRoot;

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }
        }

        private void OnEnable()
        {
            if (refreshOnEnable)
            {
                Refresh();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            panelRoot.SetActive(!panelRoot.activeSelf);
            if (panelRoot.activeSelf)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            string text = BuildDashboardText();
            if (outputText != null)
            {
                outputText.text = text;
            }
            else
            {
                Debug.Log(text);
            }
        }

        public void SaveNarrativeSlotZero()
        {
            NarrativeRuntimeSaveBridge bridge = FindObjectOfType<NarrativeRuntimeSaveBridge>();
            if (bridge == null)
            {
                var go = new GameObject(nameof(NarrativeRuntimeSaveBridge));
                bridge = go.AddComponent<NarrativeRuntimeSaveBridge>();
            }

            bridge.SaveSlot(0);
            Refresh();
        }

        public void LoadNarrativeSlotZero()
        {
            NarrativeRuntimeSaveBridge bridge = FindObjectOfType<NarrativeRuntimeSaveBridge>();
            if (bridge == null)
            {
                var go = new GameObject(nameof(NarrativeRuntimeSaveBridge));
                bridge = go.AddComponent<NarrativeRuntimeSaveBridge>();
            }

            bridge.LoadSlot(0);
            Refresh();
        }

        public void StartGameplayLoop()
        {
            ElementbornMainGameplayLoopDirector.Ensure().StartGame();
            Refresh();
        }

        public void SpawnStarterWave()
        {
            ElementbornMainGameplayLoopDirector.Ensure().SpawnStarterWaves();
            Refresh();
        }

        public void TriggerFirstFireCapitalHook()
        {
            var registry = FireCapitalRegistry.Ensure();
            if (registry.Hooks.Count > 0 && registry.Hooks[0] != null)
            {
                registry.StartHook(registry.Hooks[0].HookId);
            }
            Refresh();
        }

        public void PulseFireVolcano()
        {
            FireCapitalVolcanoHazardController volcano = ElementbornFindUtility.FindFirst<FireCapitalVolcanoHazardController>();
            if (volcano == null)
            {
                volcano = new GameObject(nameof(FireCapitalVolcanoHazardController)).AddComponent<FireCapitalVolcanoHazardController>();
            }
            volcano.PulseVolcanoPressure();
            Refresh();
        }

        public void EvaluatePoliticalEvents()
        {
            PoliticalWorldEventDirector.Ensure().EvaluateAll();
            Refresh();
        }

        public void SyncCapitalWorldState()
        {
            CapitalWorldStateTracker.Ensure().SyncRegionalSystems();
            Refresh();
        }

        public void TriggerFirstSocialGroupEvent()
        {
            var registry = SocialGroupRegistry.Ensure();
            if (registry.Events.Count > 0 && registry.Events[0] != null)
            {
                registry.ActivateEvent(registry.Events[0].EventId);
            }
            Refresh();
        }

        public void AdmitDemoOrphanageResident()
        {
            CreatureOrphanageRecoveryRegistry.Ensure().AdmitCreature(
                "demo_emberfox",
                "Demo Emberfox",
                CreatureOrphanageDepartureReason.RanAway,
                "Admitted from the dashboard for a quick playtest of recovery flows.");
            Refresh();
        }

        public void ForceRespawnWolfPack()
        {
            TimedDualLeaderPackRespawnController controller = ElementbornFindUtility.FindFirst<TimedDualLeaderPackRespawnController>();
            if (controller != null)
            {
                controller.ForceRespawnPack();
            }
            Refresh();
        }

        public string BuildDashboardText()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ELEMENTBORN STORY SYSTEMS DEBUG");
            sb.AppendLine("================================");
            sb.AppendLine();

            if (includeGameplayLoop)
            {
                sb.AppendLine("[Gameplay Loop]");
                sb.AppendLine(ElementbornMainGameplayLoopDirector.Ensure().BuildSummary());
            }

            if (includeFireCapital)
            {
                sb.AppendLine("[Fire Capital]");
                sb.AppendLine(FireCapitalRegistry.Ensure().BuildSummary());
            }

            if (includeCapitalWorldState)
            {
                sb.AppendLine("[Capital World State]");
                sb.AppendLine(CapitalWorldStateTracker.Ensure().BuildWorldSummary());
            }

            if (includePoliticalEvents)
            {
                sb.AppendLine("[Political Events]");
                sb.AppendLine(PoliticalWorldEventDirector.Ensure().BuildSummary());
            }

            if (includeQuestChains)
            {
                sb.AppendLine("[Quest Chains]");
                sb.AppendLine(QuestChainDirector.Ensure().BuildSummary());
            }

            if (includeSocialNpcs)
            {
                sb.AppendLine("[Social NPCs]");
                sb.AppendLine(SocialNpcDialogueRegistry.Ensure().BuildSummary());
            }

            if (includeSocialGroups)
            {
                sb.AppendLine("[Social Groups]");
                sb.AppendLine(SocialGroupRegistry.Ensure().BuildSummary());
            }

            if (includeCreatureOrphanage)
            {
                sb.AppendLine("[Creature Orphanage Recovery]");
                sb.AppendLine(CreatureOrphanageRecoveryRegistry.Ensure().BuildSummary());
            }

            if (includeStoryEncounters)
            {
                sb.AppendLine("[Story Encounters]");
                sb.AppendLine(StoryEncounterProgressTracker.Ensure().BuildSummary());
            }

            if (includeNarrativeSavePaths)
            {
                sb.AppendLine("[Narrative Save Paths]");
                sb.AppendLine(CapitalWorldStateSaveBridge.GetPath(0));
                sb.AppendLine(PoliticalWorldEventDirectorSaveBridge.GetPath(0));
                sb.AppendLine(QuestChainSaveBridge.GetPath(0));
                sb.AppendLine(StoryEncounterProgressSaveBridge.GetPath(0));
                sb.AppendLine(CreatureOrphanageRecoverySaveBridge.GetPath(0));
                sb.AppendLine(SocialGroupSaveBridge.GetPath(0));
            }

            return sb.ToString();
        }
    }
}
