using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Combat/Stamina Tuning", fileName = "CombatStaminaTuning")]
    public sealed class CombatStaminaTuning : ScriptableObject
    {
        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float regenPerSecond = 18f;
        [SerializeField] private float regenDelayAfterSpend = 0.75f;

        [Header("Block")]
        [SerializeField] private float blockStartCost = 5f;
        [SerializeField] private float blockDrainPerSecond = 8f;
        [SerializeField] private float blockDamageReductionPercent = 55f;
        [SerializeField] private float perfectBlockWindowSeconds = 0.18f;
        [SerializeField] private float perfectBlockReductionPercent = 100f;
        [SerializeField] private float perfectBlockStaminaRefund = 10f;

        [Header("Dodge")]
        [SerializeField] private float dodgeCost = 25f;
        [SerializeField] private float dodgeDurationSeconds = 0.45f;
        [SerializeField] private float dodgeIFrameSeconds = 0.28f;
        [SerializeField] private float dodgeDistance = 4f;
        [SerializeField] private float dodgeCooldownSeconds = 0.35f;

        [Header("Exhaustion")]
        [SerializeField] private float exhaustedThreshold = 1f;
        [SerializeField] private float exhaustedRecoveryThreshold = 20f;

        public float MaxStamina => Mathf.Max(1f, maxStamina);
        public float RegenPerSecond => Mathf.Max(0f, regenPerSecond);
        public float RegenDelayAfterSpend => Mathf.Max(0f, regenDelayAfterSpend);
        public float BlockStartCost => Mathf.Max(0f, blockStartCost);
        public float BlockDrainPerSecond => Mathf.Max(0f, blockDrainPerSecond);
        public float BlockDamageReductionPercent => Mathf.Clamp(blockDamageReductionPercent, 0f, 100f);
        public float PerfectBlockWindowSeconds => Mathf.Max(0f, perfectBlockWindowSeconds);
        public float PerfectBlockReductionPercent => Mathf.Clamp(perfectBlockReductionPercent, 0f, 100f);
        public float PerfectBlockStaminaRefund => Mathf.Max(0f, perfectBlockStaminaRefund);
        public float DodgeCost => Mathf.Max(0f, dodgeCost);
        public float DodgeDurationSeconds => Mathf.Max(0.05f, dodgeDurationSeconds);
        public float DodgeIFrameSeconds => Mathf.Clamp(dodgeIFrameSeconds, 0f, DodgeDurationSeconds);
        public float DodgeDistance => Mathf.Max(0f, dodgeDistance);
        public float DodgeCooldownSeconds => Mathf.Max(0f, dodgeCooldownSeconds);
        public float ExhaustedThreshold => Mathf.Max(0f, exhaustedThreshold);
        public float ExhaustedRecoveryThreshold => Mathf.Max(ExhaustedThreshold, exhaustedRecoveryThreshold);

        private void OnValidate()
        {
            maxStamina = Mathf.Max(1f, maxStamina);
            regenPerSecond = Mathf.Max(0f, regenPerSecond);
            regenDelayAfterSpend = Mathf.Max(0f, regenDelayAfterSpend);
            blockStartCost = Mathf.Max(0f, blockStartCost);
            blockDrainPerSecond = Mathf.Max(0f, blockDrainPerSecond);
            blockDamageReductionPercent = Mathf.Clamp(blockDamageReductionPercent, 0f, 100f);
            perfectBlockWindowSeconds = Mathf.Max(0f, perfectBlockWindowSeconds);
            perfectBlockReductionPercent = Mathf.Clamp(perfectBlockReductionPercent, 0f, 100f);
            perfectBlockStaminaRefund = Mathf.Max(0f, perfectBlockStaminaRefund);
            dodgeCost = Mathf.Max(0f, dodgeCost);
            dodgeDurationSeconds = Mathf.Max(0.05f, dodgeDurationSeconds);
            dodgeIFrameSeconds = Mathf.Clamp(dodgeIFrameSeconds, 0f, dodgeDurationSeconds);
            dodgeDistance = Mathf.Max(0f, dodgeDistance);
            dodgeCooldownSeconds = Mathf.Max(0f, dodgeCooldownSeconds);
            exhaustedThreshold = Mathf.Max(0f, exhaustedThreshold);
            exhaustedRecoveryThreshold = Mathf.Max(exhaustedThreshold, exhaustedRecoveryThreshold);
        }
    }
}
