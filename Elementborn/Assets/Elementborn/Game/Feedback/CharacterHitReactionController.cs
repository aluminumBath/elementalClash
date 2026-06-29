using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CharacterHitReactionController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string criticalTrigger = "CriticalHit";
        [SerializeField] private string blockTrigger = "Block";
        [SerializeField] private string dodgeTrigger = "Dodge";

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        public void React(HitFeedbackType type)
        {
            if (animator == null)
            {
                return;
            }

            string trigger = hitTrigger;
            if (type == HitFeedbackType.CriticalHit) trigger = criticalTrigger;
            if (type == HitFeedbackType.Block || type == HitFeedbackType.PerfectBlock) trigger = blockTrigger;
            if (type == HitFeedbackType.Dodge) trigger = dodgeTrigger;

            if (!string.IsNullOrWhiteSpace(trigger))
            {
                animator.SetTrigger(trigger);
            }
        }
    }
}
