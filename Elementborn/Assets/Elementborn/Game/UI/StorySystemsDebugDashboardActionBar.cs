
using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class StorySystemsDebugDashboardActionBar : MonoBehaviour
    {
        [SerializeField] private StorySystemsDebugDashboard dashboard;
        [SerializeField] private Button startLoopButton;
        [SerializeField] private Button spawnWaveButton;
        [SerializeField] private Button fireHookButton;
        [SerializeField] private Button fireVolcanoButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button evaluatePoliticalButton;
        [SerializeField] private Button syncCapitalsButton;
        [SerializeField] private Button socialEventButton;
        [SerializeField] private Button admitOrphanageButton;
        [SerializeField] private Button wolfPackButton;

        private void Awake()
        {
            if (dashboard == null)
            {
                dashboard = GetComponent<StorySystemsDebugDashboard>();
            }

            Wire(startLoopButton, () => dashboard?.StartGameplayLoop());
            Wire(spawnWaveButton, () => dashboard?.SpawnStarterWave());
            Wire(fireHookButton, () => dashboard?.TriggerFirstFireCapitalHook());
            Wire(fireVolcanoButton, () => dashboard?.PulseFireVolcano());
            Wire(refreshButton, () => dashboard?.Refresh());
            Wire(saveButton, () => dashboard?.SaveNarrativeSlotZero());
            Wire(loadButton, () => dashboard?.LoadNarrativeSlotZero());
            Wire(evaluatePoliticalButton, () => dashboard?.EvaluatePoliticalEvents());
            Wire(syncCapitalsButton, () => dashboard?.SyncCapitalWorldState());
            Wire(socialEventButton, () => dashboard?.TriggerFirstSocialGroupEvent());
            Wire(admitOrphanageButton, () => dashboard?.AdmitDemoOrphanageResident());
            Wire(wolfPackButton, () => dashboard?.ForceRespawnWolfPack());
        }

        private void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
