using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Attach to friendly/civilian NPCs so combat projectiles and melee do not hurt them.</summary>
    [DisallowMultipleComponent]
    public sealed class NpcDamageImmunity : MonoBehaviour
    {
        private Damageable _damageable;

        private void Awake()
        {
            _damageable = GetComponent<Damageable>();
            if (_damageable != null)
            {
                _damageable.DestroyOnDeath = false;
                _damageable.IncomingModifier = Nullify;
            }
        }

        private DamageInfo Nullify(DamageInfo incoming)
            => new DamageInfo(0f, incoming.Source, incoming.Variant);
    }
}
