using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class SocialNpcRelationshipRecord
    {
        public NpcWorldEntryDefinition Source;
        public NpcWorldEntryDefinition Target;
        public SocialRelationshipType RelationshipType = SocialRelationshipType.Unknown;
        [Range(-100, 100)]
        public int Affinity = 0;
        [TextArea]
        public string Notes = "";
    }
}
