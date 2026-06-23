using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// An air pocket under water — the Air user's bubble. Inside it you can breathe and you're shielded from
    /// drowning, but you move slowly. Deploy one where it should sit; it can follow the caster and/or expire.
    /// Registered statically so <see cref="UnderwaterController"/> can test membership.
    /// </summary>
    public sealed class BubbleVolume : MonoBehaviour
    {
        private static readonly List<BubbleVolume> All = new List<BubbleVolume>();

        [SerializeField] private float radius = 3f;
        [SerializeField] private float lifeSeconds = 0f; // 0 = permanent
        [SerializeField] private Transform follow;        // optional: stick to the caster

        public float Radius => radius;

        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() => All.Remove(this);

        private void Update()
        {
            if (follow != null) transform.position = follow.position;
            if (lifeSeconds > 0f)
            {
                lifeSeconds -= Time.deltaTime;
                if (lifeSeconds <= 0f) Destroy(gameObject);
            }
        }

        public bool Has(Vector3 p) => (p - transform.position).sqrMagnitude <= radius * radius;

        public static bool Contains(Vector3 p)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i] != null && All[i].Has(p)) return true;
            return false;
        }

        /// <summary>Freeze any bubble caught in a water user's ice — the pocket freezes solid and pops, leaving
        /// its occupants in the ice (trapped and, if they aren't water users, suffocating).</summary>
        public static void FreezeNear(Vector3 p, float radius)
        {
            float r2 = radius * radius;
            for (int i = All.Count - 1; i >= 0; i--)
                if (All[i] != null && (All[i].transform.position - p).sqrMagnitude <= r2)
                    Destroy(All[i].gameObject);
        }

        /// <summary>Deploy a bubble (optionally following a transform, e.g. the caster).</summary>
        public static BubbleVolume Deploy(Vector3 position, float radius, float lifeSeconds, Transform follow = null)
        {
            var go = new GameObject("AirBubble") { transform = { position = position } };
            var b = go.AddComponent<BubbleVolume>();
            b.radius = radius;
            b.lifeSeconds = lifeSeconds;
            b.follow = follow;
            return b;
        }
    }
}
