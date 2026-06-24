using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Flavour: a sidekick that bobs in orbit around its NPC — Parfa's two bickering frogs, or any hovering
    /// companion. Put on the sidekick object; it orbits its parent (or an assigned anchor). Two of these with
    /// different <see cref="phaseDegrees"/> give the pair their circling, squabbling look. Placeholder mesh;
    /// the real critter is an artist's job.
    /// </summary>
    public sealed class NpcSidekickOrbiter : MonoBehaviour
    {
        [SerializeField] private Transform anchor;
        [SerializeField] private float radius = 0.8f;
        [SerializeField] private float degreesPerSecond = 70f;
        [SerializeField] private float height = 1.7f;
        [SerializeField] private float bob = 0.15f;
        [SerializeField] private float phaseDegrees = 0f;
        [Header("Optional model")]
        [SerializeField] private bool useSidekickModel = false;
        [SerializeField] private WillowSidekick sidekick = WillowSidekick.Gunnar;

        private float _angle;

        private void Awake()
        {
            if (anchor == null && transform.parent != null) anchor = transform.parent;
            _angle = phaseDegrees;
            if (useSidekickModel)
                ModelLibrary.Attach(SidekickModelNames.ResourcePath(sidekick), gameObject, "Sidekick");
        }

        private void Update()
        {
            if (anchor == null) return;
            _angle += degreesPerSecond * Time.deltaTime;
            float r = _angle * Mathf.Deg2Rad;
            Vector3 center = anchor.position + Vector3.up * height;
            transform.position = center
                + new Vector3(Mathf.Cos(r), 0f, Mathf.Sin(r)) * radius
                + Vector3.up * (Mathf.Sin(r * 2f) * bob);
        }
    }
}
