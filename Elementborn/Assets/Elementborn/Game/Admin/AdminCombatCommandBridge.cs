using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AdminCombatCommandBridge : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private StatusEffectDefinition burn;
        [SerializeField] private StatusEffectDefinition wet;
        [SerializeField] private StatusEffectDefinition chilled;
        [SerializeField] private StatusEffectDefinition rooted;
        [SerializeField] private StatusEffectDefinition shocked;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;
            string trimmed = command.Trim();
            GameObject tgt = target != null ? target : gameObject;

            if (trimmed.StartsWith("combat.damage "))
            {
                string payload = trimmed.Substring("combat.damage ".Length).Trim();
                string[] parts = payload.Split('|');
                float amount = 10f;
                float.TryParse(parts[0], out amount);
                AbilityElementType element = AbilityElementType.Neutral;
                if (parts.Length > 1) System.Enum.TryParse(parts[1], true, out element);
                CombatDamageUtility.ApplyRawDamage(tgt, amount, element, gameObject, "Admin", false);
                DamageNumberSpawner.Spawn(tgt.transform.position, amount, element, false);
                return true;
            }

            if (trimmed.StartsWith("combat.heal "))
            {
                string payload = trimmed.Substring("combat.heal ".Length).Trim();
                float amount = 10f;
                float.TryParse(payload, out amount);
                var hp = tgt.GetComponent<SimpleCombatHealth>();
                if (hp != null) hp.Heal(amount);
                return true;
            }

            if (trimmed.StartsWith("combat.status "))
            {
                string key = trimmed.Substring("combat.status ".Length).Trim().ToLowerInvariant();
                var status = tgt.GetComponent<StatusEffectController>() ?? tgt.AddComponent<StatusEffectController>();
                StatusEffectDefinition def = key switch
                {
                    "burn" => burn,
                    "wet" => wet,
                    "chilled" => chilled,
                    "rooted" => rooted,
                    "shocked" => shocked,
                    _ => null
                };
                if (def != null) status.Apply(def);
                return true;
            }

            if (trimmed.StartsWith("combat.clearstatus "))
            {
                string key = trimmed.Substring("combat.clearstatus ".Length).Trim();
                var status = tgt.GetComponent<StatusEffectController>();
                if (status != null && System.Enum.TryParse(key, true, out StatusEffectType type)) status.Remove(type);
                return true;
            }

            if (trimmed == "combat.kill")
            {
                var hp = tgt.GetComponent<SimpleCombatHealth>();
                if (hp != null) hp.ApplyDamage(hp.CurrentHealth + 9999f);
                return true;
            }

            if (trimmed == "combat.drop")
            {
                var loot = tgt.GetComponent<EnemyLootDropOnDefeat>();
                if (loot != null) loot.NotifyDefeated();
                return true;
            }

            return false;
        }
    }
}
