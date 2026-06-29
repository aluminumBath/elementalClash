using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EnemyWeaknessAwareSelector : MonoBehaviour
    {
        [SerializeField] private List<CombatAttackDefinition> candidateAttacks = new List<CombatAttackDefinition>();

        public CombatAttackDefinition SelectBestAttack(GameObject target, AbilityElementType fallbackElement)
        {
            if (candidateAttacks.Count == 0)
            {
                return null;
            }

            CombatResistanceProfile resistance = target != null ? target.GetComponent<CombatResistanceProfile>() : null;
            CombatAttackDefinition best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var attack in candidateAttacks)
            {
                if (attack == null)
                {
                    continue;
                }

                float score = attack.BaseDamage;
                if (resistance != null)
                {
                    score *= 1f - resistance.GetPercent(attack.Element) / 100f;
                }

                if (attack.Element == fallbackElement)
                {
                    score += 1f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = attack;
                }
            }

            return best;
        }
    }
}
