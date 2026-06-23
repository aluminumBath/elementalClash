using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Moves the player rig to a world position using the same disable-CharacterController / set / re-enable dance
    /// <see cref="RespawnController"/> uses, so fast travel and leyline warps share one safe path. The rig is
    /// resolved once from the "Player" tag and cached.
    /// </summary>
    public static class RigTeleporter
    {
        private static Transform _rig;

        /// <summary>The player rig transform (the object carrying the CharacterController), or null if not found.</summary>
        public static Transform Rig
        {
            get
            {
                if (_rig != null) return _rig;
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged == null) return null;
                var dmg = tagged.GetComponentInParent<Damageable>();
                _rig = dmg != null ? dmg.transform : tagged.transform;
                return _rig;
            }
        }

        /// <summary>Move the rig to <paramref name="position"/> (rotation unchanged). False if no rig is present.</summary>
        public static bool WarpTo(Vector3 position)
        {
            var rig = Rig;
            if (rig == null) return false;
            var cc = rig.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            rig.position = position;
            if (cc != null) cc.enabled = true;
            return true;
        }
    }
}
