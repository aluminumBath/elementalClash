using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureCombatHook : MonoBehaviour
    {
        [SerializeField] private float damageMultiplier = 1f;
        public float DamageMultiplier => Mathf.Max(0.1f, damageMultiplier);
    }
}
