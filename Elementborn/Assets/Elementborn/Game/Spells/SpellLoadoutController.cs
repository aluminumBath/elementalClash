using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SpellLoadoutController : MonoBehaviour
    {
        [SerializeField] private SpellCastController caster;
        [SerializeField] private List<SpellLoadoutSlot> slots = new List<SpellLoadoutSlot>();
        [SerializeField] private KeyCode slot1 = KeyCode.Alpha1;
        [SerializeField] private KeyCode slot2 = KeyCode.Alpha2;
        [SerializeField] private KeyCode slot3 = KeyCode.Alpha3;
        [SerializeField] private KeyCode slot4 = KeyCode.Alpha4;

        public IReadOnlyList<SpellLoadoutSlot> Slots => slots;

        private void Awake()
        {
            if (caster == null)
            {
                caster = GetComponent<SpellCastController>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(slot1)) CastSlot(0);
            if (Input.GetKeyDown(slot2)) CastSlot(1);
            if (Input.GetKeyDown(slot3)) CastSlot(2);
            if (Input.GetKeyDown(slot4)) CastSlot(3);
        }

        public bool CastSlot(int index)
        {
            SpellLoadoutSlot slot = slots.Find(s => s != null && s.SlotIndex == index);
            if (slot == null || slot.Spell == null || caster == null)
            {
                return false;
            }

            return caster.BeginCast(slot.Spell);
        }

        public void Assign(int index, SpellCastDefinition spell)
        {
            SpellLoadoutSlot slot = slots.Find(s => s != null && s.SlotIndex == index);
            if (slot == null)
            {
                slot = new SpellLoadoutSlot { SlotIndex = index };
                slots.Add(slot);
            }

            slot.Spell = spell;
            slot.FallbackSpellId = spell != null ? spell.SpellId : "";
        }
    }
}
