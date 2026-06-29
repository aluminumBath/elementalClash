using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Cheat/admin command bridge for spells.
    ///
    /// Commands:
    /// spell.cast slotIndex
    /// spell.cooldowns.clear
    /// spell.resource.fill
    /// spell.resource.restore amount
    /// spell.interrupt
    /// spell.state
    /// </summary>
    public sealed class AdminSpellCommandBridge : MonoBehaviour
    {
        [SerializeField] private SpellCastController caster;
        [SerializeField] private SpellLoadoutController loadout;
        [SerializeField] private SpellResourcePool resourcePool;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            ResolveTargets();
            string trimmed = command.Trim();

            if (trimmed.StartsWith("spell.cast "))
            {
                string value = trimmed.Substring("spell.cast ".Length).Trim();
                if (int.TryParse(value, out int slot) && loadout != null)
                {
                    loadout.CastSlot(slot);
                }
                return true;
            }

            if (trimmed == "spell.cooldowns.clear")
            {
                SpellCooldownTracker.ClearAll();
                NotificationFeed.Post("Spell cooldowns cleared.", NotificationType.Info);
                return true;
            }

            if (trimmed == "spell.resource.fill")
            {
                if (resourcePool != null)
                {
                    resourcePool.Fill();
                }
                return true;
            }

            if (trimmed.StartsWith("spell.resource.restore "))
            {
                string value = trimmed.Substring("spell.resource.restore ".Length).Trim();
                if (resourcePool != null && float.TryParse(value, out float amount))
                {
                    resourcePool.Restore(amount);
                }
                return true;
            }

            if (trimmed == "spell.interrupt")
            {
                if (caster != null)
                {
                    caster.Interrupt();
                }
                return true;
            }

            if (trimmed == "spell.state")
            {
                if (caster != null)
                {
                    Debug.Log($"Spell state={caster.State}, current={(caster.CurrentSpell != null ? caster.CurrentSpell.DisplayName : "(none)")}");
                }
                return true;
            }

            return false;
        }

        private void ResolveTargets()
        {
            if (caster == null) caster = GetComponent<SpellCastController>();
            if (loadout == null) loadout = GetComponent<SpellLoadoutController>();
            if (resourcePool == null) resourcePool = GetComponent<SpellResourcePool>();
        }
    }
}
