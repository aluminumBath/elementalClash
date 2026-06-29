using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class ElementbornPlaytestHarnessPanel : MonoBehaviour
    {
        [SerializeField] private ElementbornPlaytestHarnessController controller;
        [SerializeField] private Button startLoopButton;
        [SerializeField] private Button teleportFireButton;
        [SerializeField] private Button teleportOrphanageButton;
        [SerializeField] private Button teleportWolfButton;
        [SerializeField] private Button spawnWaveButton;
        [SerializeField] private Button fireIntroButton;
        [SerializeField] private Button socialEventButton;
        [SerializeField] private Button admitCreatureButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button resetRuntimeButton;
        [SerializeField] private Button resetSavesButton;
        [SerializeField] private Button readinessReportButton;
        [SerializeField] private Button stablePresetButton;
        [SerializeField] private Button chaosPresetButton;
        [SerializeField] private Button cleanPresetButton;

        private void Awake()
        {
            if (controller == null)
            {
                controller = ElementbornPlaytestHarnessController.Ensure();
            }

            Wire();
        }

        private void Wire()
        {
            Wire(startLoopButton, () => controller.StartLoop());
            Wire(teleportFireButton, () => controller.TeleportFireCapital());
            Wire(teleportOrphanageButton, () => controller.TeleportOrphanage());
            Wire(teleportWolfButton, () => controller.TeleportWolfPack());
            Wire(spawnWaveButton, () => controller.SpawnWave());
            Wire(fireIntroButton, () => controller.TriggerFireIntro());
            Wire(socialEventButton, () => controller.TriggerSocialEvent());
            Wire(admitCreatureButton, () => controller.AdmitDemoCreature());
            Wire(saveButton, () => controller.SaveSlotZero());
            Wire(loadButton, () => controller.LoadSlotZero());
            Wire(resetRuntimeButton, () => controller.ResetRuntime());
            Wire(resetSavesButton, () => controller.ResetSavesAndRuntime());
            Wire(readinessReportButton, () => controller.WriteReadinessReport());
            Wire(stablePresetButton, () => controller.ApplyStableFirePreset());
            Wire(chaosPresetButton, () => controller.ApplyChaosPreset());
            Wire(cleanPresetButton, () => controller.ApplyCleanPreset());
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
