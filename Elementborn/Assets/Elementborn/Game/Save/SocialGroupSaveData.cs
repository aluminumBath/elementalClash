using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class SocialGroupSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<SocialGroupRuntimeRecord> RuntimeRecords = new List<SocialGroupRuntimeRecord>();
    }
}
