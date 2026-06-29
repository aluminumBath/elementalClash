using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AttackAnimationBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string lightAttackTrigger = "LightAttack";
        [SerializeField] private string heavyAttackTrigger = "HeavyAttack";
        [SerializeField] private string castTrigger = "Cast";
        [SerializeField] private string blockBool = "Blocking";

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        public void PlayLightAttack()
        {
            Trigger(lightAttackTrigger);
        }

        public void PlayHeavyAttack()
        {
            Trigger(heavyAttackTrigger);
        }

        public void PlayCast()
        {
            Trigger(castTrigger);
        }

        public void SetBlocking(bool blocking)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(blockBool))
            {
                animator.SetBool(blockBool, blocking);
            }
        }

        private void Trigger(string trigger)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(trigger))
            {
                animator.SetTrigger(trigger);
            }
        }
    }
}
