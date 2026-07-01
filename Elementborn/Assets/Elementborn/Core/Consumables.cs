using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>What using a consumable does: an amount of health to heal, whether it refills stamina, and how
    /// many seconds of temporary underwater breathing it grants (0 for none).</summary>
    public readonly struct ConsumableEffect
    {
        public readonly int Heal;
        public readonly bool RefillStamina;
        public readonly float WaterBreathingSeconds;
        public ConsumableEffect(int heal, bool refillStamina, float waterBreathingSeconds = 0f)
        {
            Heal = heal;
            RefillStamina = refillStamina;
            WaterBreathingSeconds = waterBreathingSeconds;
        }
    }

    /// <summary>
    /// The effect each usable consumable applies when used from the inventory. Pure data + lookup, unit-tested;
    /// the runtime (the inventory screen) applies the effect to the player's Health / stamina. Only items listed
    /// here are "usable"; foods are eaten via sidekick feeding, and other items aren't consumable.
    /// </summary>
    public static class Consumables
    {
        private static readonly Dictionary<string, ConsumableEffect> Effects = new Dictionary<string, ConsumableEffect>
        {
            { "healing_tonic",   new ConsumableEffect(40, false) },
            { "stamina_draught", new ConsumableEffect(0, true) },
            { "elixir_of_vigor", new ConsumableEffect(70, true) },
            { "tideglass_draught", new ConsumableEffect(0, false, 45f) },
        };

        public static bool IsConsumable(string itemId) => itemId != null && Effects.ContainsKey(itemId);

        public static bool TryGet(string itemId, out ConsumableEffect effect)
        {
            if (itemId != null && Effects.TryGetValue(itemId, out effect)) return true;
            effect = default;
            return false;
        }
    }
}
