using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Lightweight dialogue/quest hook for data-driven NPCs. This works before full scene dialogue UI exists.
    /// </summary>
    public sealed class NpcDialogueHookInteractable : BaseInteractable
    {
        [SerializeField] private NpcWorldEntryDefinition npc;
        [SerializeField] private NpcConversationProfile conversationProfile;
        [SerializeField] private QuestUiDefinition questToOffer;
        [SerializeField] private bool startQuestOnFirstTalk;
        [SerializeField] private bool setWaypointOnTalk = true;
        [SerializeField] private bool addJournalEntryOnTalk = true;
        [SerializeField] private bool playVoiceOnTalk = true;
        [SerializeField] private bool talkedTo;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = npc != null ? npc.DisplayName : "NPC";
            return InteractionPromptData.Simple(title, talkedTo ? "Speak Again" : "Speak");
        }

        public override void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            string npcName = npc != null ? npc.DisplayName : gameObject.name;
            string greeting = GetGreeting(npcName);
            NotificationFeed.Post($"{npcName}: {greeting}", NotificationType.Dialogue);

            if (playVoiceOnTalk)
            {
                var voice = GetComponent<NpcVoicePlaybackController>();
                if (voice != null)
                {
                    voice.PlayGreeting();
                }
                else if (npc != null)
                {
                    ElementbornAudioService.PlayAt(npc.DefaultVoiceSound, transform.position);
                }
            }

            if (npc != null)
            {
                RegisterNpcMarker(npc);

                if (addJournalEntryOnTalk)
                {
                    AddNpcJournalEntry(npc);
                }

                if (setWaypointOnTalk)
                {
                    WaypointTracker.SetWaypoint(npc.WorldPosition, npc.DisplayName);
                }
            }

            if (questToOffer != null && (startQuestOnFirstTalk || !talkedTo))
            {
                QuestUiTracker.StartQuest(questToOffer);
            }

            talkedTo = true;
            base.Interact(interactor);
        }


        // v54 compatibility alias used by older NPC placement tools.
        public void SetNpc(NpcWorldEntryDefinition value)
        {
            npc = value;
            if (npc != null)
            {
                SetPrompt(npc.DisplayName, "Speak");
            }
        }

        public void Configure(NpcWorldEntryDefinition entry, NpcConversationProfile profile, QuestUiDefinition quest, bool autoStartQuest)
        {
            npc = entry;
            conversationProfile = profile;
            questToOffer = quest;
            startQuestOnFirstTalk = autoStartQuest;
            if (npc != null)
            {
                SetPrompt(npc.DisplayName, "Speak");
            }
        }

        private string GetGreeting(string npcName)
        {
            if (conversationProfile != null && !string.IsNullOrWhiteSpace(conversationProfile.Greeting))
            {
                return conversationProfile.Greeting;
            }

            if (npc == null)
            {
                return "Hello, traveler.";
            }

            if (npc.Role == NpcWorldRole.RoyalFamily)
            {
                return $"Welcome. I am {npcName}. May our people meet you with wisdom and kindness.";
            }

            if (npc.Role == NpcWorldRole.Villain)
            {
                return $"You are far from safety, traveler.";
            }

            return $"Greetings. I am {npcName}.";
        }

        private static void RegisterNpcMarker(NpcWorldEntryDefinition entry)
        {
            MapMarkerType markerType = entry.Role == NpcWorldRole.Villain
                ? MapMarkerType.DangerZone
                : entry.Role == NpcWorldRole.Merchant
                    ? MapMarkerType.VendorNpc
                    : entry.Role == NpcWorldRole.QuestGiver
                        ? MapMarkerType.QuestGiverNpc
                        : MapMarkerType.GuideNpc;

            PlayerMapMarkerTracker.ReportOrUpdateMarker(
                "npc_" + PlayerJournalTracker.Safe(entry.NpcId),
                markerType,
                entry.WorldPosition,
                entry.DisplayName,
                true,
                -1f,
                CreatureTraversalType.Unknown,
                entry.NpcId,
                entry.LocationName,
                false);
        }

        private static void AddNpcJournalEntry(NpcWorldEntryDefinition entry)
        {
            string body =
                $"Title: {entry.TitleOrRank}\n" +
                $"Role: {entry.Role}\n" +
                $"Region: {entry.Region}\n" +
                $"Location: {entry.LocationName}\n" +
                $"Origin: {entry.Origin}\n" +
                $"Elements: {entry.PrimaryElement}{(string.IsNullOrWhiteSpace(entry.SecondaryElement) ? "" : " / " + entry.SecondaryElement)}\n" +
                $"Aliases: {entry.Aliases}\n\n" +
                $"Personality: {entry.PersonalityNotes}\n\n" +
                $"Relationships: {entry.RelationshipSummary}\n\n" +
                entry.Notes;

            PlayerJournalTracker.AddOrUpdateEntry(
                "npc_" + PlayerJournalTracker.Safe(entry.NpcId),
                JournalEntryType.Npc,
                entry.DisplayName,
                body,
                entry.Region,
                entry.NpcId);
        }
    }
}
