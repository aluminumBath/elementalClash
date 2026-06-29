using System;

namespace Elementborn.Game
{
    public interface INpcResponseProvider
    {
        void GenerateResponse(NpcConversationRequest request, Action<NpcConversationResponse> onResponse);
    }
}
