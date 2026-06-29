using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class JournalEntryRecord
    {
        public string EntryId = "";
        public JournalEntryType Type = JournalEntryType.Unknown;
        public string Title = "";
        [TextArea]
        public string Body = "";
        public string Region = "";
        public string RelatedId = "";
        public bool IsNew = true;
        public bool IsPinned = false;
        public bool IsComplete = false;
        public float CreatedAtUnscaledTime;
        public float UpdatedAtUnscaledTime;

        public bool Matches(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            string q = query.ToLowerInvariant();
            return Contains(Title, q)
                || Contains(Body, q)
                || Contains(Region, q)
                || Contains(RelatedId, q)
                || Type.ToString().ToLowerInvariant().Contains(q);
        }

        private static bool Contains(string value, string q)
        {
            return !string.IsNullOrWhiteSpace(value) && value.ToLowerInvariant().Contains(q);
        }
    }
}
