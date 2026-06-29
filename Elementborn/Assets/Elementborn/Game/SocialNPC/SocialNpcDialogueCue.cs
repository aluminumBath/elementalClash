using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class SocialNpcDialogueCue
    {
        public string CueId = "";
        public string TriggerKeyword = "";
        [TextArea]
        public string Line = "";
        [TextArea]
        public string JournalNote = "";
        public QuestUiDefinition QuestToStart;
        public SocialNpcCueImportance Importance = SocialNpcCueImportance.Flavor;
    }
}
