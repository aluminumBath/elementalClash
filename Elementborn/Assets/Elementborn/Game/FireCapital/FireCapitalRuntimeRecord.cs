using System;

namespace Elementborn.Game
{
    [Serializable]
    public class FireCapitalRuntimeRecord
    {
        public string HookId = "";
        public bool Discovered;
        public bool Started;
        public bool Resolved;
        public int TimesStarted;
        public string LastChoice = "";
        public string Notes = "";
    }
}
