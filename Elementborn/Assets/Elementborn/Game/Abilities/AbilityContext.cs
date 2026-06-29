using UnityEngine;

namespace Elementborn.Game
{
    public readonly struct AbilityContext
    {
        public readonly GameObject User;
        public readonly Transform AimTransform;
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;

        public AbilityContext(GameObject user, Transform aimTransform, Vector3 origin, Vector3 direction)
        {
            User = user;
            AimTransform = aimTransform;
            Origin = origin;
            Direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
        }

        public static AbilityContext From(GameObject user, Transform aimTransform = null)
        {
            Transform t = aimTransform != null ? aimTransform : user != null ? user.transform : null;
            Vector3 origin = t != null ? t.position : Vector3.zero;
            Vector3 direction = t != null ? t.forward : Vector3.forward;
            return new AbilityContext(user, t, origin, direction);
        }
    }
}
