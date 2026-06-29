using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Offline NPC response provider that uses:
    /// - NPC profile
    /// - player text intent classification
    /// - dialogue memory facts
    /// - known rumors
    /// - current quest objective
    /// - NPC relationship tone
    ///
    /// Use this before wiring in an online/local LLM.
    /// </summary>
    public sealed class MemoryAwareNpcResponseProvider : MonoBehaviour, INpcResponseProvider
    {
        [SerializeField] private string npcId = "";
        [SerializeField] private int maxMemoryFacts = 3;
        [SerializeField] private bool recordRelationshipChanges = true;

        public void GenerateResponse(NpcConversationRequest request, Action<NpcConversationResponse> onResponse)
        {
            if (request == null || request.Profile == null)
            {
                onResponse?.Invoke(new NpcConversationResponse
                {
                    Text = "…",
                    Intent = "missing_profile",
                    Confidence = 0f
                });
                return;
            }

            DialogueIntent intent = DialogueIntentClassifier.Classify(request.PlayerText);
            string speakerId = string.IsNullOrWhiteSpace(npcId) ? request.Profile.NpcName : npcId;
            var relationship = DialogueMemoryTracker.GetRelationship(speakerId, request.Profile.NpcName);

            if (recordRelationshipChanges)
            {
                DialogueMemoryTracker.RecordNpcInteraction(
                    speakerId,
                    request.Profile.NpcName,
                    intent.ToString(),
                    request.PlayerText);
            }

            string text = BuildResponse(request, intent, relationship);

            onResponse?.Invoke(new NpcConversationResponse
            {
                Text = text,
                Intent = intent.ToString(),
                Confidence = intent == DialogueIntent.Unknown ? 0.4f : 0.75f
            });
        }

        private string BuildResponse(NpcConversationRequest request, DialogueIntent intent, NpcRelationshipState relationship)
        {
            var profile = request.Profile;

            switch (intent)
            {
                case DialogueIntent.Greeting:
                    return profile.Greeting;

                case DialogueIntent.Thanks:
                    return relationship.Trust >= 3
                        ? "You have my thanks as well. Few travelers bother to listen."
                        : "Of course.";

                case DialogueIntent.Goodbye:
                    return "Safe travels.";

                case DialogueIntent.AskRumor:
                    return BuildRumorResponse(profile);

                case DialogueIntent.AskQuest:
                    return BuildQuestResponse(profile);

                case DialogueIntent.AskLocation:
                    return BuildMemoryResponse(request.PlayerText, profile.LocalKnowledge, "That is what I know of the area.");

                case DialogueIntent.AskBoat:
                    return "Raise the sail when the wind is with you. If the wind turns against you, lower the sail and paddle, or you will fight the sea more than ride it.";

                case DialogueIntent.AskCreature:
                    return BuildMemoryResponse("creature mount companion", "Creatures remember how they are treated. The last one you rode should still be marked on your map.", "That is what I know about creatures.");

                case DialogueIntent.AskFaction:
                    return BuildMemoryResponse("faction channeler supremacist unification", "Faction colors are not always worn openly. Listen for how people speak about mixed bloodlines and channelers.", "That is what I know about factions.");

                case DialogueIntent.AskTrade:
                    return "A vendor can help with that. If you have met one, check your map for their marker.";

                case DialogueIntent.AskTraining:
                    return "A trainer is who you need. Shrines may teach you truths, but trainers teach you how to survive them.";

                case DialogueIntent.Threaten:
                    return "Careful. Words can start fights that blades have to finish.";

                case DialogueIntent.AskHelp:
                    return BuildHelpResponse(profile);

                default:
                    return BuildKeywordOrFallback(request);
            }
        }

        private string BuildRumorResponse(NpcConversationProfile profile)
        {
            var rumor = RumorTracker.GetBestRumor();
            if (rumor == null)
            {
                return "I have heard no fresh rumors worth trusting.";
            }

            if (!rumor.IsTrue)
            {
                return $"I heard this, though I would not swear it is true: {rumor.Text}";
            }

            return $"I heard this: {rumor.Text}";
        }

        private string BuildQuestResponse(NpcConversationProfile profile)
        {
            var objectiveTracker = QuestObjectiveTracker.Instance;
            var objective = objectiveTracker != null ? objectiveTracker.GetCurrentObjective() : null;
            if (objective != null && !objective.IsComplete)
            {
                return string.IsNullOrWhiteSpace(objective.Description)
                    ? $"Your current task is: {objective.Title}."
                    : $"Your current task is: {objective.Title}. {objective.Description}";
            }

            return "I do not know your current task, but Kram usually notices when fate starts tugging at someone.";
        }

        private string BuildHelpResponse(NpcConversationProfile profile)
        {
            var important = DialogueMemoryTracker.GetImportantFacts(maxMemoryFacts);
            if (important.Count == 0)
            {
                return string.IsNullOrWhiteSpace(profile.LocalKnowledge)
                    ? profile.UnknownResponse
                    : profile.LocalKnowledge;
            }

            var sb = new StringBuilder();
            sb.Append("Here is what seems important: ");
            for (int i = 0; i < important.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(important[i].Value);
            }

            return sb.ToString();
        }

        private string BuildMemoryResponse(string query, string fallback, string prefix)
        {
            List<DialogueMemoryFact> facts = DialogueMemoryTracker.Search(query, maxMemoryFacts);
            if (facts.Count == 0)
            {
                return fallback;
            }

            var sb = new StringBuilder();
            sb.Append(prefix);
            sb.Append(" ");

            foreach (var fact in facts)
            {
                sb.Append(fact.Value);
                if (!fact.Value.EndsWith("."))
                {
                    sb.Append(".");
                }
                sb.Append(" ");
            }

            return sb.ToString().Trim();
        }

        private string BuildKeywordOrFallback(NpcConversationRequest request)
        {
            string playerText = request.PlayerText ?? string.Empty;
            string normalized = playerText.ToLowerInvariant();

            foreach (var keywordResponse in request.Profile.KeywordResponses)
            {
                if (keywordResponse == null || string.IsNullOrWhiteSpace(keywordResponse.Keyword))
                {
                    continue;
                }

                if (normalized.Contains(keywordResponse.Keyword.ToLowerInvariant()))
                {
                    return keywordResponse.Response;
                }
            }

            return request.Profile.UnknownResponse;
        }
    }
}
