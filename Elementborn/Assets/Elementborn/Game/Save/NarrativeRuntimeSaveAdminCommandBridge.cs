using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// narrative.save
    /// narrative.load
    /// narrative.save slot
    /// narrative.load slot
    /// narrative.delete slot
    /// </summary>
    public sealed class NarrativeRuntimeSaveAdminCommandBridge : MonoBehaviour
    {
        [SerializeField] private NarrativeRuntimeSaveBridge saveBridge;

        private void Awake()
        {
            if (saveBridge == null)
            {
                saveBridge = GetComponent<NarrativeRuntimeSaveBridge>();
                if (saveBridge == null)
                {
                    saveBridge = gameObject.AddComponent<NarrativeRuntimeSaveBridge>();
                }
            }
        }

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed == "narrative.save")
            {
                saveBridge.SaveCurrentSlot();
                return true;
            }

            if (trimmed == "narrative.load")
            {
                saveBridge.LoadCurrentSlot();
                return true;
            }

            if (trimmed.StartsWith("narrative.save "))
            {
                int slot = int.TryParse(trimmed.Substring("narrative.save ".Length).Trim(), out int parsed) ? parsed : 0;
                saveBridge.SaveSlot(slot);
                return true;
            }

            if (trimmed.StartsWith("narrative.load "))
            {
                int slot = int.TryParse(trimmed.Substring("narrative.load ".Length).Trim(), out int parsed) ? parsed : 0;
                saveBridge.LoadSlot(slot);
                return true;
            }

            if (trimmed.StartsWith("narrative.delete "))
            {
                int slot = int.TryParse(trimmed.Substring("narrative.delete ".Length).Trim(), out int parsed) ? parsed : 0;
                saveBridge.DeleteSlot(slot);
                return true;
            }

            return false;
        }
    }
}
