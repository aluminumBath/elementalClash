using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class WindCapitalRumorDefinition
    {
        public string RumorId = "";
        public string DisplayText = "";
        public WindCapitalHookType HookType = WindCapitalHookType.Unknown;
        public bool PubliclyKnown = true;
        [TextArea]
        public string HiddenTruth = "";
    }
}
