using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class NpcVoiceLineDefinition
    {
        public NpcVoiceLineType LineType = NpcVoiceLineType.Greeting;
        public string Subtitle = "";
        public AudioClip PlaceholderClip;
        public string FutureVoiceActorClipName = "";
    }
}
