using System;

namespace Elementborn.Game
{
    [Serializable]
    public class SocialGroupRuntimeRecord
    {
        public string GroupId = "";
        public string LastEventId = "";
        public int TimesActivated = 0;
        public int NeighborhoodTrust = 0;
        public int ChaosLevel = 0;
        public string Notes = "";
    }
}
