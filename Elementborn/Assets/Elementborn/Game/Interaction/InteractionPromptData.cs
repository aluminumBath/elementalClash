using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public struct InteractionPromptData
    {
        public string Title;
        public string ActionText;
        public Sprite Icon;
        public float HoldSeconds;
        public bool RequiresHold;

        public static InteractionPromptData Simple(string title, string actionText)
        {
            return new InteractionPromptData
            {
                Title = title,
                ActionText = actionText,
                HoldSeconds = 0f,
                RequiresHold = false
            };
        }
    }
}
