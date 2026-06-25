using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>A wall/stele inscription that surfaces its line once, the first time the player draws near — an
    /// atmospheric beat on the approach into a site. Distance-based (no collider, no interaction prompt), so it
    /// reads itself as you walk past. Configured and placed by <see cref="SiteInteriorController"/>.</summary>
    public sealed class ApproachInscription : MonoBehaviour
    {
        private string _line;
        private float _radius = 4f;
        private bool _shown;

        public void Configure(string line, float radius = 4f) { _line = line; _radius = radius; }

        private void Update()
        {
            if (_shown || string.IsNullOrEmpty(_line)) return;
            var rig = RigTeleporter.Rig;
            if (rig == null) return;
            if (Vector3.SqrMagnitude(rig.position - transform.position) <= _radius * _radius)
            {
                _shown = true;
                GameHud.Instance?.Toast(_line);
            }
        }
    }
}
