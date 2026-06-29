using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SpellLoadoutHudView : MonoBehaviour
    {
        [SerializeField] private SpellLoadoutController loadout;
        [SerializeField] private SpellCooldownHudSlot[] hudSlots;

        private void Start()
        {
            Refresh();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (loadout == null || hudSlots == null)
            {
                return;
            }

            foreach (var slot in loadout.Slots)
            {
                if (slot == null || slot.SlotIndex < 0 || slot.SlotIndex >= hudSlots.Length)
                {
                    continue;
                }

                if (hudSlots[slot.SlotIndex] != null)
                {
                    hudSlots[slot.SlotIndex].SetSpell(slot.Spell);
                }
            }
        }
    }
}
