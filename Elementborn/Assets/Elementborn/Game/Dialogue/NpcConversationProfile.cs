using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/NPC Conversation Profile", fileName = "NpcConversationProfile")]
    public sealed class NpcConversationProfile : ScriptableObject
    {
        [Header("Identity")]
        public string NpcName = "NPC";
        [TextArea]
        public string Role = "A person in Elementborn.";
        [TextArea]
        public string Personality = "Helpful, grounded, and brief.";

        [Header("World Knowledge")]
        [TextArea]
        public string LocalKnowledge = "They know the nearby area.";
        [TextArea]
        public string Boundaries = "They should not claim to know things outside their role.";

        [Header("Fallbacks")]
        [TextArea]
        public string Greeting = "Hello, traveler.";
        [TextArea]
        public string UnknownResponse = "I'm not sure about that, but I can tell you what I know.";

        [Header("Keyword Responses")]
        public List<NpcKeywordResponse> KeywordResponses = new List<NpcKeywordResponse>();
    }

    [Serializable]
    public sealed class NpcKeywordResponse
    {
        public string Keyword = "";
        [TextArea]
        public string Response = "";
    }
}
