using System;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Offline, deterministic NPC response generator.
    /// This lets the game support typed/voice conversations before connecting an LLM.
    /// </summary>
    public sealed class RuleBasedNpcResponseProvider : MonoBehaviour, INpcResponseProvider
    {
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
                    onResponse?.Invoke(new NpcConversationResponse
                    {
                        Text = keywordResponse.Response,
                        Intent = "keyword:" + keywordResponse.Keyword,
                        Confidence = 0.9f
                    });
                    return;
                }
            }

            if (ContainsAny(normalized, "hello", "hi", "hey", "greetings"))
            {
                onResponse?.Invoke(new NpcConversationResponse
                {
                    Text = request.Profile.Greeting,
                    Intent = "greeting",
                    Confidence = 0.8f
                });
                return;
            }

            if (ContainsAny(normalized, "where", "map", "lost", "go"))
            {
                onResponse?.Invoke(new NpcConversationResponse
                {
                    Text = string.IsNullOrWhiteSpace(request.Profile.LocalKnowledge)
                        ? request.Profile.UnknownResponse
                        : request.Profile.LocalKnowledge,
                    Intent = "location_help",
                    Confidence = 0.65f
                });
                return;
            }

            onResponse?.Invoke(new NpcConversationResponse
            {
                Text = request.Profile.UnknownResponse,
                Intent = "fallback",
                Confidence = 0.25f
            });
        }

        private static bool ContainsAny(string value, params string[] terms)
        {
            foreach (var term in terms)
            {
                if (value.Contains(term))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
