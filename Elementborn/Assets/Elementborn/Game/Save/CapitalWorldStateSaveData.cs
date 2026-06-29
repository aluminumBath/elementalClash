using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class CapitalWorldStateSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<CapitalRuntimeState> RuntimeStates = new List<CapitalRuntimeState>();
    }
}
