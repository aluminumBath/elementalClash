using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class BossLitePhaseController : MonoBehaviour
    {
        [SerializeField] private SimpleCombatHealth health;
        [SerializeField] private EnemyCombatBrain brain;
        [SerializeField] private EnemyWeaknessAwareSelector selector;
        [SerializeField] private List<BossLitePhaseDefinition> phases = new List<BossLitePhaseDefinition>();

        private int currentPhaseIndex = -1;

        private void Awake()
        {
            if (health == null) health = GetComponent<SimpleCombatHealth>();
            if (brain == null) brain = GetComponent<EnemyCombatBrain>();
            if (selector == null) selector = GetComponent<EnemyWeaknessAwareSelector>();
        }

        private void Update()
        {
            if (health == null || health.MaxHealth <= 0f)
            {
                return;
            }

            float healthPercent = health.CurrentHealth / health.MaxHealth;
            int nextPhase = currentPhaseIndex;

            for (int i = 0; i < phases.Count; i++)
            {
                if (phases[i] != null && healthPercent <= phases[i].StartAtHealthPercent)
                {
                    nextPhase = i;
                }
            }

            if (nextPhase != currentPhaseIndex && nextPhase >= 0 && nextPhase < phases.Count)
            {
                ApplyPhase(nextPhase);
            }
        }

        private void ApplyPhase(int index)
        {
            currentPhaseIndex = index;
            BossLitePhaseDefinition phase = phases[index];
            if (phase == null)
            {
                return;
            }

            if (brain != null && phase.ProfileOverride != null)
            {
                brain.SetProfile(phase.ProfileOverride);
            }

            if (phase.SelfBuffStatus != null)
            {
                StatusEffectController status = GetComponent<StatusEffectController>();
                if (status == null) status = gameObject.AddComponent<StatusEffectController>();
                status.Apply(phase.SelfBuffStatus);
            }

            if (!string.IsNullOrWhiteSpace(phase.PhaseAnnouncement))
            {
                NotificationFeed.Post(phase.PhaseAnnouncement, NotificationType.Combat);
            }
        }
    }
}
