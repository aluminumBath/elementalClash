using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class CombatDefenseDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private CombatDefenseController defense;
        [SerializeField] private bool refreshEveryFrame = true;

        private void Reset()
        {
            text = GetComponentInChildren<Text>();
        }

        private void Update()
        {
            if (refreshEveryFrame)
            {
                Refresh();
            }
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (text == null)
            {
                return;
            }

            if (defense == null)
            {
                text.text = "No CombatDefenseController assigned.";
                return;
            }

            StaminaResource stamina = defense.Stamina;
            text.text =
                $"Defense: {defense.State}\n" +
                $"Stamina: {(stamina != null ? stamina.CurrentStamina : 0f):0.#}/{(stamina != null ? stamina.MaxStamina : 0f):0.#}\n" +
                $"Blocking: {defense.IsBlocking}\n" +
                $"Dodging: {defense.IsDodging}\n" +
                $"I-Frames: {defense.IsInvulnerable}";
        }
    }
}
