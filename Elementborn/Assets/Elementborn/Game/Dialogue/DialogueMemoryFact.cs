using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class DialogueMemoryFact
    {
        public string FactId = "";
        public DialogueMemoryType Type = DialogueMemoryType.Unknown;
        public string Subject = "";
        public string Value = "";
        public string Source = "";
        public string Region = "";
        public string RelatedQuestId = "";
        public bool Important = false;
        public bool PlayerKnows = true;
        public float CreatedAtUnscaledTime;
        public float LastMentionedAtUnscaledTime;

        public bool Matches(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            string q = query.ToLowerInvariant();
            return Contains(Subject, q)
                || Contains(Value, q)
                || Contains(Source, q)
                || Contains(Region, q)
                || Contains(RelatedQuestId, q);
        }

        private static bool Contains(string value, string q)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.ToLowerInvariant().Contains(q);
        }
    }
}
