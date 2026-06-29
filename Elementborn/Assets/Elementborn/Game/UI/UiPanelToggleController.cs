using UnityEngine;

namespace Elementborn.Game
{
    public sealed class UiPanelToggleController : MonoBehaviour
    {
        [SerializeField] private GameObject questLogPanel;
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private KeyCode questLogKey = KeyCode.J;
        [SerializeField] private KeyCode debugKey = KeyCode.F9;

        private void Update()
        {
            if (questLogPanel != null && Input.GetKeyDown(questLogKey))
            {
                questLogPanel.SetActive(!questLogPanel.activeSelf);
            }

            if (debugPanel != null && Input.GetKeyDown(debugKey))
            {
                debugPanel.SetActive(!debugPanel.activeSelf);
            }
        }

        public void ToggleQuestLog()
        {
            if (questLogPanel != null)
            {
                questLogPanel.SetActive(!questLogPanel.activeSelf);
            }
        }

        public void ToggleDebugPanel()
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(!debugPanel.activeSelf);
            }
        }
    }
}
