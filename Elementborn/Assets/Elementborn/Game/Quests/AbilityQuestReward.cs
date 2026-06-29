using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Attach to quest-completion triggers or call Grant() from admin/quest logic.
    /// </summary>
    public sealed class AbilityQuestReward : MonoBehaviour
    {
        [SerializeField] private AbilityDefinition ability;
        [SerializeField] private int bonusSkillPoints;

        public void Grant()
        {
            if (bonusSkillPoints != 0)
            {
                PlayerAbilityTracker.AddSkillPoints(bonusSkillPoints, "Quest reward");
            }

            if (ability != null)
            {
                PlayerAbilityTracker.Unlock(ability, AbilityUnlockSource.QuestReward, spendSkillPoints: false);
            }
        }
    }
}
