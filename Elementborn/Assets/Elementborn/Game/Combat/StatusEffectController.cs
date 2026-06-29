using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class StatusEffectController : MonoBehaviour
    {
        [System.Serializable]
        private sealed class ActiveStatus
        {
            public StatusEffectDefinition Definition;
            public float Remaining;
            public float TickTimer;
        }

        [SerializeField] private List<ActiveStatus> active = new List<ActiveStatus>();

        public bool HasStatus(StatusEffectType type)
        {
            foreach (var s in active) if (s != null && s.Definition != null && s.Definition.EffectType == type) return true;
            return false;
        }

        public void Apply(StatusEffectDefinition definition)
        {
            if (definition == null || definition.EffectType == StatusEffectType.None) return;

            if (definition.UniquePerTarget)
            {
                foreach (var s in active)
                {
                    if (s != null && s.Definition != null && s.Definition.EffectType == definition.EffectType)
                    {
                        s.Definition = definition;
                        s.Remaining = definition.DurationSeconds;
                        s.TickTimer = definition.TickIntervalSeconds;
                        NotificationFeed.Post($"{gameObject.name} refreshed {definition.DisplayName}.", NotificationType.Info);
                        return;
                    }
                }
            }

            active.Add(new ActiveStatus
            {
                Definition = definition,
                Remaining = definition.DurationSeconds,
                TickTimer = definition.TickIntervalSeconds
            });
            NotificationFeed.Post($"{gameObject.name} affected by {definition.DisplayName}.", NotificationType.Info);
        }

        public void Remove(StatusEffectType type)
        {
            active.RemoveAll(s => s == null || s.Definition == null || s.Definition.EffectType == type);
        }

        public float GetMoveSpeedMultiplier()
        {
            float mult = 1f;
            foreach (var s in active) if (s != null && s.Definition != null) mult *= 1f + s.Definition.MoveSpeedPercentDelta / 100f;
            return Mathf.Max(0f, mult);
        }

        public float GetAttackMultiplier()
        {
            float mult = 1f;
            foreach (var s in active) if (s != null && s.Definition != null) mult *= 1f + s.Definition.AttackPowerPercentDelta / 100f;
            return Mathf.Max(0f, mult);
        }

        public float GetDefenseMultiplier()
        {
            float mult = 1f;
            foreach (var s in active) if (s != null && s.Definition != null) mult *= 1f + s.Definition.DefensePercentDelta / 100f;
            return Mathf.Max(0f, mult);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var s = active[i];
                if (s == null || s.Definition == null) { active.RemoveAt(i); continue; }
                s.Remaining -= dt;
                s.TickTimer -= dt;
                if (s.Definition.TickDamage > 0f && s.TickTimer <= 0f)
                {
                    s.TickTimer = s.Definition.TickIntervalSeconds;
                    CombatDamageUtility.ApplyRawDamage(gameObject, s.Definition.TickDamage, s.Definition.Element, null, "StatusTick", false);
                }
                if (s.Remaining <= 0f) active.RemoveAt(i);
            }
        }
    }
}
