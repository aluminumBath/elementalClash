using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Tracks how the player treats their creatures and enforces Kiana's law. Kindness (feeding a Heartfruit,
    /// say) raises the care score; <see cref="MistreatCreature"/> lowers it. If it falls to "abused," Kiana
    /// confiscates one of the player's creatures (it must be tamed again from scratch via
    /// <see cref="PlayerInventory.RemoveOwned"/>) and bars them from the capital for a day (<see cref="IsBanned"/>).
    /// Put on the player rig. The concrete abuse triggers (striking your own creature, prolonged neglect) call
    /// <see cref="MistreatCreature"/>; feeding wires kindness in automatically.
    /// </summary>
    public sealed class CreatureCareController : MonoBehaviour
    {
        [SerializeField] private float banHours = 24f;

        private readonly CareTracker _care = new CareTracker();
        private float _bannedUntil = -1f;

        public CareVerdict Verdict => _care.Verdict;
        public bool IsBanned => Time.time < _bannedUntil;
        public float BanHoursRemaining => IsBanned ? Mathf.Max(0f, (_bannedUntil - Time.time) / 3600f) : 0f;

        public void CareForCreature() => _care.CareFor();

        public void MistreatCreature()
        {
            _care.Mistreat();
            if (_care.Verdict == CareVerdict.Abused) Enforce();
        }

        /// <summary>Kiana takes a creature and bans the player. Public so a Kiana NPC can invoke it directly.</summary>
        public void Enforce()
        {
            var inv = PlayerInventory.Instance;
            if (inv != null)
            {
                var owned = new List<CreatureKind>(inv.Owned); // copy: RemoveOwned mutates the set
                if (owned.Count > 0) inv.RemoveOwned(owned[0]);
            }
            _bannedUntil = Time.time + banHours * 3600f;
            Debug.Log("[Kiana] Your creature is taken, and you are barred from the capital for a day. Treat them better.");
        }
    }
}
