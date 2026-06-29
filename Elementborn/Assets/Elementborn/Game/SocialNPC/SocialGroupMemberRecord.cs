using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class SocialGroupMemberRecord
    {
        public NpcWorldEntryDefinition Npc;
        public string UsualRole = "";
        public Vector3 DefaultPosition;
        [TextArea]
        public string AmbientBehavior = "";
    }
}
