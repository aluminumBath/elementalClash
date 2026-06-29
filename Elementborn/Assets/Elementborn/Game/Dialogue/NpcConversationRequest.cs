using System.Collections.Generic;

namespace Elementborn.Game
{
    public sealed class NpcConversationRequest
    {
        public NpcConversationProfile Profile;
        public string PlayerText;
        public List<NpcConversationMessage> History = new List<NpcConversationMessage>();
    }

    public sealed class NpcConversationResponse
    {
        public string Text;
        public string Intent;
        public float Confidence;
    }
}
