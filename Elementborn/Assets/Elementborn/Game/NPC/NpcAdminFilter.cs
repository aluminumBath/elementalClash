using System;

namespace Elementborn.Game
{
    [Serializable]
    public class NpcAdminFilter
    {
        public string SearchText = "";
        public NpcWorldRole Role = NpcWorldRole.Unknown;
        public string Region = "";
        public string Location = "";
        public string Element = "";
        public bool IncludeUnknownRole = true;

        public bool Matches(NpcWorldEntryDefinition npc)
        {
            if (npc == null)
            {
                return false;
            }

            if (!IncludeUnknownRole && npc.Role == NpcWorldRole.Unknown)
            {
                return false;
            }

            if (Role != NpcWorldRole.Unknown && npc.Role != Role)
            {
                return false;
            }

            if (!Contains(npc.Region, Region))
            {
                return false;
            }

            if (!Contains(npc.LocationName, Location))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Element))
            {
                bool elementMatch = Contains(npc.PrimaryElement, Element) || Contains(npc.SecondaryElement, Element);
                if (!elementMatch)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                bool textMatch =
                    Contains(npc.NpcId, SearchText) ||
                    Contains(npc.DisplayName, SearchText) ||
                    Contains(npc.Aliases, SearchText) ||
                    Contains(npc.TitleOrRank, SearchText) ||
                    Contains(npc.Origin, SearchText) ||
                    Contains(npc.Notes, SearchText) ||
                    Contains(npc.PersonalityNotes, SearchText) ||
                    Contains(npc.RelationshipSummary, SearchText);

                if (!textMatch)
                {
                    return false;
                }
            }

            return true;
        }

        private bool Contains(string source, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(source) &&
                   source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
