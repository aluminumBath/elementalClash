using System;

namespace Elementborn.Game
{
    [Serializable]
    public class SpellLoadoutSlot
    {
        public int SlotIndex = 0;
        public SpellCastDefinition Spell;
        public string FallbackSpellId = "";

        public string SpellId => Spell != null ? Spell.SpellId : FallbackSpellId;
    }
}
