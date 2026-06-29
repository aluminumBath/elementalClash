using UnityEngine;

namespace Elementborn.Game
{
    public readonly struct SpellCastRequest
    {
        public readonly SpellCastDefinition Spell;
        public readonly GameObject Target;
        public readonly Vector3 GroundPoint;

        public SpellCastRequest(SpellCastDefinition spell, GameObject target, Vector3 groundPoint)
        {
            Spell = spell;
            Target = target;
            GroundPoint = groundPoint;
        }
    }
}
