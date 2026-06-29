using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Cheat/admin command bridge for stamina/block/dodge testing.
    ///
    /// Commands:
    /// stamina.fill
    /// stamina.restore amount
    /// stamina.spend amount
    /// defense.block
    /// defense.unblock
    /// defense.dodge
    /// defense.state
    /// </summary>
    public sealed class AdminCombatDefenseCommandBridge : MonoBehaviour
    {
        [SerializeField] private CombatDefenseController targetDefense;
        [SerializeField] private StaminaResource targetStamina;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            ResolveTargets();

            string trimmed = command.Trim();

            if (trimmed == "stamina.fill")
            {
                if (targetStamina != null)
                {
                    targetStamina.Fill();
                }
                return true;
            }

            if (trimmed.StartsWith("stamina.restore "))
            {
                if (targetStamina != null && float.TryParse(trimmed.Substring("stamina.restore ".Length), out float amount))
                {
                    targetStamina.Restore(amount);
                }
                return true;
            }

            if (trimmed.StartsWith("stamina.spend "))
            {
                if (targetStamina != null && float.TryParse(trimmed.Substring("stamina.spend ".Length), out float amount))
                {
                    targetStamina.TrySpend(amount);
                }
                return true;
            }

            if (trimmed == "defense.block")
            {
                if (targetDefense != null)
                {
                    targetDefense.BeginBlock();
                }
                return true;
            }

            if (trimmed == "defense.unblock")
            {
                if (targetDefense != null)
                {
                    targetDefense.EndBlock();
                }
                return true;
            }

            if (trimmed == "defense.dodge")
            {
                if (targetDefense != null)
                {
                    targetDefense.TryDodge(transform.forward);
                }
                return true;
            }

            if (trimmed == "defense.state")
            {
                if (targetDefense != null && targetStamina != null)
                {
                    Debug.Log($"Defense={targetDefense.State}, Stamina={targetStamina.CurrentStamina}/{targetStamina.MaxStamina}");
                }
                return true;
            }

            return false;
        }

        private void ResolveTargets()
        {
            if (targetDefense == null)
            {
                targetDefense = GetComponent<CombatDefenseController>();
            }

            if (targetStamina == null)
            {
                targetStamina = GetComponent<StaminaResource>();
            }

            if (targetDefense != null && targetStamina == null)
            {
                targetStamina = targetDefense.Stamina;
            }
        }
    }
}
