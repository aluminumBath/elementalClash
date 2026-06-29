using UnityEngine;

namespace Elementborn.Game
{
    public sealed class NarrativeRuntimeSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot = 0;
        [SerializeField] private CapitalWorldStateSaveBridge capitalWorldState;
        [SerializeField] private PoliticalWorldEventDirectorSaveBridge politicalWorldEvents;
        [SerializeField] private QuestChainSaveBridge questChains;
        [SerializeField] private CreatureOrphanageRecoverySaveBridge creatureOrphanageRecovery;
        [SerializeField] private bool autoLoadOnStart = false;
        [SerializeField] private bool autoSaveOnApplicationPause = true;
        [SerializeField] private bool autoSaveOnApplicationQuit = true;

        private void Awake()
        {
            EnsureChildBridges();
        }

        private void Start()
        {
            if (autoLoadOnStart)
            {
                LoadCurrentSlot();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && autoSaveOnApplicationPause)
            {
                SaveCurrentSlot();
            }
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnApplicationQuit)
            {
                SaveCurrentSlot();
            }
        }

        public void SetCurrentSlot(int slot)
        {
            currentSlot = Mathf.Max(0, slot);
            EnsureChildBridges();
            capitalWorldState.SetCurrentSlot(currentSlot);
            politicalWorldEvents.SetCurrentSlot(currentSlot);
            questChains.SetCurrentSlot(currentSlot);
            creatureOrphanageRecovery.SetCurrentSlot(currentSlot);
        }

        public void SaveCurrentSlot() => SaveSlot(currentSlot);
        public void LoadCurrentSlot() => LoadSlot(currentSlot);

        public void SaveSlot(int slot)
        {
            SetCurrentSlot(slot);
            capitalWorldState.SaveSlot(slot);
            politicalWorldEvents.SaveSlot(slot);
            questChains.SaveSlot(slot);
            creatureOrphanageRecovery.SaveSlot(slot);
            Debug.Log($"Saved narrative runtime systems for slot {slot}.");
        }

        public void LoadSlot(int slot)
        {
            SetCurrentSlot(slot);
            capitalWorldState.LoadSlot(slot);
            politicalWorldEvents.LoadSlot(slot);
            questChains.LoadSlot(slot);
            creatureOrphanageRecovery.LoadSlot(slot);
            Debug.Log($"Loaded narrative runtime systems for slot {slot}.");
        }

        public void DeleteSlot(int slot)
        {
            EnsureChildBridges();
            capitalWorldState.DeleteSlot(slot);
            politicalWorldEvents.DeleteSlot(slot);
            questChains.DeleteSlot(slot);
            creatureOrphanageRecovery.DeleteSlot(slot);
            Debug.Log($"Deleted narrative runtime save files for slot {slot}.");
        }

        private void EnsureChildBridges()
        {
            if (capitalWorldState == null)
            {
                capitalWorldState = GetComponent<CapitalWorldStateSaveBridge>();
                if (capitalWorldState == null) capitalWorldState = gameObject.AddComponent<CapitalWorldStateSaveBridge>();
            }

            if (politicalWorldEvents == null)
            {
                politicalWorldEvents = GetComponent<PoliticalWorldEventDirectorSaveBridge>();
                if (politicalWorldEvents == null) politicalWorldEvents = gameObject.AddComponent<PoliticalWorldEventDirectorSaveBridge>();
            }

            if (questChains == null)
            {
                questChains = GetComponent<QuestChainSaveBridge>();
                if (questChains == null) questChains = gameObject.AddComponent<QuestChainSaveBridge>();
            }

            if (creatureOrphanageRecovery == null)
            {
                creatureOrphanageRecovery = GetComponent<CreatureOrphanageRecoverySaveBridge>();
                if (creatureOrphanageRecovery == null)
                {
                    creatureOrphanageRecovery = gameObject.AddComponent<CreatureOrphanageRecoverySaveBridge>();
                }
            }

        }
    }
}
