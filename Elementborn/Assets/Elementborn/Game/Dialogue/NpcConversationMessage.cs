using System;

namespace Elementborn.Game
{
    [Serializable]
    public struct NpcConversationMessage
    {
        public string Speaker;
        public string Text;

        public NpcConversationMessage(string speaker, string text)
        {
            Speaker = speaker;
            Text = text;
        }
    }
}
