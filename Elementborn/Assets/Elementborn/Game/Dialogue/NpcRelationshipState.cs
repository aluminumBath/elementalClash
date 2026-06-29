using System;

namespace Elementborn.Game
{
    [Serializable]
    public class NpcRelationshipState
    {
        public string NpcId = "";
        public string DisplayName = "";
        public int Trust = 0;
        public int Fear = 0;
        public int Respect = 0;
        public int Annoyance = 0;
        public string LastTopic = "";
        public string LastPlayerStatement = "";

        public string GetToneHint()
        {
            if (Trust >= 5)
            {
                return "warm and trusting";
            }

            if (Annoyance >= 5)
            {
                return "short and irritated";
            }

            if (Fear >= 5)
            {
                return "nervous and cautious";
            }

            if (Respect >= 5)
            {
                return "respectful";
            }

            return "neutral";
        }
    }
}
