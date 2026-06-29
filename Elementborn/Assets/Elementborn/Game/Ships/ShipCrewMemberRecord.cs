using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class ShipCrewMemberRecord
    {
        public NpcWorldEntryDefinition Npc;
        public ShipCrewRole CrewRole = ShipCrewRole.Deckhand;
        public string Station = "";
        [TextArea]
        public string CombatRole = "";
        [TextArea]
        public string PartyRole = "";
        [TextArea]
        public string SecretOrHook = "";
    }
}
