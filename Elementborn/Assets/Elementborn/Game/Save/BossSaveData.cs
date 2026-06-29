using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable] public class BossSaveFile { public int Version = 1; public int SlotIndex = 0; public List<BossRuntimeRecord> Bosses = new List<BossRuntimeRecord>(); }
}
