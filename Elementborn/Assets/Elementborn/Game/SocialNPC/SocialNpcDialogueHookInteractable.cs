using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SocialNpcDialogueHookInteractable : BaseInteractable
    {
        [SerializeField] private SocialNpcDialogueProfileDefinition profile;
        [SerializeField] private string promptPrefix = "Talk to";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = profile != null ? $"{promptPrefix} {profile.DisplayName}" : "Talk";
            return InteractionPromptData.Simple(title, "Talk");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return profile != null;
        }

        public override void Interact(GameObject interactor)
        {
            SpeakCue("");
        }

        public void SetProfile(SocialNpcDialogueProfileDefinition value)
        {
            profile = value;
        }

        public void SpeakCue(string keywordOrCueId)
        {
            if (profile == null)
            {
                return;
            }

            SocialNpcDialogueCue cue = profile.FindCue(keywordOrCueId);
            if (cue == null)
            {
                NotificationFeed.Post($"{profile.DisplayName} has nothing new to say.", NotificationType.Info);
                return;
            }

            NotificationFeed.Post($"{profile.DisplayName}: {cue.Line}", cue.Importance == SocialNpcCueImportance.Quest ? NotificationType.Quest : NotificationType.Info);

            if (!string.IsNullOrWhiteSpace(cue.JournalNote))
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "social_npc_" + PlayerJournalTracker.Safe(profile.NpcId + "_" + cue.CueId),
                    JournalEntryType.Quest,
                    profile.DisplayName + " — " + cue.CueId,
                    cue.JournalNote,
                    profile.Npc != null ? profile.Npc.Region : "Social NPC",
                    profile.NpcId);
            }

            if (cue.QuestToStart != null)
            {
                QuestUiTracker.StartQuest(cue.QuestToStart);
            }
        }
    }
}
