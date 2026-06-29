using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornAudioEventRouter : MonoBehaviour
    {
        private void OnEnable()
        {
            QuestUiEventHub.QuestStarted += HandleQuestStarted;
            QuestUiEventHub.QuestCompleted += HandleQuestCompleted;
            BossEventHub.BossStarted += HandleBossStarted;
            BossEventHub.BossPhaseChanged += HandleBossPhase;
        }

        private void OnDisable()
        {
            QuestUiEventHub.QuestStarted -= HandleQuestStarted;
            QuestUiEventHub.QuestCompleted -= HandleQuestCompleted;
            BossEventHub.BossStarted -= HandleBossStarted;
            BossEventHub.BossPhaseChanged -= HandleBossPhase;
        }

        private void HandleQuestStarted(QuestUiRecord quest)
        {
            ElementbornAudioService.Play(ElementbornSoundEventId.QuestStart);
        }

        private void HandleQuestCompleted(QuestUiRecord quest)
        {
            ElementbornAudioService.Play(ElementbornSoundEventId.QuestComplete);
        }

        private void HandleBossStarted(BossController boss)
        {
            if (boss != null)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.BossAwaken, boss.transform.position);
            }
            else
            {
                ElementbornAudioService.Play(ElementbornSoundEventId.BossAwaken);
            }
        }

        private void HandleBossPhase(BossController boss, int phase)
        {
            if (boss != null)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.BossPhase, boss.transform.position);
            }
            else
            {
                ElementbornAudioService.Play(ElementbornSoundEventId.BossPhase);
            }
        }

        public void PlayUiConfirm() => ElementbornAudioService.Play(ElementbornSoundEventId.UiConfirm);
        public void PlayUiCancel() => ElementbornAudioService.Play(ElementbornSoundEventId.UiCancel);
        public void PlayUiTick() => ElementbornAudioService.Play(ElementbornSoundEventId.UiTick);
    }
}
